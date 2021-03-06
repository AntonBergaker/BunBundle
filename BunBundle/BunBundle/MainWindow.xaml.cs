﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BunBundle.Annotations;
using Microsoft.Win32;
using BunBundle.Model;
using MaterialDesignThemes.Wpf;

namespace BunBundle {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        private Workspace? workspace;

        private Workspace? Workspace {
            get => workspace;
            set {
                workspace = value;
                if (workspace != null) {
                    workspace.OnImportSprite += WorkspaceOnOnImportSprite;
                    workspace.OnAddFolder += WorkspaceOnOnAddFolder;
                    workspace.OnUnsavedChanged += WorkspaceOnOnUnsavedChanged;
                    workspace.OnError += WorkspaceOnError;
                    PopulateTreeView();
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(AnyVisibility));
            }
        }

        private readonly Dictionary<IWorkspaceItem, TreeItemViewModel> itemToViewModels;

        private readonly PreviewImage previewImageHandler;

        private TreeItemViewModel? selectedItem;

        public Visibility SpriteVisibility => selectedItem is SpriteViewModel ? Visibility.Visible : Visibility.Hidden;
        public Visibility AnyVisibility => Workspace != null ? Visibility.Visible : Visibility.Hidden;

        public Visibility AnySelected => selectedItem != null ? Visibility.Visible : Visibility.Hidden;

        public SpriteViewModel? SelectedSprite => selectedItem as SpriteViewModel;

        public ObservableCollection<string> RecentProjects { get; set; }

        public TreeItemViewModel? SelectedItem {
            get => selectedItem;
            set {
                selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SpriteVisibility));
                OnPropertyChanged(nameof(SelectedSprite));
                OnPropertyChanged(nameof(AnySelected));
            }
        }

        //private List<PictureBox> subImages = null;

        private FolderViewModel? rootViewModel;

        public FolderViewModel? RootViewModel {
            get => rootViewModel;
            set {
                rootViewModel = value;
                OnPropertyChanged();
            }
        }

        private bool isDarkMode;
        public bool IsDarkMode {
            get => isDarkMode;
            set {
                isDarkMode = value;
                OnPropertyChanged();
            }
        }


        private bool preventRedraw;

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow(string[] arguments) {
            InitializeComponent();

            previewImageHandler = new PreviewImage(imagePreview);

            previewImageHandler.OnOriginSet += OnOriginSet;
            
            this.KeyDown += OnKeyDown;

            itemToViewModels = new Dictionary<IWorkspaceItem, TreeItemViewModel>();

            if (arguments.Length == 1) {
                string path = arguments[0];
                path = new FileInfo(path).FullName;
                LoadFile(path);
            }

            preventRedraw = false;


            IsDarkMode = Settings.Default.IsDark;
            RecentProjects = new ObservableCollection<string>();
            if (Settings.Default.RecentFiles != null) {
                foreach (string? file in Settings.Default.RecentFiles) {
                    if (file != null) {
                        RecentProjects.Add(file);
                    }
                }

                ModifyTheme();
            }
        }

        private void WorkspaceOnOnUnsavedChanged(object? sender, bool unsaved) {
            if (workspace != null && workspace.Unsaved) {
                this.Title = "BunBundle*";
            } else {
                this.Title = "BunBundle";
            }
        }

        private void WorkspaceOnError(object? sender, Error e) {
            MessageBox.Show(e.ErrorMessage);
        }

        private void ModifyTheme() {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkMode ? Theme.Dark : Theme.Light);

            paletteHelper.SetTheme(theme);
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)       // Ctrl-S Save
            {
                Save();
                e.Handled = true;  // Stops other controls on the form receiving event.
            }
        }

        private void WorkspaceOnOnImportSprite(object? sender, (WorkspaceFolder parentFolder, Sprite sprite) e) {
            FolderViewModel parent = (FolderViewModel)itemToViewModels[e.parentFolder];
            SpriteViewModel newNode = new SpriteViewModel(e.sprite, parent);

            itemToViewModels.Add(e.sprite, newNode);

            parent?.AddItem(newNode);

            newNode.IsSelected = true;
            PopulateItemProperties();
        }


        private void WorkspaceOnOnAddFolder(object? sender, (WorkspaceFolder parentFolder, WorkspaceFolder folder) e) {
            FolderViewModel parent = (FolderViewModel)itemToViewModels[e.parentFolder];


            FolderViewModel newNode = new FolderViewModel(e.folder, parent);

            itemToViewModels.Add(e.folder, newNode);

            parent?.AddItem(newNode);

            newNode.IsSelected = true;
            PopulateItemProperties();
        }

        private void Save() {
            Workspace?.Save();
        }

        private void LoadFile(string path) {
            if (Settings.Default.RecentFiles == null) {
                Settings.Default.RecentFiles = new StringCollection();
            }
            // If there's an old one remove it
            Settings.Default.RecentFiles.Remove(path);
            Settings.Default.RecentFiles.Insert(0, path);
            while (Settings.Default.RecentFiles.Count > 10) {
                Settings.Default.RecentFiles.RemoveAt(10);
            }
            Settings.Default.Save();

            try {
                Workspace = new Workspace(path);
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }

        }

        private void PopulateTreeView() {
            if (workspace == null) {
                return;
            }
            rootViewModel = new FolderViewModel(workspace.RootFolder, null);
            folderView.ItemsSource = rootViewModel.Items;
            AddToDictionary(rootViewModel);
        }

        private void AddToDictionary(TreeItemViewModel viewModel) {
            itemToViewModels.Add(viewModel.Source, viewModel);
            if (viewModel is FolderViewModel folder) {
                foreach (TreeItemViewModel item in folder.Items) {
                    AddToDictionary(item);
                }
            }
        }

        private void PopulateItemProperties() {
            if (SelectedItem is SpriteViewModel sprite) {
                PopulateSpriteProperties(sprite);
            } else if (SelectedItem is FolderViewModel folder) {
                PopulateFolderProperties(folder.Folder);
            }
        }

        private void PopulateSpriteProperties(SpriteViewModel selectedSprite) {
            preventRedraw = true;

            UpdateOriginSelection(selectedSprite);

            labelTextureSize.Content = selectedSprite.Sprite.Width + " x " + selectedSprite.Sprite.Height;


            textBoxName.SetCurrentValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Sprite Name");
            textBoxName.Text = selectedSprite.Name;

            preventRedraw = false;

            if (SelectedItem is SpriteViewModel sprite) {
                previewImageHandler.SetImage(sprite.Sprite, 0);
            }
        }

        private void PopulateFolderProperties(WorkspaceFolder folder) {
            textBoxName.SetCurrentValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Folder Name");
            textBoxName.Text = folder.Name;
        }

        private void UpdateOriginSelection(SpriteViewModel selectedSprite) {
            int imageWidth = selectedSprite.Width;
            int imageHeight = selectedSprite.Height;

            int widthIndex = -1;

            int oX = (int)selectedSprite.OriginX;
            int oY = (int)selectedSprite.OriginY;

            if (oX == 0) {
                widthIndex = 0;
            } else if (oX == imageWidth/2) {
                widthIndex = 1;
            } else if (oX == imageWidth) {
                widthIndex = 2;
            }

            int heightIndex = -1;

            if (oY == 0) {
                heightIndex = 0;
            } else if (oY == imageHeight/2) {
                heightIndex = 1;
            } else if (oY == imageHeight) {
                heightIndex = 2;
            }

            if (widthIndex == -1 || heightIndex == -1) {
                comboBoxOrigin.SelectedIndex = 0;
            } else {
                comboBoxOrigin.SelectedIndex = 1 + widthIndex + heightIndex * 3;
            }
        }

        private void buttonBuild_Click(object sender, RoutedEventArgs e) {
            Workspace?.Build();
        }

        private void folderView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            preventRedraw = true;
            SelectedItem = e.NewValue as TreeItemViewModel;

            PopulateItemProperties();
        }

        private void textBoxName_TextChanged(object sender, TextChangedEventArgs e) {
            if (SelectedItem == null) {
                return;
            }

            SelectedItem.Name = textBoxName.Text;
        }

        private void textBoxOriginX_TextChanged(object sender, TextChangedEventArgs e) {
            if (SelectedItem is SpriteViewModel sprite) {
                if (float.TryParse(textBoxOriginX.Text, out float result)) {
                    sprite.OriginX = result;
                }

                UpdateOriginSelection(sprite);
                if (preventRedraw == false) {
                    previewImageHandler.Redraw();
                }
            }
        }

        private void textBoxOriginY_TextChanged(object sender, TextChangedEventArgs e) {
            if (SelectedItem is SpriteViewModel sprite) {
                if (float.TryParse(textBoxOriginY.Text, out float result)) {
                    sprite.OriginY = result;
                }

                UpdateOriginSelection(sprite);
                if (preventRedraw == false) {
                    previewImageHandler.Redraw();
                }
            }
        }

        private void OnOriginSet(object? sender, Point e) {
            if (SelectedItem is SpriteViewModel selectedSprite) {
                selectedSprite.OriginX = (float)e.X;
                selectedSprite.OriginY = (float)e.Y;

                UpdateOriginSelection(selectedSprite);

                previewImageHandler.Redraw();
            }
        }

        private void NumberOnlyPreview(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9\\-.]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void comboBoxOrigin_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SpriteViewModel? selectedSprite = (SelectedItem as SpriteViewModel);

            if (selectedSprite == null) {
                return;
            }

            int index = comboBoxOrigin.SelectedIndex - 1;
            if (index == -1) {
                return;
            }
            int wIndex = index % 3;
            int hIndex = index / 3;

            selectedSprite.OriginX = ((selectedSprite.Width) * wIndex / 2f);
            selectedSprite.OriginY = (selectedSprite.Height) * hIndex / 2f;

        }

        private FolderViewModel? GetSelectedFolder() {
            switch (SelectedItem) {
                case FolderViewModel folder:
                    return folder;
                case SpriteViewModel sprite:
                    return sprite.Parent;
                default:
                    return rootViewModel;
            }
        }

        private void buttonNewFolder_Click(object sender, RoutedEventArgs e) {
            FolderViewModel? folder = GetSelectedFolder();
            if (folder == null) {
                return;
            }

            Workspace?.CreateFolder(folder.Folder);
        }

        private OpenFileDialog MakeOpenSpritesDialog() {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "PNG Files (*.png)|*.png";
            dialog.DefaultExt = "PNG Files (*.png)|*.png";
            return dialog;
        }

        private void buttonNewSprite_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = MakeOpenSpritesDialog();

            if (dialog.ShowDialog() == true) {
                if (workspace == null) {
                    return;
                }

                FolderViewModel? folder = GetSelectedFolder();
                if (folder == null) {
                    return;
                }
                Workspace?.ImportSprites(dialog.FileNames, folder.Folder);
            }
        }

        private void AddSpritesButton_Click(object sender, RoutedEventArgs e) {
            SpriteViewModel? sprite = selectedItem as SpriteViewModel;

            if (sprite == null) {
                return;
            }

            OpenFileDialog dialog = MakeOpenSpritesDialog();

            if (dialog.ShowDialog() == true) {
                sprite.AddImages(dialog.FileNames);
            }
        }

        private void ReplaceSpritesButton_Click(object sender, RoutedEventArgs e) {
            SpriteViewModel? sprite = selectedItem as SpriteViewModel;

            if (sprite == null) {
                return;
            }

            OpenFileDialog dialog = MakeOpenSpritesDialog();

            if (dialog.ShowDialog() == true) {
                sprite.ReplaceImages(dialog.FileNames);
            }
        }

        private void MenuSave_OnClick(object sender, RoutedEventArgs e) {
            Save();
        }

        private void MenuNewProject_OnClick(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Bunbundle File (*.bubu)|*bubu";
            dialog.Title = "Save new project file";
            dialog.DefaultExt = ".bubu";
            dialog.FileName = "sprites";

            if (dialog.ShowDialog() == true) {
                try {
                    Workspace.CreateNew(dialog.FileName);
                    LoadFile(dialog.FileName);
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
            }
        }


        private void MenuOpenFolder_OnClick(object sender, RoutedEventArgs e) {

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            string fileType = "Bunbundle File (*.bubu)|*bubu";
            dialog.Filter = fileType;
            dialog.DefaultExt = fileType;


            if (dialog.ShowDialog() == true) {
                LoadFile(dialog.FileName);
            }
        }

        private void MenuProjectSettings_OnClick(object sender, RoutedEventArgs e) {
            ProjectSettingsWindow window = new ProjectSettingsWindow();
            window.Show();
        }

        private void RecentProject_OnClick(object sender, RoutedEventArgs e) {
            string? project = (sender as MenuItem)?.Header as string;
            if (project == null) {
                return;
            }

            if (!File.Exists(project)) {
                Settings.Default.RecentFiles.Remove(project);
                Settings.Default.Save();
                MessageBox.Show("Project was not found. Removing from recent.");
                return;
            }

            LoadFile(project);
        }

        private void MenuToggleDarkMode_OnClick(object sender, RoutedEventArgs e) {
            IsDarkMode = !IsDarkMode;
            ModifyTheme();
            Settings.Default.IsDark = isDarkMode;
            Settings.Default.Save();
        }

        private void MenuOpenInExplorer_OnClick(object sender, RoutedEventArgs e) {
            TreeItemViewModel? model = ((((sender as MenuItem)?.Parent as ContextMenu)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem)?.Header as TreeItemViewModel;

            if (model == null) {
                return;
            }
            OpenFolder(model.Source.Storage.Path);
        }

        private void MenuDelete_OnClick(object sender, RoutedEventArgs e) {
            TreeItemViewModel? model = ((((sender as MenuItem)?.Parent as ContextMenu)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem)?.Header as TreeItemViewModel;

            model?.Delete();
        }


        private void OpenFolder(string folderPath) {
            if (Directory.Exists(folderPath)) {
                ProcessStartInfo startInfo = new ProcessStartInfo {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            else {
                MessageBox.Show($"{folderPath} Directory does not exist!");
            }
        }

        #region Drag and drop for treeview

        private Point tvLastMouseDown;
        private TreeItemViewModel? tvDraggedItem;
        private TreeItemViewModel? tvTargetItem;

        private TreeItemViewModel? GetNearestContainer(UIElement? element) {

            // Walk up the element tree to the nearest tree view item.
            TreeViewItem? container = element as TreeViewItem;
            while ((container == null) && (element != null)) {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }

            return container?.Header as TreeItemViewModel;
            
        }

        private bool CheckDropTarget(TreeItemViewModel? source, TreeItemViewModel? target) {
            FolderViewModel targetFolder;
            switch (target) {
                case FolderViewModel tFolder:
                    targetFolder = tFolder;
                    break;
                case SpriteViewModel targetSprite:
                    targetFolder = targetSprite.Parent;
                    break;
                default:
                    return false;
            }


            // Make sure my new folder isn't the same as my current one
            if (source?.Parent == targetFolder) {
                return false;
            }

            // If i'm a folder, make sure I'm not dragging to a child of me
            if (source is FolderViewModel sourceFolder) {
                FolderViewModel? parent = targetFolder;
                while (parent != null) {
                    if (parent == sourceFolder) {
                        return false;
                    }
                    parent = parent.Parent;
                }
            }

            return true;
        }

        private void MoveFile(TreeItemViewModel source, TreeItemViewModel target) {
            if (CheckDropTarget(source, target) == false) {
                return;
            }

            FolderViewModel? targetFolder;
            switch (target) {
                case FolderViewModel tFolder:
                    targetFolder = tFolder;
                    break;
                case SpriteViewModel targetSprite:
                    targetFolder = targetSprite.Parent;
                    break;
                default:
                    return;
            }

            if (targetFolder != null) {
                source.MoveTo(targetFolder);
            }
        }

        private void treeView_DragOver(object sender, DragEventArgs e) {

            Point currentPosition = e.GetPosition(folderView);

            if ((Math.Abs(currentPosition.X - tvLastMouseDown.X) > 10.0) ||
                (Math.Abs(currentPosition.Y - tvLastMouseDown.Y) > 10.0)) {
                // Verify that this is a valid drop and then store the drop target
                TreeItemViewModel? item = GetNearestContainer(e.OriginalSource as UIElement);
                if (CheckDropTarget(tvDraggedItem, item)) {
                    e.Effects = DragDropEffects.Move;
                } else {
                    e.Effects = DragDropEffects.None;
                }
            }
            e.Handled = true;
            
        }

        private void treeView_Drop(object sender, DragEventArgs e) {
            
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            // Verify that this is a valid drop and then store the drop target
            TreeItemViewModel? TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
            if (TargetItem != null && tvDraggedItem != null) {
                tvTargetItem = TargetItem;
                e.Effects = DragDropEffects.Move;
            }
        
        }

        private void treeView_MouseMove(object sender, MouseEventArgs e) {
        
            if (e.LeftButton == MouseButtonState.Pressed) {
                Point currentPosition = e.GetPosition(folderView);

                if ((Math.Abs(currentPosition.X - tvLastMouseDown.X) > 10.0) ||
                    (Math.Abs(currentPosition.Y - tvLastMouseDown.Y) > 10.0)) {
                    tvDraggedItem = folderView.SelectedItem as TreeItemViewModel;
                    if (tvDraggedItem != null) {
                        DragDropEffects finalDropEffect =
                            DragDrop.DoDragDrop(folderView,
                                folderView.SelectedValue,
                                DragDropEffects.Move);
                        //Checking target is not null and item is
                        //dragging(moving)
                        if ((finalDropEffect == DragDropEffects.Move) &&
                            (tvTargetItem != null)) {
                            // A Move drop was accepted
                            MoveFile(tvDraggedItem, tvTargetItem);
                            tvTargetItem = null;
                            tvDraggedItem = null;
                        }
                    }
                }
            }
        
        }

        private void treeView_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                tvLastMouseDown = e.GetPosition(folderView);
            }
        }

        #endregion

    }
}
