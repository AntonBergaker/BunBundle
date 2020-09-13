using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model.Saving {
    class SaveActionMoved : SaveAction {
        private readonly string newPath;
        public override IWorkspaceItem Item { get; }

        public SaveActionMoved(IWorkspaceItem item, string newPath) {
            this.newPath = newPath;
            Item = item;
        }

        public override void Run(out bool shouldSave) {
            shouldSave = true;
            if (Item is Sprite sprite) {
                MoveSprite(sprite, newPath);
                return;
            }

            if (Item is WorkspaceFolder folder) {
                MoveFolder(folder, newPath);
            }            
        }
    }
}
