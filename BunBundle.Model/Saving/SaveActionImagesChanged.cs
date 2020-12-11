using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BunBundle.Model.Saving {
    class SaveActionImagesChanged : SaveAction {
        public Sprite Sprite { get; }

        public override IWorkspaceItem Item => Sprite;

        public SaveActionImagesChanged(Sprite sprite) {
            Sprite = sprite;
        }
        public override void Run(out bool shouldSave) {
            shouldSave = true;
            for (int index = 0; index < Sprite.ImagePaths.Count; index++) {
                string spriteImagePath = Sprite.ImagePaths[index];
                if (spriteImagePath != Sprite.Name + index + ".png") {
                    FixSpriteOrder(Sprite);
                    break;
                }
            }

            string[] paths = Directory.GetFiles(Path.Combine(Sprite.StorageSprite.Path, "img"));
            string[] truthTable = Sprite.ImagePaths.Select(x => Path.GetFileName(x)).ToArray();

            foreach (string path in paths) {
                string strippedPath = Path.GetFileName(path);

                if (truthTable.Contains(strippedPath) == false) {
                    File.Delete(path);
                }
            }
        }

        private void FixSpriteOrder(Sprite sprite) {
            List<(string from, string to)> toFix = new List<(string from, string to)>();

            for (int index = 0; index < sprite.ImagePaths.Count; index++) {
                string spriteImagePath = sprite.ImagePaths[index];
                string fullPath = Path.Combine(sprite.StorageSprite.Path, "img", spriteImagePath);
                string newPath = Path.Combine(sprite.StorageSprite.Path, "img", sprite.Name + index + ".png");
                string tempPath = Path.Combine(sprite.StorageSprite.Path, "img", sprite.Name + index + "_temp_for_moving_about" + ".png");

                File.Move(fullPath, tempPath);

                sprite.SetImagePath(index, newPath);

                toFix.Add((tempPath, newPath));
            }

            foreach ((string from, string to) in toFix) {
                File.Move(from, to, true);
            }
        }
    }
}
