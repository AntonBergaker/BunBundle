using System;
using System.Collections.Generic;
using System.Text;
using BunBundle.Model.Storage;

namespace BunBundle.Model.Saving {
    class SaveActionMoved : SaveAction {
        private readonly StorageFolder newParent;
        public override IWorkspaceItem Item { get; }

        public SaveActionMoved(IWorkspaceItem item, StorageFolder newParent) {
            this.newParent = newParent;
            Item = item;
        }

        public override void Run(out bool shouldSave) {
            shouldSave = true;
            Item.Storage.Move(newParent);
        }
    }
}
