using System;
using System.Collections.Generic;
using System.IO;

namespace BunBundle.Model.Storage {
    public class StorageFolder : StorageItem {
        private List<StorageItem> children;
        public IReadOnlyList<StorageItem> Children => children;

        public bool IsRoot { get; }
        public string RootPath { get; }

        public WorkspaceFolder Folder { get; }
        public override IWorkspaceItem Item => Folder;
        public override string Path => IsRoot ? RootPath : base.Path;

        private StorageFolder(string rootPath, WorkspaceFolder folder) : base(null) {
            IsRoot = true;
            RootPath = rootPath;
            children = new List<StorageItem>();
            Folder = folder;
            Name = folder.Name;
        }

        public StorageFolder(StorageFolder? parent, WorkspaceFolder folder) : base(parent) {
            RootPath = null!;
            IsRoot = false;
            children = new List<StorageItem>();
            Folder = folder;
            Name = folder.Name;
        }

        public static StorageFolder MakeRoot(string rootPath, WorkspaceFolder folder) {
            return new StorageFolder(rootPath, folder);
        }

        public override void Rename(string newName) {
            if (Name == newName) {
                return;
            }

            string oldPath = Path;
            Name = newName;
            Folder.Workspace.Directory.Move(oldPath, Path);
        }

        public override void Move(StorageFolder newParent) {
            if (newParent == Parent) {
                return;
            }

            string oldPath = Path;
            Parent?.RemoveChild(this);
            newParent.AddChild(this);

            Folder.Workspace.Directory.Move(oldPath, Path);
        }

        public void AddChild(StorageItem child) {
            if (child.Parent != null) {
                child.Parent.RemoveChild(child);
            }
            children.Add(child);
            child.Parent = this;
        }

        public void RemoveChild(StorageItem child) {
            children.Remove(child);
            child.Parent = null;
        }
    }
}
