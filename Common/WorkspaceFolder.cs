using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace MonogameTexturePacker {
    public class WorkspaceFolder : IWorkspaceItem {
        public string name;
        public string path;
        public List<WorkspaceFolder> subFolders;
        public List<Sprite> files;

        public WorkspaceFolder(string name, string path, List<WorkspaceFolder> subFolders, List<Sprite> files) {
            this.name = name;
            this.path = path;
            this.subFolders = subFolders;
            this.files = files;
        }
    }
}
