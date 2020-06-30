﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonogameTexturePacker {
    class Builder {

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }



        public void Build(WorkspaceFolder workspaceFolder, string rootFolder, string targetFolder, string MgcbPath) {
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

            MoveFiles(workspaceFolder, cacheFolder, "", imageHashes, exportSprites, mips, true);

            foreach (SpriteExportData spriteData in exportSprites.Values) {
                if (spriteData.IsCached) {
                    continue;
                }

                Image image = Image.FromFile(spriteData.FilePath);

                double height = image.Height;
                double width = image.Width;

                for (int i = 1; i < mips.Length; i++) {
                    height /= 2;
                    width /= 2;
                    Bitmap resizedImage = ResizeImage(image, (int)width, (int)height);
                    string path = Path.Combine(cacheFolder, mips[i], spriteData.LocalPath);
                    if (!Directory.Exists(Path.GetDirectoryName(path))) {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    resizedImage.Save(path);
                    resizedImage.Dispose();
                }

                image.Dispose();
            }


            // Run the MGCB
            string contentPath = Path.Combine(fullTargetFolder, "Content", "TempContent.mgcb");
            Console.WriteLine("NEW PATH: " + contentPath);
            
            CodeBuilder mgcbText = new CodeBuilder();
            mgcbText.AddLines(
                "/outputDir:bin/Content",
                "/intermediateDir:obj/Content",
                "/platform:Windows",
                "/config:",
                "/profile:Reach",
                "/compress:False"
            );

            foreach (SpriteExportData sprite in exportSprites.Values) {
                mgcbText.AddLine(GeneratePackingCode(sprite, cacheFolder, mips));
            }

            // Build the file
            File.WriteAllText(contentPath, mgcbText.ToString());

            ProcessStartInfo mgcbInfo = new ProcessStartInfo(MgcbPath, contentPath);
            mgcbInfo.WorkingDirectory = Path.Combine(fullTargetFolder, "Content");
            mgcbInfo.UseShellExecute = false;
            // Run the exporter
            Process processMgcb = Process.Start(mgcbInfo);
            processMgcb.WaitForExit();

            // Build the importer class

            CodeBuilder importerClass = new CodeBuilder();

            importerClass.AddLines(
                "using Microsoft.Xna.Framework;",
                "using Microsoft.Xna.Framework.Content;",
                "namespace Autocute.Engine {"
            );

            importerClass.Indent();

            BuildClassContent(importerClass, workspaceFolder, "", true);

            importerClass.Unindent();
            importerClass.AddLine("}");

            File.WriteAllText(Path.Combine(fullTargetFolder, "Generated", "SpriteLibrary.cs"), importerClass.ToString());

            // Delete the temp content
            File.Delete(contentPath);

            // Save the new hash content
            Dictionary<string, string> hashDict = new Dictionary<string, string>();
            exportSprites.Values.Each(x => hashDict.Add(x.SourcePath, x.Hash));
            SaveImageHashes(cacheFolder, hashDict);
        }

        private void MoveFiles(WorkspaceFolder folder, string cachePath, string path, Dictionary<string, string> imageHashes, Dictionary<string, SpriteExportData> exportSprites, string[] mips, bool isRoot) {
            foreach (WorkspaceFolder childFolder in folder.subFolders) {
                MoveFiles(childFolder, cachePath, path + childFolder.name, imageHashes, exportSprites, mips, false);
            }

            string folderPath = Path.Combine(cachePath, mips[0], path);
            // Create the folder if it doesn't exist
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }
            
            // Move all sprites to mip0
            foreach (Sprite spr in folder.files) {
                foreach (string fullPath in spr.ImageAbsolutePaths) {
                    string sourceHash = GetImageHash(fullPath);
                    bool isCached = imageHashes.ContainsKey(fullPath) && imageHashes[fullPath] == sourceHash;

                    string newTargetPath = Path.Combine(folderPath, Path.GetFileName(fullPath));
                    string localPath = Path.Combine(path, Path.GetFileName(fullPath));

                    SpriteExportData data = new SpriteExportData {
                        SourcePath = fullPath,
                        FilePath = newTargetPath,
                        LocalPath = localPath,
                        Name = Path.GetFileName(fullPath),
                        Hash = sourceHash,
                        IsCached = isCached
                    };

                    exportSprites.Add(fullPath, data);

                    File.Copy(fullPath, newTargetPath, true);
                }
            }
        }

        private void BuildClassContent(CodeBuilder importerClass, WorkspaceFolder folder, string localPath, bool isRoot) {
            if (isRoot) {
                importerClass.AddLine("public static partial class Sprites {");
            }
            else {
                importerClass.AddLine($"public static class {Utils.FirstLetterToUpper(folder.name)} {{");
            }

            importerClass.Indent();

            foreach (WorkspaceFolder childFolder in folder.subFolders) {
                BuildClassContent(importerClass, childFolder, localPath + "\\\\" + childFolder.name, false);
            }

            foreach (Sprite sprite in folder.files) {
               importerClass.AddLine($"public static Sprite {Utils.FirstLetterToUpper(sprite.Name)} {{private set; get;}}");
            }

            importerClass.AddLine("");
            importerClass.AddLine("internal static void ImportSprites(ContentManager content) {");
            importerClass.Indent();

            foreach (WorkspaceFolder childFolder in folder.subFolders) {
                importerClass.AddLine($"{Utils.FirstLetterToUpper(childFolder.name)}.ImportSprites(content);");
            }

            if (folder.subFolders.Count > 0) {
                importerClass.AddLine("");
            }

            foreach (Sprite spr in folder.files) {
                importerClass.AddLine($"{Utils.FirstLetterToUpper(spr.Name)} = MakeSprite(content, ");
                importerClass.Indent();
                importerClass.AddLine($"new Vector2({spr.OriginX}, {spr.OriginY}),");
                for (int i = 0; i < spr.ImagePaths.Length; i++) {
                    importerClass.AddLine($"\"{localPath + "\\\\" + spr.Name}{i}\"{(i == spr.ImagePaths.Length - 1 ? "" : ",")}");
                }
                importerClass.Unindent();
                importerClass.AddLine(");");
                importerClass.AddLine("");
            }
            importerClass.Unindent();
            importerClass.AddLine("}");
            importerClass.Unindent();
            importerClass.AddLine("}");
        }


        private string GeneratePackingCode(SpriteExportData sprite, string cacheFolder, string[] mips) {
            StringBuilder sb = new StringBuilder();
            foreach (string mip in mips) {

                sb.AppendLine();
                sb.AppendLine();
                sb.Append(GeneratePackingCodeForImage(
                    Path.Combine(cacheFolder, mip, sprite.LocalPath),
                    Path.Combine(mip, sprite.LocalPath)));
            
            }

            return sb.ToString();
        }

        private string GeneratePackingCodeForImage(string absolutePath, string localPath) {
            return string.Join("\r\n",
                $"#begin {absolutePath}", //this is just a comment, but it looks neat to have
                "/importer:TextureImporter",
                "/processor:TextureProcessor",
                "/processorParam:ColorKeyColor=255,0,255,255",
                "/processorParam:ColorKeyEnabled=True",
                "/processorParam:GenerateMipmaps=False",
                "/processorParam:PremultiplyAlpha=True",
                "/processorParam:ResizeToPowerOfTwo=False",
                "/processorParam:MakeSquare=False",
                "/processorParam:TextureFormat=Color",
                $"/build:{absolutePath};{localPath}"
            );
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
            public string LocalPath;
            public string Name;
            public string Hash;
            public bool IsCached;
        }

    }
}

