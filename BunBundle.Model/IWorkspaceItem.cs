using System;
using System.Collections.Generic;
using System.Text;
using BunBundle.Model.Storage;

namespace BunBundle.Model {
    public interface IWorkspaceItem {
        string Name { get; set; }
        StorageItem Storage { get; }

        WorkspaceFolder Parent { get; set; }

        public void Delete();

        public void MoveTo(WorkspaceFolder targetFolder);
    }
}
