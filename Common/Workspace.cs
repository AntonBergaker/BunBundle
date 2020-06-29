using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MonogameTexturePacker {
    public class Workspace {
        private string basePath;
        public WorkspaceFolder SelectedFolder { get; set; }
        private string targetFolder;

        public WorkspaceFolder RootFolder { protected set; get; }

        public bool Unsaved => updatedSprites.Count > 0;

        private string SaveFilePath => Path.Combine(RootFolder.path, "sprites.sprm");

        private readonly Dictionary<Sprite, UpdatedSpritesData> updatedSprites;

        public Workspace() {
            updatedSprites = new Dictionary<Sprite, UpdatedSpritesData>();
        }


        private Sprite[] GetAllSprites() {
            return GetAllSprites(RootFolder).ToArray();
        }

        private IEnumerable<Sprite> GetAllSprites(WorkspaceFolder rootFolder) {
            IEnumerable<Sprite> sprites = rootFolder.files;
            foreach (WorkspaceFolder folder in rootFolder.subFolders) {
                sprites = sprites.Concat(GetAllSprites(folder));
            }

            return sprites;
        }

        private WorkspaceFolder GetWorkspaceFolder(string path) {
            bool hasChild = false;
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

            return new WorkspaceFolder(Path.GetFileName(path), path, subFolders, files);
        }

        public void OpenFolder(string folder) {
            string[] files = Directory.GetFiles(folder, "sprites.sprm");
            if (files.Length >= 1) {
                OpenProjectFile(files[0]);
            }

            RootFolder = GetWorkspaceFolder(folder);

            if (RootFolder == null) {
                RootFolder = new WorkspaceFolder(Path.GetFileName(folder), folder, new List<WorkspaceFolder>(), new List<Sprite>());
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


        public void ImportSprites(string[] paths) {
            // Copy the file into the folder

            WorkspaceFolder pasteTarget = SelectedFolder ?? RootFolder;

            string newName = Path.GetFileNameWithoutExtension(paths[0]);

            Sprite spr = Sprite.Create(newName, paths, pasteTarget.path, this);

            pasteTarget.files.Add(spr);

            RaiseImportSprite(pasteTarget, spr);
        }

        public void CreateFolder() {

            WorkspaceFolder pasteTarget = SelectedFolder ?? RootFolder;

            string newName = "New Folder";

            for (int i = 0; i < 999; i++) {
                if (pasteTarget.subFolders.Any(x => x.name.Equals(newName, StringComparison.CurrentCultureIgnoreCase))) {
                    newName = "New Folder(" + i + ")";
                    continue;
                }

                break;
            }

            WorkspaceFolder folder = new WorkspaceFolder(newName, Path.Combine(pasteTarget.path, newName), new List<WorkspaceFolder>(), new List<Sprite>());
            if (!Directory.Exists(folder.path)) {
                Directory.CreateDirectory(folder.path);
            }

            pasteTarget.subFolders.Add(folder);

            RaiseAddFolder(pasteTarget, folder);
        }

        public void Build(string mgcbPath) {

            Builder builder = new Builder();

            builder.Build(GetAllSprites(), RootFolder.path, targetFolder, mgcbPath);

        }

        private void MoveFiles(Sprite sprite, string oldDirectory, string newDirectory) {

            if (Directory.Exists(newDirectory) == false) {
                Directory.CreateDirectory(newDirectory);
            }

            string newImgDir = Path.Combine(newDirectory, "img");

            if (!Directory.Exists(newImgDir)) {
                Directory.CreateDirectory(newImgDir);
            }

            string oldName = Path.GetFileName(oldDirectory);
            string newName = Path.GetFileName(newDirectory);

            File.Move(Path.Combine(oldDirectory, oldName) + ".spr", Path.Combine(newDirectory, newName) + ".spr");

            string[] absolutePaths = sprite.ImageAbsolutePaths;
            for (int i = 0; i < absolutePaths.Length; i++) {
                string newPath = newName + i + ".png";
                sprite.ImagePaths[i] = newPath;
                File.Move(absolutePaths[i], Path.Combine(newImgDir, newPath));
            }

            sprite.Path = newDirectory;

            Directory.Delete(oldDirectory, true);
        }

        public void Save() {

            var obj = new {
                targetDirectory = targetFolder
            };

            File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(obj), Encoding.UTF8);

            List<(Sprite, string)> tempLocations = new List<(Sprite, string)>();

            // Scan if we have too many sprites
            foreach (UpdatedSpritesData spriteData in updatedSprites.Values) {
                if (spriteData.CheckSprites == false) {
                    continue;
                }

                string[] paths = Directory.GetFiles(Path.Combine(spriteData.Sprite.Path, "img"));
                string[] truthTable = spriteData.Sprite.ImagePaths.Select(x => Path.GetFileName(x)).ToArray();

                foreach (string path in paths) {
                    string strippedPath = Path.GetFileName(path);

                    if (truthTable.Contains(strippedPath) == false) {
                        File.Delete(path);
                    }
                }
            }

            // Move sprites that need moving
            foreach (UpdatedSpritesData spriteData in updatedSprites.Values) {
                Sprite sprite = spriteData.Sprite;
                string newPath = "";

                if (Path.GetFileNameWithoutExtension(sprite.Path) != sprite.Name) {
                    newPath = Path.Combine( Path.GetDirectoryName(sprite.Path), sprite.Name);
                }

                if (newPath == "") {
                    continue;
                }

                // If it already exists wait to move until later, just in case something is moving from here
                if (Directory.Exists(newPath)) {
                    newPath = newPath + "-temp_for_moving_about";
                    tempLocations.Add((sprite, newPath));
                }

                MoveFiles(sprite, sprite.Path, newPath);
            }

            foreach ((Sprite sprite, string newPath) in tempLocations) {
                string stripped = newPath.Remove(newPath.LastIndexOf("-temp_for_moving_about"));
                MoveFiles(sprite, newPath, stripped);
            }

            foreach (UpdatedSpritesData sprite in updatedSprites.Values) {
                sprite.Sprite.Save();
                
            }

            updatedSprites.Clear();
        }

        public void AddUnsaved(Sprite sprite, bool checkSprites = false) {
            if (updatedSprites.ContainsKey(sprite)) {
                return;

            }
            updatedSprites.Add(sprite, new UpdatedSpritesData(sprite, "", checkSprites));
        }

        public class ImportSpriteEventArgs {
            public Sprite Sprite { get; }
            public WorkspaceFolder ParentFolder { get; }

            public ImportSpriteEventArgs(WorkspaceFolder parentFolder, Sprite sprite) {
                Sprite = sprite;
                ParentFolder = parentFolder;
            }
        }


        public event EventHandler<ImportSpriteEventArgs> OnImportSprite;

        private void RaiseImportSprite(WorkspaceFolder parentFolder, Sprite file) {
            OnImportSprite?.Invoke(this, new ImportSpriteEventArgs(parentFolder, file));
        }


        public class AddFolderEventArgs {
            public WorkspaceFolder Folder { get; }
            public WorkspaceFolder ParentFolder { get; }

            public AddFolderEventArgs(WorkspaceFolder parentFolder, WorkspaceFolder folder) {
                Folder = folder;
                ParentFolder = parentFolder;
            }
        }


        public event EventHandler<AddFolderEventArgs> OnAddFolder;

        private void RaiseAddFolder(WorkspaceFolder parentFolder, WorkspaceFolder folder) {
            OnAddFolder?.Invoke(this, new AddFolderEventArgs(parentFolder, folder));
        }

    }
}