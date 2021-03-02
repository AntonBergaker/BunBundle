using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;

namespace BunBundle.Model.Saving {
    public abstract class SaveAction {
        protected Workspace Workspace;
        protected IFile File;
        protected IDirectory Directory;
        
        protected SaveAction(Workspace workspace) {
            this.Workspace = workspace;
            File = workspace.File;
            Directory = workspace.Directory;
        }
        
        public abstract IWorkspaceItem Item { get; }

        public abstract void Run(out bool shouldSave);

        public virtual bool TryMerge(SaveAction previous) {
            return false;
        }
    }
}
