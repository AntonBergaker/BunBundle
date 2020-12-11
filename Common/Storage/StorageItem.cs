using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BunBundle.Model.Storage {
    public abstract class StorageItem {

        public abstract IWorkspaceItem Item { get; }
        public StorageFolder? Parent { protected internal set; get; }

        public string Name { protected set; get;}

        public virtual string Path => System.IO.Path.Combine(Parent?.Path ?? throw new NullReferenceException("No parent"), Name); 

        protected StorageItem(StorageFolder? parent) {
            Parent = parent;
            parent?.AddChild(this);
            Name = "";
        }

        protected void RunFunctionRecursive(IWorkspaceItem item, Action<IWorkspaceItem> action) {
            if (item is WorkspaceFolder folder) {
                foreach (WorkspaceFolder subFolder in folder.subFolders) {
                    RunFunctionRecursive(subFolder, action);
                }

                foreach (Sprite sprite in folder.files) {
                    RunFunctionRecursive(sprite, action);
                }
            }

            action(item);
        }

        public abstract void Rename(string newName);

        public abstract void Move(StorageFolder newParent);
    }
}
