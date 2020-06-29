using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<Sprite> GetAllSprites() {
            IEnumerable<Sprite> sprites = files;
            foreach (WorkspaceFolder folder in subFolders) {
                sprites = sprites.Concat(folder.GetAllSprites());
            }

            return sprites;
        }
    }
}
