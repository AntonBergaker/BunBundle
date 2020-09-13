using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BunBundle.Model.Saving {
    public abstract class SaveAction {
        public abstract IWorkspaceItem Item { get; }

        public abstract void Run(out bool shouldSave);

        protected void MoveItem(IWorkspaceItem item, string newDirectory) {
            if (item is Sprite sprite) {
                MoveSprite(sprite, newDirectory);
                return;
            }

            if (item is WorkspaceFolder folder) {
                MoveFolder(folder, newDirectory);
                return;
            }
        }

        protected void RunFunctionRecursive(IWorkspaceItem item, Action<IWorkspaceItem> action) {
            if (item is WorkspaceFolder folder) {
                foreach (WorkspaceFolder subFolder in folder.subFolders) {
                    RunFunctionRecursive(subFolder, action);
                }

                foreach (Sprite sprite in folder.files) {
                    RunFunctionRecursive(sprite, action);
                }
            }

            action(item);
        }

        protected void MoveSprite(Sprite sprite, string newDirectory) {
            string oldDirectory = sprite.Path;

            if (Directory.Exists(newDirectory) == false) {
                Directory.CreateDirectory(newDirectory);
            }

            string newImgDir = Path.Combine(newDirectory, "img");

            if (!Directory.Exists(newImgDir)) {
                Directory.CreateDirectory(newImgDir);
            }

            string oldName = Path.GetFileName(oldDirectory);
            string newName = Path.GetFileName(newDirectory);

            File.Move(Path.Combine(oldDirectory, oldName) + ".spr", Path.Combine(newDirectory, newName) + ".spr");

            var absolutePaths = sprite.ImageAbsolutePaths;
            for (int i = 0; i < absolutePaths.Count; i++) {
                string newPath = newName + i + ".png";
                sprite.SetImagePath(i, newPath);
                File.Move(absolutePaths[i], Path.Combine(newImgDir, newPath));
            }

            sprite.Path = newDirectory;

            Directory.Delete(oldDirectory, true);
        }

        protected void MoveFolder(WorkspaceFolder folder, string newDirectory) {
            string parentDirectory = Path.GetDirectoryName(newDirectory);
            if (!Directory.Exists(parentDirectory)) {
                Directory.CreateDirectory(parentDirectory);
            }

            Directory.Move(folder.Path, newDirectory);


            void UpdatePath(WorkspaceFolder workFolder, string pathSoFar) {
                foreach (WorkspaceFolder subFolder in workFolder.subFolders) {
                    UpdatePath(subFolder, Path.Combine(pathSoFar, Path.GetFileName(workFolder.Path)));
                }

                foreach (Sprite sprite in workFolder.files) {
                    sprite.Path = Path.Combine(pathSoFar, Path.GetFileName(sprite.Path));
                }

                workFolder.Path = pathSoFar;
            }

            UpdatePath(folder, newDirectory);
        }
    }
}
