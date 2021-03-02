using BunBundle.Model;
using BunBundle.ViewModels;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using BunBundle.Annotations;

namespace BunBundle {
    public class SpriteViewModel : TreeItemViewModel, IDropTarget {

        public readonly Sprite Sprite;

        private FolderViewModel parent;
        
        public override FolderViewModel Parent {
            get => parent;
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
            set => parent = value ?? throw new ArgumentNullException(nameof(value), "Parent can not be null for a sprite");
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        }

        public override bool IsUsed => true;

        public override bool HasSpriteChildren {
            get => false;
            set {}
        }

        public override ObservableCollection<TreeItemViewModel>? Items => null;

        public override bool IsExpanded {
            get => false;
            set { }
        }

        public float OriginX {
            get => Sprite.OriginX;
            set {
                Sprite.OriginX = value;
                OnPropertyChanged();
            }
        }

        public float OriginY {
            get => Sprite.OriginY;
            set {
                Sprite.OriginY = value;
                OnPropertyChanged();
            }
        }

        public int Width => Sprite.Width;
        public int Height => Sprite.Height;

        public ObservableCollection<SubImageViewModel> SubImages { get; }

        public override IWorkspaceItem Source => Sprite;

        public SpriteViewModel(Sprite sprite, FolderViewModel parent) {
            Sprite = sprite;
            this.parent = parent;

            SubImages = new ObservableCollection<SubImageViewModel>();
            for (int i=0; i < sprite.ImageAbsolutePaths.Count; i++) {
                SubImages.Add(new SubImageViewModel(this, i));
            }
        }

        public void DragOver(IDropInfo dropInfo) {
            if (dropInfo.Data == null) {
                return;
            }

            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            
        }

        public void Drop(IDropInfo dropInfo) {
            if (dropInfo.Data is SubImageViewModel image) {
                int targetIndex = dropInfo.InsertIndex;

                Sprite.MoveImage(image.Index, targetIndex);

                if (targetIndex > image.Index) {
                    targetIndex--;
                }
                SubImages.Move(image.Index, targetIndex);

                int i = 0;
                foreach (SubImageViewModel imageViewModel in SubImages) {
                    imageViewModel.Index = i++;
                }
            }
        }

        public override void Delete() {
            Parent.Items.Remove(this);
            Parent.CheckHavingSpriteChildren();
            Sprite.Delete();
        }

        public override void MoveTo(FolderViewModel target) {
            Parent.RemoveChild(this);
            Parent = target;

            Sprite.MoveTo(target.Folder);

            Parent.AddItem(this);
            Parent.CheckHavingSpriteChildren();
        }

        public void ReplaceImages(string[] fileNames) {
            Sprite.ClearImages();
            Sprite.AddImages(fileNames);
        }

        public void AddImages(string[] fileNames) {
            Sprite.AddImages(fileNames);
        }

        public void RemoveSubImage(int index) {
            Sprite.RemoveImage(index);
            SubImages.RemoveAt(index);
            for (int i = index; i < SubImages.Count; i++) {
                SubImages[i].Index = i;
            }
        }
    }
}
