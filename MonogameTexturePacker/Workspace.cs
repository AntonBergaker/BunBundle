using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonogameTexturePacker {
    public class Workspace {
        private string basePath;
        private WorkspaceFolder selectedFolder;
        private string targetFolder;

        public WorkspaceFolder RootFolder { protected set; get; }
        private string SaveFilePath => Path.Combine(RootFolder.path, "sprites.sprm");

        public Workspace() {
        }


        private Sprite[] GetAllSprites() {
            return GetAllSprites(RootFolder).ToArray();
        }

        private IEnumerable<Sprite> GetAllSprites(WorkspaceFolder rootFolder) {
            IEnumerable<Sprite> sprites = rootFolder.files;
            foreach (WorkspaceFolder folder in rootFolder.subFolders) {
                sprites = sprites.Concat(GetAllSprites(folder));
            }

            return sprites;
        }

        private (bool hasChild, WorkspaceFolder folder) GetWorkspaceFolder(string path) {
            bool hasChild = false;
            List<WorkspaceFolder> subFolders = new List<WorkspaceFolder>();
            List<Sprite> files = new List<Sprite>();
            foreach (string dir in Directory.GetDirectories(path)) {
                if (Directory.GetFiles(dir, "*.spr").Length > 0) {
                    hasChild = true;
                    files.Add(new Sprite(Path.GetFileName(dir), dir));
                    continue;
                }

                (bool subHasChild, WorkspaceFolder sub) = GetWorkspaceFolder(dir);
                if (subHasChild) {
                    hasChild = true;
                    subFolders.Add(sub);
                }
            }

            return (hasChild, hasChild ? new WorkspaceFolder(Path.GetFileName(path), path, subFolders, files) : null);
        }

        public void OpenFolder(string folder) {
            string[] files = Directory.GetFiles(folder, "sprites.sprm");
            if (files.Length >= 1) {
                OpenFile(files[0]);
            }

            (_, RootFolder) = GetWorkspaceFolder(folder);

            if (RootFolder == null) {
                RootFolder = new WorkspaceFolder(Path.GetFileName(folder), folder, new List<WorkspaceFolder>(), new List<Sprite>());
            }
        }

        public void OpenFile(string file) {
            JObject obj = JObject.Parse(File.ReadAllText(file));
            targetFolder = (string)obj["targetDirectory"];
        }


        public void ImportSprites(string[] paths) {
            // Copy the file into the folder

            WorkspaceFolder pasteTarget = selectedFolder ?? RootFolder;

            string newName = Path.GetFileNameWithoutExtension(paths[0]);

            Sprite spr = Sprite.Create(newName, paths, pasteTarget.path);

            pasteTarget.files.Add(spr);

            RaiseImportSprite(pasteTarget, spr);
        }

        public void Build() {

            Sprite[] allSprites = GetAllSprites();
            string fullTargetFolder = Path.Combine(RootFolder.path, targetFolder);

            string cacheFolder = Path.GetFullPath(Path.Combine(RootFolder.path, "..", "__build_cache"));

            if (Directory.Exists(cacheFolder) == false) {
                Directory.CreateDirectory(cacheFolder);
            }

            string[] mips = { "mip0", "mip1", "mip2", "mip3", "mip4" };
            string[] sizes = { "100%", "50%", "25%", "12.5%", "6.25%" };

            foreach (string mip in mips) {
                string fullPath = Path.Combine(cacheFolder, mip);
                if (Directory.Exists(fullPath) == false) {
                    Directory.CreateDirectory(fullPath);
                }
            }

            // Move all sprites to mip0
            foreach (Sprite spr in allSprites) {
                spr.ImageAbsolutePaths.Each(x =>
                    File.Copy(
                        x,
                        Path.Combine(cacheFolder, mips[0], Path.GetFileName(x)), true
                    )
                );
            }

            // Resize the sprites
            ProcessStartInfo resizeInfo = new ProcessStartInfo("\"C:\\Program Files\\ImageMagick-7.0.9-Q16\\magick.exe\"");
            resizeInfo.WorkingDirectory = RootFolder.path;
            resizeInfo.UseShellExecute = true;
            resizeInfo.WindowStyle = ProcessWindowStyle.Hidden;
            for (int i = 1; i < mips.Length; i++) {
                resizeInfo.Arguments =
                    $"{Path.Combine(cacheFolder, mips[0], "*.png")} -resize {sizes[i]} -set filename:original %t {cacheFolder}\\{mips[i]}\\%[filename:original].png";
                Process process = Process.Start(resizeInfo);
                process.WaitForExit();
            }


            // Run the MGCB
            string contentPath = Path.Combine(fullTargetFolder, "Content", "TempContent.mgcb");
            Console.WriteLine("NEW PATH: " + contentPath);
            string defaultProperties = string.Join("\r\n",
                "/outputDir:bin/DesktopGL/Content",
                "/intermediateDir:obj/Windows",
                "/platform:Windows",
                "/config:",
                "/profile:Reach",
                "/compress:False",
                "/rebuild"
            );

            // Build the file
            File.WriteAllText(contentPath, defaultProperties + string.Join("", allSprites.Select(x => x.GeneratePackingCode(cacheFolder, mips)).ToArray()));

            ProcessStartInfo mgcbInfo = new ProcessStartInfo("C:\\Program Files (x86)\\MSBuild\\MonoGame\\v3.0\\Tools\\MGCB.exe", contentPath);
            mgcbInfo.WorkingDirectory = Path.Combine(fullTargetFolder, "Content");
            mgcbInfo.UseShellExecute = false;
            // Run the exporter
            Process processMgcb = Process.Start(mgcbInfo);
            processMgcb.WaitForExit();

            // Build the importer class

            StringBuilder importerClass = new StringBuilder(
               string.Join("\r\n", new[] {
                    "using Microsoft.Xna.Framework;",
                    "using Microsoft.Xna.Framework.Content;",
                    "namespace Autocute.Engine {",
                    "\tpublic partial class SpriteLibrary {\r\n",
                })
            );

            allSprites.Each(x => importerClass.AppendLine($"\t\tpublic readonly Sprite {Utils.FirstLetterToUpper(x.Name)};"));
            importerClass.AppendLine();
            importerClass.AppendLine("\t\tpublic SpriteLibrary(ContentManager content) {");

            foreach (Sprite spr in allSprites) {
                importerClass.AppendLine($"\t\t\t{Utils.FirstLetterToUpper(spr.Name)} = MakeSprite(content, ");
                importerClass.AppendLine($"\t\t\t\tnew Vector2({spr.OriginX}, {spr.OriginY}),");
                for (int i = 0; i < spr.ImagePaths.Length; i++) {
                    importerClass.AppendLine($"\t\t\t\t\"{spr.Name}{i}\"{(i == spr.ImagePaths.Length - 1 ? "" : ",")}");
                }

                importerClass.AppendLine("\t\t\t);");
                importerClass.AppendLine();
            }

            importerClass.AppendLine("\t\t}");
            importerClass.AppendLine("\t}");
            importerClass.AppendLine("}");

            File.WriteAllText(Path.Combine(fullTargetFolder, "Generated", "SpriteLibrary.cs"), importerClass.ToString());

            // Delete the temp content
            File.Delete(contentPath);
        }

        public void Save() {

            var obj = new {
                targetDirectory = targetFolder
            };

            File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(obj), Encoding.UTF8);

            GetAllSprites().Each(x => {
                if (x.Unsaved) {
                    x.Save();
                }
            });
        }

        public class ImportSpriteEventArgs {
            public Sprite Sprite { get; }
            public WorkspaceFolder ParentFolder { get; }

            public ImportSpriteEventArgs(WorkspaceFolder parentFolder, Sprite sprite) {
                Sprite = sprite;
                ParentFolder = parentFolder;
            }
        }


        public event EventHandler<ImportSpriteEventArgs> OnImportSprite;

        private void RaiseImportSprite(WorkspaceFolder parentFolder, Sprite file) {
            OnImportSprite?.Invoke(this, new ImportSpriteEventArgs(parentFolder, file));
        }

    }

    public class WorkspaceFolder {
        public string name;
        public string path;
        public List<WorkspaceFolder> subFolders;
        public List<Sprite> files;

        public WorkspaceFolder(string name, string path, List<WorkspaceFolder> subFolders, List<Sprite> files) {
            this.name = name;
            this.path = path;
            this.subFolders = subFolders;
            this.files = files;
        }
    }
}