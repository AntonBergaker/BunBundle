using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    class SaveActionSave : SaveAction {
        public readonly Sprite Sprite;
        public override IWorkspaceItem Item => Sprite;

        public SaveActionSave(Sprite sprite) {
            Sprite = sprite;
        }
    }
}
