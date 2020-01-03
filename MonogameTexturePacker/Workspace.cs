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

            Builder builder = new Builder();

            builder.Build(GetAllSprites(), RootFolder.path, targetFolder);

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