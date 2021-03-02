using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
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

        private readonly SpriteViewModel sprite;
        private int index;
        public int Index {
            get => index;
            set {
                index = value;
                OnPropertyChanged();
            }
        }

        public SubImageViewModel(SpriteViewModel sprite, int index) {
            path = sprite.Sprite.ImageAbsolutePaths[index];
            this.sprite = sprite;
            this.index = index;
            removeCommand = new RelayCommand(
                null,
                _ => this.Remove()
            );
        }

        private readonly ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand;


        public void Remove() {
            sprite.RemoveSubImage(index);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
