using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BunBundle.Annotations;
using BunBundle.Model;

namespace BunBundle {
    public class FolderViewModel : ItemViewModel {
        public readonly WorkspaceFolder Folder;

        public ObservableCollection<ItemViewModel> Items { get; }

        public override IWorkspaceItem Source => Folder;

        private bool isExpanded;
        public override bool IsExpanded { 
            get => isExpanded;
            set {
                isExpanded = value;
                if (isExpanded && Parent != null) {
                    Parent.IsExpanded = true;
                }
                OnPropertyChanged();
            }
        }

        private void CheckHavingSpriteChildren() {
            foreach (ItemViewModel item in Items) {
                if (item is SpriteViewModel) {
                    HasSpriteChildren = true;
                    return;
                }
                if (item is FolderViewModel subFolder) {
                    if (subFolder.HasSpriteChildren) {
                        HasSpriteChildren = true;
                        return;
                    }

                }
            }

            HasSpriteChildren = false;
        }

        private bool hasSpriteChildren;
        public bool HasSpriteChildren {
            get => hasSpriteChildren;
            set {
                if (hasSpriteChildren == value) {
                    return;
                }

                hasSpriteChildren = value;
                OnPropertyChanged();

                if (hasSpriteChildren && Parent != null) {
                    Parent.HasSpriteChildren = true;
                }
                else {
                    Parent?.CheckHavingSpriteChildren();
                }
            }
        }

        public FolderViewModel(WorkspaceFolder folder, FolderViewModel parent) {
            Name = folder.Name;
            Folder = folder;
            Parent = parent;

            HasSpriteChildren = folder.files.Count > 0;

            Items = new ObservableCollection<ItemViewModel>();
            foreach (WorkspaceFolder subFolder in folder.subFolders) {
                Items.Add(new FolderViewModel(subFolder, this));
            }

            foreach (Sprite sprite in folder.files) {
                Items.Add(new SpriteViewModel(sprite, this));
            }
        }

        public void AddItem(ItemViewModel item) {
            for (int i = 0; i < Items.Count; i++) {
                if (CompareItems(item, Items[i]) < 0) {
                    Items.Insert(i, item);
                    return;
                }
            }
            Items.Add(item);
            if (item is SpriteViewModel) {
                HasSpriteChildren = true;
            }
        }


        private static int CompareItems(ItemViewModel a, ItemViewModel b) {
            if (a.GetType() == b.GetType()) {
                return a.Name.CompareTo(b.Name);
            }

            if (a is FolderViewModel) {
                return -1;
            }

            if (b is FolderViewModel) {
                return 1;
            }

            return 0;
        }
    }
}
