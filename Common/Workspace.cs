using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BunBundle.Model {
    public class Workspace {
        private string basePath;
        private string targetFolder;

        public WorkspaceFolder RootFolder { protected set; get; }

        private readonly SavingManager savingManager;

        public bool Unsaved => savingManager.UnsavedChanges;

        private string SaveFilePath => Path.Combine(RootFolder.Path, "sprites.sprm");



        public Workspace() {
            savingManager = new SavingManager();
        }


        private Sprite[] GetAllSprites() {
            return RootFolder.GetAllSprites().ToArray();
        }

        private WorkspaceFolder GetWorkspaceFolder(string path) {
            List<WorkspaceFolder> subFolders = new List<WorkspaceFolder>();
            List<Sprite> files = new List<Sprite>();
            foreach (string dir in Directory.GetDirectories(path)) {
                if (Directory.GetFiles(dir, "*.spr").Length > 0) {
                    files.Add(new Sprite(Path.GetFileName(dir), dir, this));
                    continue;
                }

                WorkspaceFolder sub = GetWorkspaceFolder(dir);
                subFolders.Add(sub);
            }

            return new WorkspaceFolder(Path.GetFileName(path), path, this, subFolders, files);

        }

        public void OpenFolder(string folder) {
            string[] files = Directory.GetFiles(folder, "sprites.sprm");
            if (files.Length >= 1) {
                OpenProjectFile(files[0]);
            }

            RootFolder = GetWorkspaceFolder(folder);

            if (RootFolder == null) {
                RootFolder = new WorkspaceFolder(Path.GetFileName(folder), folder, this, new List<WorkspaceFolder>(), new List<Sprite>());
            }
        }

        public void OpenFile(string file) {
            OpenFolder(Path.GetDirectoryName(file));
        }

        private void OpenProjectFile(string file) {
            JObject obj = JObject.Parse(File.ReadAllText(file));
            targetFolder = (string)obj["targetDirectory"];
            targetFolder = targetFolder.Replace('\\', Path.DirectorySeparatorChar);
        }


        public void ImportSprites(string[] paths, WorkspaceFolder target) {
            // Copy the file into the folder

            string newName = Path.GetFileNameWithoutExtension(paths[0]);

            Sprite spr = Sprite.Create(newName, paths, target, this);

            target.files.Add(spr);

            RaiseImportSprite(target, spr);
        }

        public void CreateFolder(WorkspaceFolder target) {

            string newName = "New Folder";

            for (int i = 0; i < 999; i++) {
                if (target.subFolders.Any(x => x.Name.Equals(newName, StringComparison.CurrentCultureIgnoreCase))) {
                    newName = "New Folder(" + i + ")";
                    continue;
                }

                break;
            }

            WorkspaceFolder folder = new WorkspaceFolder(newName, Path.Combine(target.Path, newName), this, new List<WorkspaceFolder>(), new List<Sprite>());
            if (!Directory.Exists(folder.Path)) {
                Directory.CreateDirectory(folder.Path);
            }

            target.subFolders.Add(folder);

            RaiseAddFolder(target, folder);
        }

        public void Build() {

            Builder builder = new Builder();

            builder.Build(RootFolder, RootFolder.Path, targetFolder);

        }

        public void Save() {

            var obj = new {
                targetDirectory = targetFolder
            };

            File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(obj), Encoding.UTF8);

            savingManager.Save();
            
            RaiseUnsavedChanged();
        }

        public void AddSaveAction(SaveAction saveAction) {
            savingManager.AddSaveAction(saveAction);

            RaiseUnsavedChanged();
        }


        public event EventHandler<(WorkspaceFolder parentFolder, Sprite sprite)> OnImportSprite;

        private void RaiseImportSprite(WorkspaceFolder parentFolder, Sprite file) {
            OnImportSprite?.Invoke(this, (parentFolder, file));
        }


        public event EventHandler<(WorkspaceFolder parentFolder, WorkspaceFolder folder)> OnAddFolder;

        private void RaiseAddFolder(WorkspaceFolder parentFolder, WorkspaceFolder folder) {
            OnAddFolder?.Invoke(this, (parentFolder, folder));
        }


        public event EventHandler<bool> OnUnsavedChanged;

        private void RaiseUnsavedChanged() {
            OnUnsavedChanged?.Invoke(this, Unsaved);
        }
    }
}