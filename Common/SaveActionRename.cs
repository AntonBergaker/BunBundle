using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    public class SaveActionRename : SaveAction {
        public override IWorkspaceItem Item { get; }
        public readonly string OldPath;

        public SaveActionRename(IWorkspaceItem item) {
            Item = item;
            OldPath = item.Path;
        }
    }
}
