﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BunBundle.Model.Saving;
using BunBundle.Model.Storage;

namespace BunBundle.Model {
    public class Sprite : IWorkspaceItem {
        public readonly Workspace Workspace;
        public StorageItem Storage => StorageSprite;

        public StorageSprite StorageSprite { get; }

        private WorkspaceFolder parent;

        public WorkspaceFolder Parent {
            get => parent;
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
            set => parent = value ?? throw new ArgumentNullException(nameof(value), "Parent can not be null for a sprite");
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        }
        

        private string name;

        public string Name {
            get => name;
            set {
                if (name == value) return;
                Workspace.AddSaveAction(new SaveActionRename(this, value));
                name = value;
            }
        }

        public void Delete() {
            Parent.files.Remove(this);
            Workspace.AddSaveAction(new SaveActionDelete(this));
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

        private void DirtyDimensions() {
            _width = -1;
            _height = -1;
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

        public IReadOnlyList<string> ImageAbsolutePaths => imagePaths.Select(x => Path.Combine(StorageSprite.Path, "img", x)).ToList();

        protected string MetaFile => Path.Combine(StorageSprite.Path, Path.GetFileName(StorageSprite.Path) + ".spr");

        public Sprite(string name, WorkspaceFolder parent) {
            this.name = name;
            StorageSprite = new StorageSprite(parent.StorageFolder, this);
            this.parent = parent;
            Workspace = parent.Workspace;
            Load();
        }

        private Sprite(string name, string[] imagePaths, WorkspaceFolder parent) {
            this.name = name;
            StorageSprite = new StorageSprite(parent.StorageFolder, this);
            this.imagePaths = imagePaths;
            Workspace = parent.Workspace;
            this.parent = parent;
        }


        public void SetImagePath(int index, string path) {
            imagePaths[index] = path;
            _width = -1;
            _height = -1;
        }

        public void ClearImages() {
            DirtyDimensions();
            imagePaths = new string[0];
            Workspace.AddSaveAction(new SaveActionImagesChanged(this));
        }

        public void AddImages(string[] sourcePaths) {
            DirtyDimensions();
            string[] relativePaths = new string[sourcePaths.Length];

            for (int i = 0; i < sourcePaths.Length; i++) {
                int imageCount = imagePaths.Length + i;
                relativePaths[i] = Name + imageCount + ".png";
                string targetPath = Path.Combine(StorageSprite.Path, "img", relativePaths[i]);

                File.Copy(sourcePaths[i], targetPath, true);
            }

            this.imagePaths = ImagePaths.Concat(relativePaths).ToArray();

            Workspace.AddSaveAction(new SaveActionImagesChanged(this));
        }

        public void MoveImage(int index, int targetIndex) {
            List<string> paths = imagePaths.ToList();
            string path = imagePaths[index];
            paths.RemoveAt(index);
            if (targetIndex > index) {
                targetIndex--;
            }
            paths.Insert(targetIndex, path);
            imagePaths = paths.ToArray();

            Workspace.AddSaveAction(new SaveActionImagesChanged(this));
        }

        public void RemoveImage(int index) {
            List<string> imagePathList = imagePaths.ToList();
            imagePathList.RemoveAt(index);
            imagePaths = imagePathList.ToArray();
            Workspace.AddSaveAction(new SaveActionImagesChanged(this));
        }


        public static Sprite Create(string name, string[] sourcePaths, WorkspaceFolder targetFolder) {
            string folder = Path.Combine(targetFolder.Storage.Path, name);

            string imageFolder = Path.Combine(folder, "img");
            if (Directory.Exists(folder) == false) {
                Directory.CreateDirectory(folder);
            }

            if (Directory.Exists(imageFolder) == false) {
                Directory.CreateDirectory(imageFolder);
            }

            string[] relativePaths = sourcePaths.Select((_, i) => name + i + ".png").ToArray();
            string[] imagePaths = sourcePaths.Select((_, i) => Path.Combine(folder, "img", relativePaths[i])).ToArray();

            sourcePaths.Each((x, i) => File.Copy(x, imagePaths[i], true));

            Sprite spr = new Sprite(name, imagePaths, targetFolder);
            spr.Save();

            return spr;
        }

        public void MoveTo(WorkspaceFolder targetFolder) {
            Parent.RemoveChild(this);
            Parent = targetFolder;

            Parent.files.Add(this);

            Workspace.AddSaveAction(new SaveActionMoved(this, targetFolder.StorageFolder));
        }


        private record SpriteJson {
            public SpriteJson(Origin origin, string texturePage, string[] images) {
                this.origin = origin;
                this.texturePage = texturePage;
                this.images = images;
            }

            public record Origin(float x, float y);
            public Origin origin { get; }
            public string texturePage { get; }
            public string[] images { get; }
        }
        
        [MemberNotNull(nameof(imagePaths))]
        public void Load() {
            SpriteJson? obj = JsonSerializer.Deserialize<SpriteJson>(File.ReadAllText(MetaFile));
            if (obj == null) {
                throw new Exception("Failed to load JSON");
            }
            imagePaths = obj.images;
            _originX = obj.origin.x;
            _originY = obj.origin.y;
        }

        public void Save() {
            var obj = new SpriteJson(
                origin: new SpriteJson.Origin(OriginX, OriginY),
                texturePage: "default",
                images: ImagePaths.Select(x => Path.GetFileName(x)).ToArray()
            );

            JsonSerializerOptions options = new JsonSerializerOptions
                {WriteIndented = true, PropertyNamingPolicy = new JsonSnakeCaseifier()};
            
            File.WriteAllText(MetaFile, JsonSerializer.Serialize(obj, options), Encoding.UTF8);
        }



    }

}
