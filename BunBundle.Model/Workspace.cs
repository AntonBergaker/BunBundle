using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using BunBundle.Model.Saving;

namespace BunBundle.Model {
    public class Workspace {
        private Settings settings;

        public WorkspaceFolder RootFolder { protected set; get; }

        private readonly SavingManager savingManager;

        public bool Unsaved => savingManager.UnsavedChanges;

        private string SaveFilePath => Path.Combine(RootFolder.StorageFolder.Path, "sprites.bubu");



        public Workspace(string path) {
            savingManager = new SavingManager();

            if (File.Exists(path)) {
                string? dirname = Path.GetDirectoryName(path);
                if (dirname == null) {
                    throw new FileNotFoundException("Directory/File not found");
                }

                Open(path);
            } 
            else {
                throw new FileNotFoundException("Directory/File not found");
            }

        }





        public static void CreateNew(string path) {
            Settings settings = new Settings("", "content/Generated/", "Sprites.cs", "Sprites", "");
            WriteSaveFile(path, settings);
        }

        [MemberNotNull(nameof(RootFolder))]
        [MemberNotNull(nameof(settings))]
        private void Open(string file) {
            ImportSettings(file);

            RootFolder = WorkspaceFolder.Import(Path.GetDirectoryName(file)!, this);
        }

        private void ImportSettings(string file) {
            string text;
            try {
                text = File.ReadAllText(file);
            }
            catch (Exception ex) {
                RaiseError(new Error(ex));
                return;
            }

            Settings? obj;
            try {
                obj = JsonSerializer.Deserialize<Settings>(text,
                    JsonSettings.GetSerializeOptions());
            }
            catch (Exception ex) {
                RaiseError(new Error(ex));
                return;
            }

            if (obj == null) {
                RaiseError(new Error("Failed to read settings file"));
                return;
            }

            settings = obj;
        }

        private static void WriteSaveFile(string path, Settings settings) {
            File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonSettings.GetSerializeOptions()), Encoding.UTF8);
        }
        
        public void Save() {
            try {
                WriteSaveFile(SaveFilePath, settings);
            }
            catch (Exception ex) {
                RaiseError(new Error(ex));
            }

            savingManager.RunActions();

            RaiseUnsavedChanged();
        }


        public void ImportSprites(string[] paths, WorkspaceFolder target) {
            // Copy the file into the folder

            string newName = Path.GetFileNameWithoutExtension(paths[0]);

            Sprite spr = Sprite.Create(newName, paths, target);

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

            WorkspaceFolder folder = new WorkspaceFolder(newName, target);
            if (!Directory.Exists(folder.Storage.Path)) {
                Directory.CreateDirectory(folder.Storage.Path);
            }

            target.subFolders.Add(folder);

            RaiseAddFolder(target, folder);
        }

        public void Build() {
            if (string.IsNullOrEmpty(settings.TargetDirectory)) {
                RaiseError(new Error("The field target_directory is not set for the current project."));
                return;
            }
            if (string.IsNullOrEmpty(settings.Namespace)) {
                RaiseError(new Error("The field namespace is not set for the current project."));
                return;
            }

            Builder builder = new Builder(settings);

            try {
                builder.Build(RootFolder, RootFolder.Storage.Path);
            }
            catch (Exception ex) {
                RaiseError(new Error(ex));
            }
        }


        public void AddSaveAction(SaveAction saveAction) {
            savingManager.Add(saveAction);

            RaiseUnsavedChanged();
        }


        public event EventHandler<(WorkspaceFolder parentFolder, Sprite sprite)>? OnImportSprite;

        private void RaiseImportSprite(WorkspaceFolder parentFolder, Sprite file) {
            OnImportSprite?.Invoke(this, (parentFolder, file));
        }


        public event EventHandler<(WorkspaceFolder parentFolder, WorkspaceFolder folder)>? OnAddFolder;

        private void RaiseAddFolder(WorkspaceFolder parentFolder, WorkspaceFolder folder) {
            OnAddFolder?.Invoke(this, (parentFolder, folder));
        }


        public event EventHandler<bool>? OnUnsavedChanged;

        private void RaiseUnsavedChanged() {
            OnUnsavedChanged?.Invoke(this, Unsaved);
        }

        public event EventHandler<Error>? OnError;

        private void RaiseError(Error error) {
            OnError?.Invoke(this, error);
        }
    }

}