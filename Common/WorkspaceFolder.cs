using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BunBundle.Model {
    public class WorkspaceFolder : IWorkspaceItem {
        private string _name;

        public string Name {
            get => _name;
            set {
                if (_name == value) return;
                Workspace.AddSaveAction(new SaveActionRename(this));
                _name = value;
            }
        }
        public string Path { get; set; }
        public readonly Workspace Workspace;
        public WorkspaceFolder Parent;
        public List<WorkspaceFolder> subFolders;
        public List<Sprite> files;

        public WorkspaceFolder(string name, string path, WorkspaceFolder parent, List<WorkspaceFolder> subFolders, List<Sprite> files) {
            _name = name;
            this.Path = path;
            this.Workspace = parent?.Workspace;
            Parent = parent;
            this.subFolders = subFolders;

            foreach (WorkspaceFolder subFolder in subFolders) {
                subFolder.Parent = this;
            }

            this.files = files;

            foreach (Sprite sprite in files) {
                sprite.Parent = this;
            }
        }

        public WorkspaceFolder(string name, string path, Workspace workspace, List<WorkspaceFolder> subFolders, List<Sprite> files) : this(name, path, (WorkspaceFolder)null, subFolders, files) {
            Workspace = workspace;
        }

        public IEnumerable<Sprite> GetAllSprites() {
            IEnumerable<Sprite> sprites = files;
            foreach (WorkspaceFolder folder in subFolders) {
                sprites = sprites.Concat(folder.GetAllSprites());
            }

            return sprites;
        }
    }
}
