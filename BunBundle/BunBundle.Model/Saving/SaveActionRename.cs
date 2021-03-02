using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BunBundle.Model.Saving {
    public class SaveActionRename : SaveAction {
        public override IWorkspaceItem Item { get; }
        public string NewName { get; private set; }

        public SaveActionRename(IWorkspaceItem item, string newName): base(item.Workspace) {
            Item = item;
            NewName = newName;
        }

        public override void Run(out bool shouldSave) {
            shouldSave = true;

            Item.Storage.Rename(NewName);
        }

        public override bool TryMerge(SaveAction previous) {
            if (previous is SaveActionRename) {
                if (previous.Item == Item) {
                    return true;
                }
            }

            return false;
        }

    }
}
