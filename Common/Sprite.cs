using System;
using System.Collections.Generic;
using System.Drawing;
using io = System.IO;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonogameTexturePacker {
    public class Sprite {
        private string _name;
        public string Name {
            get => _name;
            set {
                Unsaved = true;
                _name = value;
            }
        }

        private string _path;

        public string Path {
            get => _path;
            set {
                Unsaved = true;
                _path = value;
            }
        }

        public float _originX;

        public float OriginX {
            get => _originX;
            set {
                Unsaved = true;
                _originX = value;
            }
        }
        public float _originY;

        public float OriginY {
            get => _originY;
            set {
                Unsaved = true;
                _originY = value;
            }
        }

        public int Width {
            get {
                Image image = Image.FromFile(ImageAbsolutePaths[0]);
                int width = image.Width;
                image.Dispose();
                return width;
            }
        }

        public int Height {
            get {
                Image image = Image.FromFile(ImageAbsolutePaths[0]);
                int height = image.Height;
                image.Dispose();
                return height;
            }
        }

        public string[] ImagePaths;
        public bool Unsaved { private set; get; }

        public string[] ImageAbsolutePaths => ImagePaths.Select(x => System.IO.Path.Combine(_path, "img", x)).ToArray();

        protected string MetaFile => System.IO.Path.Combine(_path, Name + ".spr");

        public Sprite(string name, string path) {
            _name = name;
            _path = path;
            this.Unsaved = false;
            Load();
        }

        public Sprite(string name, string path, string[] imagePaths) {
            _name = name;
            _path = path;
            this.ImagePaths = imagePaths;
            this.Unsaved = false;
        }

        public void Load() {
            JObject obj = JObject.Parse(File.ReadAllText(MetaFile));
            JArray paths = (JArray)obj.GetValue("images");
            ImagePaths = paths.Select(x => (string) x).ToArray();
            JObject pos = (JObject) obj["origin"];
            _originX = (float)pos["x"];
            _originY = (float)pos["y"];
        }

        public void SetNewName(string newName) {
            if (string.IsNullOrWhiteSpace(newName)) {
                return;
            }

            if (newName == _name) {
                return;
            }

            string oldName = _name;
            string root = io.Path.GetDirectoryName(_path);
            string newDirectory = io.Path.Combine(root, newName);

            if (!Directory.Exists(newDirectory)) {
                Directory.CreateDirectory(newDirectory);
            }

            string newImgDir = io.Path.Combine(newDirectory, "img");

            if (!Directory.Exists(newImgDir)) {
                Directory.CreateDirectory(newImgDir);
            }

            File.Move(io.Path.Combine(Path, oldName) + ".spr", io.Path.Combine(newDirectory, newName) + ".spr");
            

            string[] absolutePaths = ImageAbsolutePaths;
            for (int i = 0; i < ImagePaths.Length; i++) {
                string newPath = newName + i + ".png";
                ImagePaths[i] = newPath;
                File.Move(absolutePaths[i], io.Path.Combine(newImgDir, newPath));
            }

            Directory.Delete(io.Path.Combine(_path, "img"));
            Directory.Delete(_path);

            _path = newDirectory;
            _name = newName;
            Save();
        }

        public void Save() {
            var obj = new {
                origin = new { x = OriginX, y = OriginY},
                texturePage = "default",
                images = ImagePaths.Select(x => io.Path.GetFileName(x)).ToArray()
            };

            File.WriteAllText(MetaFile, JsonConvert.SerializeObject(obj), Encoding.UTF8);
            Unsaved = false;
        }

        public static Sprite Create(string name, string[] sourcePaths, string targetFolder) {
            string folder = io.Path.Combine(targetFolder, name);

            string imageFolder = io.Path.Combine(folder, "img");
            if (Directory.Exists(folder) == false) {
                Directory.CreateDirectory(folder);
            }

            if (Directory.Exists(imageFolder) == false) {
                Directory.CreateDirectory(imageFolder);
            }

            string[] relativePaths = sourcePaths.Select((_, i) => name + i + ".png").ToArray();
            string[] imagePaths = sourcePaths.Select((_, i) => io.Path.Combine(folder, "img", relativePaths[i])).ToArray();

            sourcePaths.Each((x, i) => File.Copy(x, imagePaths[i], true));
            
            Sprite spr = new Sprite(name, folder, relativePaths);
            spr.Save();

            return spr;
        }

        public string GeneratePackingCode(string cacheFolder, string[] mips) {
            StringBuilder sb = new StringBuilder();
            foreach (string mip in mips) {
                for (int i = 0; i < ImagePaths.Length; i++) {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append(GeneratePackingCodeForImage(System.IO.Path.Combine(cacheFolder, mip, ImagePaths[i]),
                        System.IO.Path.Combine(mip, ImagePaths[i])));
                }
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
    }
}
