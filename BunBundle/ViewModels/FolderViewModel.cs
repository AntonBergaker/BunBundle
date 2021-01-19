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
    public class FolderViewModel : TreeItemViewModel {
        public readonly WorkspaceFolder Folder;

        public override ObservableCollection<TreeItemViewModel> Items { get; }

        public override IWorkspaceItem Source => Folder;

        public override bool IsUsed => HasSpriteChildren;

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

        internal void CheckHavingSpriteChildren() {
            foreach (TreeItemViewModel item in Items) {
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
        public override bool HasSpriteChildren {
            get => hasSpriteChildren;
            set {
                if (hasSpriteChildren == value) {
                    return;
                }

                hasSpriteChildren = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUsed));

                if (hasSpriteChildren && Parent != null) {
                    Parent.HasSpriteChildren = true;
                }
                else {
                    Parent?.CheckHavingSpriteChildren();
                }
            }
        }

        public FolderViewModel(WorkspaceFolder folder, FolderViewModel? parent) {
            Folder = folder;
            Parent = parent;

            HasSpriteChildren = folder.files.Count > 0;

            Items = new ObservableCollection<TreeItemViewModel>();
            foreach (WorkspaceFolder subFolder in folder.subFolders) {
                Items.Add(new FolderViewModel(subFolder, this));
            }

            foreach (Sprite sprite in folder.files) {
                Items.Add(new SpriteViewModel(sprite, this));
            }
        }

        public override void Delete() {
            if (Parent == null) {
                throw new NullReferenceException("Can't delete the root folder");
            }
            Parent.Items.Remove(this);
            Parent.CheckHavingSpriteChildren();
            Folder.Delete();
        }

        public override void MoveTo(FolderViewModel target) {
            Parent.RemoveChild(this);
            Parent = target;

            Folder.MoveTo(target.Folder);

            Parent.AddItem(this);
            Parent.CheckHavingSpriteChildren();
        }

        public void AddItem(TreeItemViewModel treeItem) {
            for (int i = 0; i < Items.Count; i++) {
                if (CompareItems(treeItem, Items[i]) < 0) {
                    Items.Insert(i, treeItem);
                    return;
                }
            }
            Items.Add(treeItem);
            if (treeItem is SpriteViewModel) {
                HasSpriteChildren = true;
            }
        }


        private static int CompareItems(TreeItemViewModel a, TreeItemViewModel b) {
            if (a.GetType() == b.GetType()) {
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            }

            if (a is FolderViewModel) {
                return -1;
            }

            if (b is FolderViewModel) {
                return 1;
            }

            return 0;
        }

        public void RemoveChild(TreeItemViewModel item) {
            Items.Remove(item);
        }
    }
}
