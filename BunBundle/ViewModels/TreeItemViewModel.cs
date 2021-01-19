using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Markup;
using BunBundle.Annotations;
using BunBundle.Model;

namespace BunBundle {
    public abstract class TreeItemViewModel : INotifyPropertyChanged {
        
        public string Name {
            get => Source.Name;
            set {
                Source.Name = value;
                OnPropertyChanged();
            }
        }

        private bool isSelected;
        public bool IsSelected {
            get => isSelected;
            set {
                isSelected = value;
                if (isSelected && Parent != null) {
                    Parent.IsExpanded = true;
                }
                OnPropertyChanged();
            }
        }

        public abstract bool IsUsed { get; }

        public abstract bool HasSpriteChildren { get; set; }

        public abstract ObservableCollection<TreeItemViewModel> Items { get; }

        public abstract bool IsExpanded { get; set; }



        public abstract IWorkspaceItem Source { get; }

        public abstract void Delete();

        public abstract void MoveTo(FolderViewModel target);

        public FolderViewModel? Parent;

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
