using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BunBundle.Model {
    public class SavingManager {

        private readonly Dictionary<IWorkspaceItem, List<SaveAction>> updatedItem;

        public SavingManager() {
            updatedItem = new Dictionary<IWorkspaceItem, List<SaveAction>>();
        }

        public void AddSaveAction(SaveAction saveAction) {
            if (updatedItem.TryGetValue(saveAction.Item, out List<SaveAction> values) == false) {
                values = new List<SaveAction>();
                updatedItem[saveAction.Item] = values;
            }

            // Replace the action if it already exists
            for (int i = 0; i < values.Count; i++) {
                if (values[i].GetType() == saveAction.GetType()) {
                    values[i] = saveAction;
                    return;
                }
            }

            values.Add(saveAction);
        }

        public bool UnsavedChanges => updatedItem.Count > 0;


        private List<T> GetActionsOfType<T>() where T : SaveAction {
            List<T> list = new List<T>();

            foreach (List<SaveAction> actions in updatedItem.Values) {
                foreach (SaveAction action in actions) {
                    if (action is T typedAction) {
                        list.Add(typedAction);
                    } 
                }
            }


            return list;
        }

        public void Save() {
            List<(IWorkspaceItem, string)> tempLocations = new List<(IWorkspaceItem, string)>();

            // Scan if we have too many sprites
            foreach (SaveActionImageCountChanged saveAction in GetActionsOfType<SaveActionImageCountChanged>()) {

                Sprite sprite = saveAction.Sprite;

                string[] paths = Directory.GetFiles(Path.Combine(saveAction.Item.Path, "img"));
                string[] truthTable = sprite.ImagePaths.Select(x => Path.GetFileName(x)).ToArray();

                foreach (string path in paths) {
                    string strippedPath = Path.GetFileName(path);

                    if (truthTable.Contains(strippedPath) == false) {
                        File.Delete(path);
                    }
                }
            }

            // Move folders that need moving
            foreach (SaveActionRename folderData in GetActionsOfType<SaveActionRename>()) {
                if (!(folderData.Item is WorkspaceFolder folder)) {
                    continue;
                }

                string newPath = "";

                if (Path.GetFileNameWithoutExtension(folder.Path) != folder.Name) {
                    newPath = Path.Combine(Path.GetDirectoryName(folder.Path), folder.Name);
                }

                if (newPath == "") {
                    continue;
                }

                // If it already exists wait to move until later, just in case something is moving from here
                if (Directory.Exists(newPath)) {
                    newPath = newPath + "-temp_for_moving_about";
                    tempLocations.Add((folder, newPath));
                }

                MoveFolder(folder, folder.Path, newPath);
            }

            // Move sprites that need moving
            foreach (SaveActionRename spriteData in GetActionsOfType<SaveActionRename>()) {
                Sprite sprite = spriteData.Item as Sprite;
                if (sprite == null) {
                    continue;
                }
                string newPath = "";

                if (Path.GetFileNameWithoutExtension(sprite.Path) != sprite.Name) {
                    newPath = Path.Combine(Path.GetDirectoryName(sprite.Path), sprite.Name);
                }

                if (newPath == "") {
                    continue;
                }

                // If it already exists wait to move until later, just in case something is moving from here
                if (Directory.Exists(newPath)) {
                    newPath = newPath + "-temp_for_moving_about";
                    tempLocations.Add((sprite, newPath));
                }

                MoveSprite(sprite, sprite.Path, newPath);
            }

            foreach ((IWorkspaceItem item, string newPath) in tempLocations) {
                string stripped = newPath.Remove(newPath.LastIndexOf("-temp_for_moving_about"));
                if (item is Sprite sprite) {
                    MoveSprite(sprite, newPath, stripped);
                } else if (item is WorkspaceFolder folder) {
                    MoveFolder(folder, newPath, stripped);
                }
            }

            foreach (List<SaveAction> list in updatedItem.Values) {
                if (list[0].Item is Sprite sprite) {
                    sprite.Save();
                }
            }

            updatedItem.Clear();
        }

        private void MoveSprite(Sprite sprite, string oldDirectory, string newDirectory) {

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

        private void MoveFolder(WorkspaceFolder folder, string oldDirectory, string newDirectory) {
            string parentDirectory = Path.GetDirectoryName(newDirectory);
            if (!Directory.Exists(parentDirectory)) {
                Directory.CreateDirectory(parentDirectory);
            }

            Directory.Move(oldDirectory, newDirectory);
        }

    }
}
