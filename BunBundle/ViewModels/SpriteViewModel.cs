using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using BunBundle.Annotations;
using BunBundle.Model;

namespace BunBundle {
    public class SpriteViewModel : ItemViewModel {

        public readonly Sprite Sprite;

        public override bool IsExpanded {
            get => false;
            set { }
        }

        public override IWorkspaceItem Source => Sprite;

        public SpriteViewModel(Sprite sprite, FolderViewModel parent) {
            Sprite = sprite;
            Name = sprite.Name;
            Parent = parent;
        }

    }
}
