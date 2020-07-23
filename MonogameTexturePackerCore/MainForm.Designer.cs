namespace MonogameTexturePackerCore {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderView = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.sizeLabel = new System.Windows.Forms.Label();
            this.replaceImages = new System.Windows.Forms.Button();
            this.originSelectionDropdown = new System.Windows.Forms.ComboBox();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.subFramesPreview = new System.Windows.Forms.FlowLayoutPanel();
            this.originY = new System.Windows.Forms.NumericUpDown();
            this.originX = new System.Windows.Forms.NumericUpDown();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonImportSprites = new System.Windows.Forms.Button();
            this.openSpriteDialog = new System.Windows.Forms.OpenFileDialog();
            this.buttonAddFolder = new System.Windows.Forms.Button();
            this.buttonBuild = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.originY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.originX)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(888, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.loadToolStripMenuItem.Text = "Open Folder...";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.saveToolStripMenuItem.Text = "Save...";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // folderView
            // 
            this.folderView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderView.FullRowSelect = true;
            this.folderView.ImageIndex = 0;
            this.folderView.ImageList = this.imageList1;
            this.folderView.Location = new System.Drawing.Point(4, 3);
            this.folderView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.folderView.Name = "folderView";
            this.folderView.SelectedImageIndex = 0;
            this.folderView.Size = new System.Drawing.Size(284, 520);
            this.folderView.TabIndex = 1;
            this.folderView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FolderView_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.folderView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.sizeLabel);
            this.splitContainer1.Panel2.Controls.Add(this.replaceImages);
            this.splitContainer1.Panel2.Controls.Add(this.originSelectionDropdown);
            this.splitContainer1.Panel2.Controls.Add(this.nameBox);
            this.splitContainer1.Panel2.Controls.Add(this.subFramesPreview);
            this.splitContainer1.Panel2.Controls.Add(this.originY);
            this.splitContainer1.Panel2.Controls.Add(this.originX);
            this.splitContainer1.Size = new System.Drawing.Size(888, 524);
            this.splitContainer1.SplitterDistance = 292;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 3;
            // 
            // sizeLabel
            // 
            this.sizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sizeLabel.Location = new System.Drawing.Point(8, 501);
            this.sizeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.sizeLabel.Name = "sizeLabel";
            this.sizeLabel.Size = new System.Drawing.Size(574, 23);
            this.sizeLabel.TabIndex = 12;
            this.sizeLabel.Text = "256x256";
            this.sizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // replaceImages
            // 
            this.replaceImages.Location = new System.Drawing.Point(8, 81);
            this.replaceImages.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.replaceImages.Name = "replaceImages";
            this.replaceImages.Size = new System.Drawing.Size(100, 51);
            this.replaceImages.TabIndex = 11;
            this.replaceImages.Text = "Replace Image(s)";
            this.replaceImages.UseVisualStyleBackColor = true;
            this.replaceImages.Click += new System.EventHandler(this.replaceImages_Click);
            // 
            // originSelectionDropdown
            // 
            this.originSelectionDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.originSelectionDropdown.FormattingEnabled = true;
            this.originSelectionDropdown.Items.AddRange(new object[] {
            "Custom",
            "Top Left",
            "Top Center",
            "Top Right",
            "Middle Left",
            "Middle Center",
            "Middle Right",
            "Bot Left",
            "Bot Center",
            "Bot Right"});
            this.originSelectionDropdown.Location = new System.Drawing.Point(296, 32);
            this.originSelectionDropdown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.originSelectionDropdown.Name = "originSelectionDropdown";
            this.originSelectionDropdown.Size = new System.Drawing.Size(140, 23);
            this.originSelectionDropdown.TabIndex = 10;
            this.originSelectionDropdown.SelectedIndexChanged += new System.EventHandler(this.originSelectionDropdown_SelectedIndexChanged);
            // 
            // nameBox
            // 
            this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameBox.Location = new System.Drawing.Point(4, 3);
            this.nameBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(571, 23);
            this.nameBox.TabIndex = 8;
            this.nameBox.TextChanged += new System.EventHandler(this.nameBox_TextChanged);
            this.nameBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.nameBox_KeyDown);
            // 
            // subFramesPreview
            // 
            this.subFramesPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subFramesPreview.Location = new System.Drawing.Point(121, 70);
            this.subFramesPreview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.subFramesPreview.Name = "subFramesPreview";
            this.subFramesPreview.Size = new System.Drawing.Size(461, 115);
            this.subFramesPreview.TabIndex = 7;
            // 
            // originY
            // 
            this.originY.DecimalPlaces = 2;
            this.originY.Location = new System.Drawing.Point(149, 33);
            this.originY.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.originY.Name = "originY";
            this.originY.Size = new System.Drawing.Size(140, 23);
            this.originY.TabIndex = 6;
            this.originY.ValueChanged += new System.EventHandler(this.OriginY_ValueChanged);
            // 
            // originX
            // 
            this.originX.DecimalPlaces = 2;
            this.originX.Location = new System.Drawing.Point(2, 33);
            this.originX.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.originX.Name = "originX";
            this.originX.Size = new System.Drawing.Size(140, 23);
            this.originX.TabIndex = 5;
            this.originX.ValueChanged += new System.EventHandler(this.OriginX_ValueChanged);
            // 
            // buttonImportSprites
            // 
            this.buttonImportSprites.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImportSprites.Location = new System.Drawing.Point(4, 558);
            this.buttonImportSprites.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonImportSprites.Name = "buttonImportSprites";
            this.buttonImportSprites.Size = new System.Drawing.Size(138, 27);
            this.buttonImportSprites.TabIndex = 4;
            this.buttonImportSprites.Text = "Import Images";
            this.buttonImportSprites.UseVisualStyleBackColor = true;
            this.buttonImportSprites.Click += new System.EventHandler(this.SpriteImport_Click);
            // 
            // openSpriteDialog
            // 
            this.openSpriteDialog.DefaultExt = "png";
            this.openSpriteDialog.FileName = "openSpriteDialog";
            this.openSpriteDialog.Filter = "Image Files|*.png|All files|*.*";
            this.openSpriteDialog.Multiselect = true;
            // 
            // buttonAddFolder
            // 
            this.buttonAddFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddFolder.Location = new System.Drawing.Point(148, 558);
            this.buttonAddFolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonAddFolder.Name = "buttonAddFolder";
            this.buttonAddFolder.Size = new System.Drawing.Size(88, 27);
            this.buttonAddFolder.TabIndex = 5;
            this.buttonAddFolder.Text = "Add Folder";
            this.buttonAddFolder.UseVisualStyleBackColor = true;
            this.buttonAddFolder.Click += new System.EventHandler(this.buttonAddFolder_Click);
            // 
            // buttonBuild
            // 
            this.buttonBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBuild.Location = new System.Drawing.Point(786, 557);
            this.buttonBuild.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonBuild.Name = "buttonBuild";
            this.buttonBuild.Size = new System.Drawing.Size(88, 27);
            this.buttonBuild.TabIndex = 6;
            this.buttonBuild.Text = "Build";
            this.buttonBuild.UseVisualStyleBackColor = true;
            this.buttonBuild.Click += new System.EventHandler(this.ButtonBuild_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(888, 588);
            this.Controls.Add(this.buttonBuild);
            this.Controls.Add(this.buttonAddFolder);
            this.Controls.Add(this.buttonImportSprites);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MainForm";
            this.Text = "SpritePacker";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.originY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.originX)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.TreeView folderView;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private PreviewImage imagePreview;
        private System.Windows.Forms.NumericUpDown originY;
        private System.Windows.Forms.NumericUpDown originX;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Button buttonImportSprites;
        private System.Windows.Forms.OpenFileDialog openSpriteDialog;
        private System.Windows.Forms.Button buttonAddFolder;
        private System.Windows.Forms.FlowLayoutPanel subFramesPreview;
        private System.Windows.Forms.Button buttonBuild;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ComboBox originSelectionDropdown;
        private System.Windows.Forms.Button replaceImages;
        private System.Windows.Forms.Label sizeLabel;
    }
}

