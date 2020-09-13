using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    public interface IWorkspaceItem {
        string Name { get; set; }

        string Path { get; set; }

        WorkspaceFolder Parent { get; set; }

        public void Delete();

        public void MoveTo(WorkspaceFolder targetFolder);
    }
}
