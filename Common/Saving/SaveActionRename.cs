using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BunBundle.Model.Saving {
    public class SaveActionRename : SaveAction {
        public override IWorkspaceItem Item { get; }
        public readonly string OldPath;

        public SaveActionRename(IWorkspaceItem item, string newName) {
            Item = item;
            OldPath = item.Path;
        }

        public override void Run(out bool shouldSave) {
            shouldSave = true;
            string newPath = "";

            if (Path.GetFileNameWithoutExtension(Item.Path) != Item.Name) {
                newPath = Path.Combine(Path.GetDirectoryName(Item.Path), Item.Name);
            }

            if (newPath == Item.Path) {
                return;
            }

            if (newPath == "") {
                return;
            }

            MoveItem(Item, newPath);
        }
    }
}
