using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    public abstract class SaveAction {
        public abstract IWorkspaceItem Item { get; }
    }
}
