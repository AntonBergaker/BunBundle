using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model.Saving {
    class SaveActionSave : SaveAction {
        public readonly Sprite Sprite;
        public override IWorkspaceItem Item => Sprite;
        public override void Run(out bool shouldSave) {
            shouldSave = true;
            // Blank on purpose, everything is saved inside manager on completion
        }

        public SaveActionSave(Sprite sprite):base(sprite.Workspace) {
            Sprite = sprite;
        }
    }
}
