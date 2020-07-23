using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonogameTexturePacker;

namespace MonogameTexturePackerCore {
    public partial class MainForm : Form {
        private Workspace workspace;
        private Dictionary<WorkspaceFolder, TreeNode> folderToTree;

        private readonly Dictionary<TreeNode, IWorkspaceItem> treeToItem;

        private TreeNode rootNode = null;
        private IWorkspaceItem selectedItem = null;
        private List<PictureBox> subImages = null;

        private readonly Control[] SpriteControls;

        public MainForm(string[] arguments) {
            InitializeComponent();
            workspace = new Workspace();
            workspace.OnImportSprite += WorkspaceOnOnImportSprite;
            workspace.OnAddFolder += WorkspaceOnOnAddFolder;

            this.KeyPreview = true;
            this.KeyDown += OnKeyDown;

            SpriteControls = new Control[] { imagePreview, sizeLabel, originSelectionDropdown, originX, originY, replaceImages, subFramesPreview };

            folderToTree = new Dictionary<WorkspaceFolder, TreeNode>();
            treeToItem = new Dictionary<TreeNode, IWorkspaceItem>();
            subImages = new List<PictureBox>();

            if (arguments.Length == 1) {
                string path = arguments[0];
                path = new FileInfo(path).FullName;
                LoadFolder(path);
            }

            UpdateTitle();


        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Control && e.KeyCode == Keys.S)       // Ctrl-S Save
            {
                Save();
                e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
            }
        }

        private void WorkspaceOnOnImportSprite(object sender, Workspace.ImportSpriteEventArgs e) {
            TreeNode newNode = new TreeNode(e.Sprite.Name);

            TreeNode parent = folderToTree[e.ParentFolder];
            treeToItem.Add(newNode, e.Sprite);

            (parent == rootNode ? folderView.Nodes : parent.Nodes).Add(newNode);



            folderView.SelectedNode = newNode;
            PopulateItemProperties();
        }


        private void WorkspaceOnOnAddFolder(object sender, Workspace.AddFolderEventArgs e) {
            TreeNode newNode = GetNodeFromFolder(e.Folder);

            TreeNode parent = folderToTree[e.ParentFolder];
            treeToItem.Add(newNode, e.Folder);

            (parent == rootNode ? folderView.Nodes : parent.Nodes).Add(newNode);

            folderView.SelectedNode = newNode;

            PopulateItemProperties();
        }


        private void UpdateTitle() {
            if (workspace.Unsaved) {
                this.Text = "SpritePacker*";
            } else {
                this.Text = "SpritePacker";
            }
        }


        public TreeNode GetNodeFromFolder(WorkspaceFolder folder) {
            TreeNode[] subFolders = folder.subFolders.Select(x => GetNodeFromFolder(x)).ToArray();
            TreeNode[] files = folder.files.Select(x => new TreeNode(x.Name)).ToArray();
            files.Each((x, i) => treeToItem.Add(x, folder.files[i]));

            TreeNode newNode = new TreeNode(folder.Name, subFolders.Concat(files).ToArray());
            newNode.ImageIndex = 1;
            newNode.SelectedImageIndex = 1;

            treeToItem.Add(newNode, folder);

            folderToTree.Add(folder, newNode);

            return newNode;

        }

        private void PopulateTreeView() {
            rootNode = GetNodeFromFolder(workspace.RootFolder);
            folderView.Nodes.Clear();
            foreach (TreeNode node in rootNode.Nodes) {
                folderView.Nodes.Add(node);
            }
        }

        private void PopulateItemProperties() {
            if (selectedItem is Sprite sprite) {
                PopulateSpriteProperties(sprite);
            } else if (selectedItem is WorkspaceFolder folder) {
                PopulateFolderProperties(folder);
            }
        }

        private void PopulateSpriteProperties(Sprite selectedSprite) {
            SpriteControls.Each(x => x.Visible = true);

            subFramesPreview.Controls.Clear();

            foreach (PictureBox box in subImages) {
                box.Dispose();
            }

            imagePreview.UpdateImage(selectedSprite.ImageAbsolutePaths[0]); ;

            originX.Value = (decimal)selectedSprite.OriginX;
            originY.Value = (decimal)selectedSprite.OriginY;

            sizeLabel.Text = selectedSprite.Width + " x " + selectedSprite.Height;

            foreach (string path in selectedSprite.ImageAbsolutePaths) {
                PictureBox box = new PictureBox();
                box.ImageLocation = path;
                box.Height = 90;
                box.Width = 120;
                box.SizeMode = PictureBoxSizeMode.Zoom;
                subFramesPreview.Controls.Add(box);
            }

            nameBox.Text = selectedSprite.Name;
            UpdateOriginSelection(selectedSprite);
        }

        private void PopulateFolderProperties(WorkspaceFolder folder) {
            SpriteControls.Each(x => x.Visible = false);

            nameBox.Text = selectedItem.Name;
        }

        private void UpdateOriginSelection(Sprite selectedSprite) {
            int imageWidth = selectedSprite.Width - 1;
            int imageHeight = selectedSprite.Height - 1;

            int widthIndex = -1;

            if (imageWidth == 0) {
                widthIndex = 0;
            } else if (imageWidth == (int)selectedSprite.OriginX / 2) {
                widthIndex = 1;
            } else if (imageWidth == (int)selectedSprite.OriginX) {
                widthIndex = 2;
            }

            int heightIndex = -1;

            if (imageHeight == 0) {
                widthIndex = 0;
            } else if (imageHeight == (int)selectedSprite.OriginY / 2) {
                widthIndex = 1;
            } else if (imageHeight == (int)selectedSprite.OriginY) {
                widthIndex = 2;
            }

            if (widthIndex == -1 || heightIndex == -1) {
                originSelectionDropdown.SelectedIndex = 0;
            } else {
                originSelectionDropdown.SelectedIndex = 1 + widthIndex + heightIndex * 3;
            }

            UpdateTitle();
        }

        public void LoadFolder(string path) {
            workspace.OpenFolder(path);
            PopulateTreeView();
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e) {
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK) {
                return;
            }

            LoadFolder(folderBrowserDialog.SelectedPath);
        }

        private void FolderView_AfterSelect(object sender, TreeViewEventArgs e) {

            treeToItem.TryGetValue(e.Node, out IWorkspaceItem item);
            selectedItem = item;

            if (selectedItem is Sprite sprite) {
                imagePreview.selectedSprite = sprite;
            }

            PopulateItemProperties();
        }

        private void SpriteImport_Click(object sender, EventArgs e) {
            buttonImportSprites.Show();

            if (openSpriteDialog.ShowDialog() != DialogResult.OK) {
                return;
            }

            workspace.ImportSprites(openSpriteDialog.FileNames);
            UpdateTitle();
        }


        private void ImagePreview_MouseMove(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == 0) {
                return;
            }

            if (selectedItem == null) {
                return;
            }

            Sprite selectedSprite = selectedItem as Sprite;

            if (selectedSprite == null) {
                return;
            }

            Point p = imagePreview.GetImagePoint();

            selectedSprite.OriginX = p.X;
            selectedSprite.OriginY = p.Y;

            originX.Value = p.X;
            originY.Value = p.Y;

            imagePreview.Refresh();
        }


        private void OriginX_ValueChanged(object sender, EventArgs e) {
            if (selectedItem is Sprite sprite) {
                sprite.OriginX = (float)originX.Value;
                UpdateOriginSelection(sprite);
                imagePreview.Refresh();
                UpdateTitle();
            }
        }

        private void OriginY_ValueChanged(object sender, EventArgs e) {
            if (selectedItem is Sprite sprite) {
                sprite.OriginY = (float)originY.Value;
                UpdateOriginSelection(sprite);
                imagePreview.Refresh();
                UpdateTitle();
            }
        }

        private void ButtonBuild_Click(object sender, EventArgs e) {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            workspace.Build(Path.Combine(path, ".dotnet\\tools\\.store\\dotnet-mgcb-editor\\3.8.0.1375-develop\\dotnet-mgcb-editor\\3.8.0.1375-develop\\tools\\netcoreapp3.1\\any\\mgcb.exe"));
        }

        private void Save() {
            workspace.Save();
            UpdateTitle();
            if (selectedItem is Sprite sprite) {
                imagePreview.UpdateImage(sprite.ImageAbsolutePaths[0]);
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e) {
            Save();
        }

        private void nameBox_KeyDown(object sender, KeyEventArgs e) {

        }

        private void originSelectionDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            Sprite selectedSprite = selectedItem as Sprite;

            if (selectedSprite == null) {
                return;
            }

            int index = originSelectionDropdown.SelectedIndex - 1;
            if (index == -1) {
                return;
            }
            int wIndex = index % 3;
            int hIndex = index / 3;

            selectedSprite.OriginX = (selectedSprite.Width - 1) * wIndex / 2;
            selectedSprite.OriginY = (selectedSprite.Height - 1) * hIndex / 2;

            originX.Value = (int)selectedSprite.OriginX;
            originY.Value = (int)selectedSprite.OriginY;

            UpdateTitle();
        }

        private void nameBox_TextChanged(object sender, EventArgs e) {
            if (selectedItem == null) {
                return;
            }

            selectedItem.Name = nameBox.Text;

            PopulateItemProperties();
            TreeNode node = treeToItem.FirstOrDefault(x => x.Value == selectedItem).Key;
            node.Text = selectedItem.Name;
            UpdateTitle();

        }

        private void replaceImages_Click(object sender, EventArgs e) {
            Sprite selectedSprite = selectedItem as Sprite;

            if (selectedSprite == null) {
                return;
            }

            DialogResult result = openSpriteDialog.ShowDialog();

            if (result != DialogResult.OK) {
                return;
            }

            Save();
            selectedSprite.ClearImages();
            selectedSprite.AddImages(openSpriteDialog.FileNames);

            Save();
        }

        private void buttonAddFolder_Click(object sender, EventArgs e) {
            workspace.CreateFolder();
        }
    }
}
