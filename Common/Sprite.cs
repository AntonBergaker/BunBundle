using System;
using System.Collections.Generic;
using System.Drawing;
using io = System.IO;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonogameTexturePacker {
    public class Sprite : IWorkspaceItem {
        private string _name;

        public string Name {
            get => _name;
            set {
                if (_name == value) return;
                workspace.AddUnsaved(this);
                _name = value;
            }
        }

        private string _path;
        private readonly Workspace workspace;

        public string Path {
            get => _path;
            set {
                if (_path == value) return;
                workspace.AddUnsaved(this);
                _path = value;
            }
        }

        public float _originX;

        public float OriginX {
            get => _originX;
            set {
                if (_originX == value) return;
                workspace.AddUnsaved(this);
                _originX = value;
            }
        }

        public float _originY;

        public float OriginY {
            get => _originY;
            set {
                if (_originY == value) return;
                workspace.AddUnsaved(this);
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

        public string[] ImageAbsolutePaths => ImagePaths.Select(x => System.IO.Path.Combine(_path, "img", x)).ToArray();

        protected string MetaFile => System.IO.Path.Combine(_path, System.IO.Path.GetFileName(_path) + ".spr");

        public Sprite(string name, string path, Workspace workspace) {
            _name = name;
            _path = path;
            this.workspace = workspace;
            Load();
        }

        public Sprite(string name, string path, string[] imagePaths, Workspace workspace) {
            _name = name;
            _path = path;
            this.ImagePaths = imagePaths;
            this.workspace = workspace;
        }

        public void Load() {
            JObject obj = JObject.Parse(File.ReadAllText(MetaFile));
            JArray paths = (JArray) obj.GetValue("images");
            ImagePaths = paths.Select(x => (string) x).ToArray();
            JObject pos = (JObject) obj["origin"];
            _originX = (float) pos["x"];
            _originY = (float) pos["y"];
        }

        public void Save() {
            var obj = new {
                origin = new {x = OriginX, y = OriginY},
                texturePage = "default",
                images = ImagePaths.Select(x => io.Path.GetFileName(x)).ToArray()
            };

            File.WriteAllText(MetaFile, JsonConvert.SerializeObject(obj), Encoding.UTF8);
        }

        public void ClearImages() {
            ImagePaths = new string[0];
            workspace.AddUnsaved(this, true);
        }

        public void AddImages(string[] sourcePaths) {
            string[] relativePaths = sourcePaths.Select((_, i) => Name + i + ".png").ToArray();
            string[] imagePaths = sourcePaths.Select((_, i) => io.Path.Combine(Path, "img", relativePaths[i])).ToArray();

            sourcePaths.Each((x, i) => File.Copy(x, imagePaths[i], true));
            ImagePaths = ImagePaths.Concat(relativePaths).ToArray();

            workspace.AddUnsaved(this, true);
        }

        public static Sprite Create(string name, string[] sourcePaths, string targetFolder, Workspace workspace) {
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

            Sprite spr = new Sprite(name, folder, relativePaths, workspace);
            spr.Save();

            return spr;
        }
    }
}
