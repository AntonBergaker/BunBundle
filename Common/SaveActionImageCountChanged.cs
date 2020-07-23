using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    class SaveActionImageCountChanged : SaveAction {
        public Sprite Sprite { get; }

        public override IWorkspaceItem Item => Sprite;

        public SaveActionImageCountChanged(Sprite sprite) {
            Sprite = sprite;
        }
    }
}
