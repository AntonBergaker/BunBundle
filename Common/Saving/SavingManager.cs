using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BunBundle.Model.Saving {
    public class SavingManager {
        private readonly LinkedList<SaveAction> saveQueue;

        public bool UnsavedChanges => saveQueue.Count > 0;

        public SavingManager() {
            this.saveQueue = new LinkedList<SaveAction>();
        }

        public void Add(SaveAction action) {
            SaveAction? last = saveQueue.Count > 0 ? saveQueue.Last() : null;
            if (last != null) {
                if (last.TryMerge(action)) {
                    saveQueue.RemoveLast();
                }
            }

            saveQueue.AddLast(action);
        }

        public void RunActions() {
            HashSet<IWorkspaceItem> items = new HashSet<IWorkspaceItem>(); 
            while (saveQueue.Count > 0) {
                SaveAction action = saveQueue.First();
                saveQueue.RemoveFirst();
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