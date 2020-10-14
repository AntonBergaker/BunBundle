using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BunBundle.Model.Saving {
    class SaveActionDelete : SaveAction {
        public override IWorkspaceItem Item { get; }

        public SaveActionDelete(IWorkspaceItem item) {
            Item = item;
        }

        public override void Run(out bool shouldSave) {
            shouldSave = false;
            Directory.Delete(Item.Storage.Path, true);
        }
    }
}
