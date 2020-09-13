using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using BunBundle.Annotations;
using BunBundle.Model;

namespace BunBundle.ViewModels {
    public class SubImageViewModel : INotifyPropertyChanged {
        private string path;
        public string Path {
            get => path;
            set {
                path = value;
                OnPropertyChanged();
            }
        }

        private int index;
        public int Index {
            get => index;
            set {
                index = value;
                OnPropertyChanged();
            }
        }

        public SubImageViewModel(Sprite sprite, int index) {
            path = sprite.ImageAbsolutePaths[index];
            this.index = index;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
