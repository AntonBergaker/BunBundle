using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    class SaveActionImagesChanged : SaveAction {
        public Sprite Sprite { get; }

        public override IWorkspaceItem Item => Sprite;

        public SaveActionImagesChanged(Sprite sprite) {
            Sprite = sprite;
        }
    }
}
