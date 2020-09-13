using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BunBundle.Model.Saving;
using BunBundle.Model;

namespace BunBundle.Model {
    public class WorkspaceFolder : IWorkspaceItem {
        private string _name;

        public string Name {
            get => _name;
            set {
                if (_name == value) return;
                Workspace.AddSaveAction(new SaveActionRename(this, value));
                _name = value;
            }
        }
        public string Path { get; set; }
        public Workspace Workspace { get; }
        public WorkspaceFolder Parent { get; set; }
        public List<WorkspaceFolder> subFolders;
        public List<Sprite> files;

        public WorkspaceFolder(string name, string path, WorkspaceFolder parent) {
            _name = name;
            Workspace = parent?.Workspace;
            Parent = parent;
            Path = path;

            subFolders = new List<WorkspaceFolder>();
            files = new List<Sprite>();
        }

        // Used for importing
        private WorkspaceFolder(string name, string path, Workspace workspace, WorkspaceFolder parent) {
            _name = name;
            Workspace = workspace;
            Parent = parent;
            Path = path;
            subFolders = new List<WorkspaceFolder>();
            files = new List<Sprite>();
            foreach (string dir in Directory.GetDirectories(path)) {
                if (Directory.GetFiles(dir, "*.spr").Length > 0) {
                    files.Add(new Sprite(System.IO.Path.GetFileName(dir), dir, this));
                    continue;
                }

                WorkspaceFolder sub = new WorkspaceFolder(System.IO.Path.GetFileName(dir), dir, workspace, this);
                subFolders.Add(sub);
            }
        }


        public static WorkspaceFolder Import(string path, Workspace workspace) {
            return new WorkspaceFolder(System.IO.Path.GetFileName(path), path, workspace, null);
        }

        public void RemoveChild(WorkspaceFolder folder) {
            subFolders.Remove(folder);
        }

        public void RemoveChild(Sprite sprite) {
            files.Remove(sprite);
        }


        public void Delete() {
            Parent.subFolders.Remove(this);
            Workspace.AddSaveAction(new SaveActionDelete(this));
        }

        public IEnumerable<Sprite> GetAllSprites() {
            IEnumerable<Sprite> sprites = files;
            foreach (WorkspaceFolder folder in subFolders) {
                sprites = sprites.Concat(folder.GetAllSprites());
            }

            return sprites;
        }
        public void MoveTo(WorkspaceFolder targetFolder) {
            Parent.RemoveChild(this);
            Parent = targetFolder;

            Parent.subFolders.Add(this);
            Workspace.AddSaveAction(new SaveActionMoved(this, Utils.GeneratePath(this)));
        }
    }
}
