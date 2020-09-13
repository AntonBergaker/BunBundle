using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BunBundle.Model.Saving {
    public class SavingManager {
        private readonly Queue<SaveAction> saveQueue;

        public bool UnsavedChanges => saveQueue.Count > 0;

        public SavingManager() {
            this.saveQueue = new Queue<SaveAction>();
        }

        public void Add(SaveAction action) {
            saveQueue.Enqueue(action);
        }

        public void RunActions() {
            HashSet<IWorkspaceItem> items = new HashSet<IWorkspaceItem>(); 
            while (saveQueue.Count > 0) {
                SaveAction action = saveQueue.Dequeue();
                action.Run(out bool shouldSave);
                if (shouldSave) {
                    items.Add(action.Item);
                }
            }

            foreach (IWorkspaceItem item in items) {
                if (item is Sprite sprite) {
                    sprite.Save();
                }
            }
        }

    }
}