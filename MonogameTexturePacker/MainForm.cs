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

namespace MonogameTexturePacker {
    public partial class MainForm : Form {
        private Workspace workspace;
        private Dictionary<WorkspaceFolder, TreeNode> folderToTree;
        private Dictionary<TreeNode, Sprite> treeToSprite;

        private TreeNode rootNode = null;
        private Sprite selectedSprite = null;
        private List<PictureBox> subImages = null;

        public MainForm(string[] arguments) {
            InitializeComponent();
            workspace = new Workspace();
            workspace.OnImportSprite += WorkspaceOnOnImportSprite;

            folderToTree = new Dictionary<WorkspaceFolder, TreeNode>();
            treeToSprite = new Dictionary<TreeNode, Sprite>();
            subImages = new List<PictureBox>();

            if (arguments.Length == 1) {
                string path = arguments[0];
                path = new FileInfo(path).FullName;
                LoadFolder(path);
            }
        }

        private void WorkspaceOnOnImportSprite(object sender, Workspace.ImportSpriteEventArgs e) {
            TreeNode newNode = new TreeNode(e.Sprite.Name);

            TreeNode parent = folderToTree[e.ParentFolder];
            treeToSprite.Add(newNode, e.Sprite);

            (parent == rootNode ? folderView.Nodes : parent.Nodes).Add(newNode);



            folderView.SelectedNode = newNode;
            PopulateSpriteProperties();
        }


        public TreeNode GetNodeFromFolder(WorkspaceFolder folder) {
            TreeNode[] subFolders = folder.subFolders.Select(x => GetNodeFromFolder(x)).ToArray();
            TreeNode[] files = folder.files.Select(x => new TreeNode(x.Name)).ToArray();
            files.Each((x,i) => treeToSprite.Add(x, folder.files[i]));

            TreeNode newNode = new TreeNode(folder.name, subFolders.Concat(files).ToArray());
            newNode.ImageIndex = 1;
            newNode.SelectedImageIndex = 1;

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

        private void PopulateSpriteProperties() {
            subFramesPreview.Controls.Clear();

            foreach (PictureBox box in subImages) {
                box.Dispose();
            }

            imagePreview.UpdateImage(selectedSprite.ImageAbsolutePaths[0]);;

            originX.Value = (decimal)selectedSprite.OriginX;
            originY.Value = (decimal)selectedSprite.OriginY;


            foreach (string path in selectedSprite.ImageAbsolutePaths) {
                PictureBox box = new PictureBox();
                box.ImageLocation = path;
                box.Height = 90;
                box.Width = 120;
                box.SizeMode = PictureBoxSizeMode.Zoom;
                subFramesPreview.Controls.Add(box);
            }

            nameBox.Text = selectedSprite.Name;
            UpdateOriginSelection();
        }

        private void UpdateOriginSelection() {
            int imageWidth = selectedSprite.Width-1;
            int imageHeight = selectedSprite.Height-1;

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
            }
            else {
                originSelectionDropdown.SelectedIndex = 1 + widthIndex + heightIndex * 3;
            }
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
            if (treeToSprite.ContainsKey(e.Node) == false) {
                return;
            }

            selectedSprite = treeToSprite[e.Node];
            imagePreview.selectedSprite = selectedSprite;
            PopulateSpriteProperties();
        }

        private void SpriteImport_Click(object sender, EventArgs e) {
            buttonImportSprites.Show();

            if (openSpriteDialog.ShowDialog() != DialogResult.OK) {
                return;
            }

            workspace.ImportSprites(openSpriteDialog.FileNames);
        }


        private void ImagePreview_MouseMove(object sender, MouseEventArgs e) {
            if ((e.Button & MouseButtons.Left) == 0) {
                return;
            }

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
            selectedSprite.OriginX = (float)originX.Value;
            UpdateOriginSelection();
            imagePreview.Refresh();
        }

        private void OriginY_ValueChanged(object sender, EventArgs e) {
            selectedSprite.OriginY = (float)originY.Value;
            UpdateOriginSelection();
            imagePreview.Refresh();
        }

        private void ButtonBuild_Click(object sender, EventArgs e) {
            workspace.Build("C:\\Program Files (x86)\\MSBuild\\MonoGame\\v3.0\\Tools\\MGCB.exe");
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e) {
            workspace.Save();
        }

        private void confirmNewName_Click(object sender, EventArgs e) {
            selectedSprite.SetNewName(nameBox.Text);
            PopulateSpriteProperties();
            TreeNode node = treeToSprite.FirstOrDefault(x => x.Value == selectedSprite).Key;
            node.Text = selectedSprite.Name;
        }

        private void nameBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                confirmNewName.PerformClick();
                e.Handled = true;
            }
        }


        private void originSelectionDropdown_SelectedIndexChanged(object sender, EventArgs e) {
            int index = originSelectionDropdown.SelectedIndex - 1;
            if (index == -1) {
                return;
            }
            int wIndex = index % 3;
            int hIndex = index / 3;

            selectedSprite.OriginX = (selectedSprite.Width -1) * wIndex / 2;
            selectedSprite.OriginY = (selectedSprite.Height-1) * hIndex / 2;

            originX.Value = (int)selectedSprite.OriginX;
            originY.Value = (int)selectedSprite.OriginY;
        }
    }
}
