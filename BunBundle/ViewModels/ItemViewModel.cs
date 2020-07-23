using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Markup;
using BunBundle.Annotations;
using BunBundle.Model;

namespace BunBundle {
    public abstract class ItemViewModel : INotifyPropertyChanged {
        
        private string name;
        public string Name {
            get => name;
            set {
                name = value;
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


        public abstract bool IsExpanded { get; set; }



        public abstract IWorkspaceItem Source { get; }

        public FolderViewModel Parent;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
