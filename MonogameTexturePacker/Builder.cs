using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonogameTexturePacker {
    class Builder {



        public void Build(Sprite[] sprites, string rootFolder, string targetFolder) {
            Sprite[] allSprites = sprites;
            string fullTargetFolder = Path.Combine(rootFolder, targetFolder);

            string cacheFolder = Path.GetFullPath(Path.Combine(rootFolder, "..", "__build_cache"));

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

            Dictionary<string, string> imageHashes = LoadImageHashes(cacheFolder);
            Dictionary<string, SpriteExportData> exportSprites = new Dictionary<string, SpriteExportData>();

            // Move all sprites to mip0
            foreach (Sprite spr in allSprites) {
                foreach (string fullPath in spr.ImageAbsolutePaths) {
                    string sourceHash = GetImageHash(fullPath);
                    bool isCached = imageHashes.ContainsKey(fullPath) && imageHashes[fullPath] == sourceHash;


                    string newTargetPath = Path.Combine(cacheFolder, mips[0], Path.GetFileName(fullPath));

                    SpriteExportData data = new SpriteExportData {
                        SourcePath = fullPath,
                        FilePath = newTargetPath,
                        Name = Path.GetFileName(fullPath),
                        Hash = sourceHash,
                        IsCached = isCached
                    };

                    exportSprites.Add(fullPath, data);

                    File.Copy(fullPath, newTargetPath, true);
                }
            }

            // Create a file containing all images to process for magick
            string magickFile = Path.Combine(cacheFolder, "resizeme.txt");
            File.WriteAllLines(magickFile, exportSprites.Values.Where(x => x.IsCached == false).Select(x => x.FilePath));

            // Resize the sprites
            ProcessStartInfo resizeInfo = new ProcessStartInfo("\"C:\\Program Files\\ImageMagick-7.0.9-Q16\\magick.exe\"");
            resizeInfo.WorkingDirectory = cacheFolder;
            resizeInfo.UseShellExecute = true;
            resizeInfo.WindowStyle = ProcessWindowStyle.Hidden;
            List<Process> processes = new List<Process>();

            for (int i = 1; i < mips.Length; i++) {
                resizeInfo.Arguments =
                    $"@resizeme.txt -resize {sizes[i]} -set filename:original %t {cacheFolder}\\{mips[i]}\\%[filename:original].png";
                Process process = Process.Start(resizeInfo);
                processes.Add(process);

            }

            processes.Each(x => x.WaitForExit());

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
                    "\tstatic public partial class Sprites {\r\n",
                })
            );

            allSprites.Each(x => importerClass.AppendLine($"\t\tpublic static Sprite {Utils.FirstLetterToUpper(x.Name)} {{private set; get;}}"));
            importerClass.AppendLine();
            importerClass.AppendLine("\t\tstatic private void ImportSprites(ContentManager content) {");

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

            // Save the new hash content
            Dictionary<string, string> hashDict = new Dictionary<string, string>();
            exportSprites.Values.Each(x => hashDict.Add(x.SourcePath, x.Hash));
            SaveImageHashes(cacheFolder, hashDict);
        }


        private string GetImageHash(string imagePath) {
            byte[] bites;
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(imagePath)) {
                    bites = md5.ComputeHash(stream);
                }
            }
            return Convert.ToBase64String(bites);
        }

        private Dictionary<string, string> LoadImageHashes(string cacheFolder) {
            string path = Path.Combine(cacheFolder, "hashes.json");

            try {
                string json = File.ReadAllText(path);

                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            } catch { }
            return new Dictionary<string, string>();
        }

        private void SaveImageHashes(string cacheFolder, Dictionary<string, string> dict) {
            string path = Path.Combine(cacheFolder, "hashes.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(dict));
        }

        private class SpriteExportData {
            public string SourcePath;
            public string FilePath;
            public string Name;
            public string Hash;
            public bool IsCached;
        }

    }
}

