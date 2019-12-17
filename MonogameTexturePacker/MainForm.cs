using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
                LoadFolder(arguments[0]);
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
            imagePreview.Refresh();
        }

        private void OriginY_ValueChanged(object sender, EventArgs e) {
            selectedSprite.OriginY = (float)originY.Value;
            imagePreview.Refresh();
        }

        private void ButtonBuild_Click(object sender, EventArgs e) {
            workspace.Build();
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
    }
}
