using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BunBundle.Model.Saving {
    public abstract class SaveAction {
        public abstract IWorkspaceItem Item { get; }

        public abstract void Run(out bool shouldSave);

    }
}
