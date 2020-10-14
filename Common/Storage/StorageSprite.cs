using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BunBundle.Model.Storage {
    public class StorageSprite : StorageItem {
        public Sprite Sprite { get; }
        public override IWorkspaceItem Item => Sprite;
        public override void Rename(string newName) {
            if (Name == newName) {
                return;
            }

            string oldName = Name;
            string oldPath = Path;
            Name = newName;
            Directory.Move(oldPath, Path);

            string newImgDir = System.IO.Path.Combine(Path, "img");

            File.Move(System.IO.Path.Combine(Path, oldName) + ".spr", System.IO.Path.Combine(Path, newName) + ".spr");

            var absolutePaths = Sprite.ImageAbsolutePaths;
            for (int i = 0; i < absolutePaths.Count; i++) {
                string newPath = newName + i + ".png";
                Sprite.SetImagePath(i, newPath);
                File.Move(absolutePaths[i], System.IO.Path.Combine(newImgDir, newPath));
            }
        }

        public override void Move(StorageFolder newParent) {
            if (newParent == Parent) {
                return;
            }

            string oldPath = Path;
            Parent.RemoveChild(this);
            newParent.AddChild(this);

            Directory.Move(oldPath, Path);
        }

        public StorageSprite(StorageFolder parent, Sprite sprite) : base(parent) {
            Sprite = sprite;
            Name = sprite.Name;
        }

    }
}
