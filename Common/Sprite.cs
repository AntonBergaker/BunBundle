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

namespace BunBundle.Model {
    public class Sprite : IWorkspaceItem {
        public readonly Workspace Workspace;
        public WorkspaceFolder Parent;

        private string _name;

        public string Name {
            get => _name;
            set {
                if (_name == value) return;
                Workspace.AddSaveAction(new SaveActionRename(this));
                _name = value;
            }
        }

        private string _path;

        public string Path {
            get => _path;
            set {
                if (_path == value) return;
                Workspace.AddSaveAction(new SaveActionSave(this));
                _path = value;
            }
        }

        public float _originX;

        public float OriginX {
            get => _originX;
            set {
                if (_originX == value) return;
                Workspace.AddSaveAction(new SaveActionSave(this));
                _originX = value;
            }
        }

        public float _originY;

        public float OriginY {
            get => _originY;
            set {
                if (_originY == value) return;
                Workspace.AddSaveAction(new SaveActionSave(this));
                _originY = value;
            }
        }

        private int _width = -1;
        private int _height = -1;

        private void SetDimensions() {
            if (_width != -1 || _height != -1) {
                return;
            }
            using Stream stream = File.OpenRead(ImageAbsolutePaths[0]);
            using Image image = Image.FromStream(stream, false, false);
            _width = image.Width;
            _height = image.Height;
        }

        public int Width {
            get {
                SetDimensions();
                return _width;
            }
        }

        public int Height {
            get {
                SetDimensions();
                return _height;
            }
        }

        public string[] imagePaths;

        public IReadOnlyList<string> ImagePaths => imagePaths;

        public IReadOnlyList<string> ImageAbsolutePaths => imagePaths.Select(x => System.IO.Path.Combine(_path, "img", x)).ToList();

        protected string MetaFile => System.IO.Path.Combine(_path, System.IO.Path.GetFileName(_path) + ".spr");

        public Sprite(string name, string path, WorkspaceFolder parent) {
            _name = name;
            _path = path;
            this.Parent = parent;
            Workspace = parent?.Workspace;
            Load();
        }

        public Sprite(string name, string path, Workspace workspace) : this(name, path, (WorkspaceFolder)null) {
            Workspace = workspace;
        }

        private Sprite(string name, string path, string[] imagePaths, WorkspaceFolder parent) {
            _name = name;
            _path = path;
            this.imagePaths = imagePaths;
            Workspace = parent.Workspace;
            this.Parent = parent;
        }

        public void Load() {
            JObject obj = JObject.Parse(File.ReadAllText(MetaFile));
            JArray paths = (JArray) obj.GetValue("images");
            imagePaths = paths.Select(x => (string) x).ToArray();
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

        public void SetImagePath(int index, string path) {
            imagePaths[index] = path;
            _width = -1;
            _height = -1;
        }

        public void ClearImages() { 
            imagePaths = new string[0];
            Workspace.AddSaveAction(new SaveActionImageCountChanged(this));
        }

        public void AddImages(string[] sourcePaths) {
            string[] relativePaths = new string[sourcePaths.Length];

            for (int i = 0; i < sourcePaths.Length; i++) {
                int imageCount = imagePaths.Length + i;
                relativePaths[i] = Name + imageCount + ".png";
                string targetPath = io.Path.Combine(Path, "img", relativePaths[i]);

                File.Copy(sourcePaths[i], targetPath, true);
            }

            this.imagePaths = ImagePaths.Concat(relativePaths).ToArray();

            Workspace.AddSaveAction(new SaveActionImageCountChanged(this));
        }

        public static Sprite Create(string name, string[] sourcePaths, WorkspaceFolder targetFolder, Workspace workspace) {
            string folder = io.Path.Combine(targetFolder.Path, name);

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

            Sprite spr = new Sprite(name, folder, relativePaths, targetFolder);
            spr.Save();

            return spr;
        }
    }
}
