using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using BunBundle.Annotations;
using Microsoft.Win32;
using BunBundle.Model;

namespace BunBundle {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        private Workspace workspace;
        private Dictionary<IWorkspaceItem, ItemViewModel> itemToViewModels;

        private readonly PreviewImage previewImageHandler;
        
        private IWorkspaceItem selectedItem;

        public Visibility SpriteVisibility => selectedItem is Sprite ? Visibility.Visible : Visibility.Hidden;

        public Sprite SelectedSprite => selectedItem as Sprite;

        private bool preventRedraw;

        public IWorkspaceItem SelectedItem {
            get => selectedItem;
            set {
                selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SpriteVisibility));
                OnPropertyChanged(nameof(SelectedSprite));
            }
        }

        //private List<PictureBox> subImages = null;

        private FolderViewModel rootViewModel;

        public FolderViewModel RootViewModel {
            get => rootViewModel;
            set {
                rootViewModel = value;
                OnPropertyChanged();
            }
        }

        private float originY;
        public float OriginY {
            get => originY;
            set {
                originY = value;
                OnPropertyChanged();
            }
        }

        private float originX;
        public float OriginX {
            get => originX;
            set {
                originX = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow(string[] arguments) {
            InitializeComponent();

            workspace = new Workspace();
            workspace.OnImportSprite += WorkspaceOnOnImportSprite;
            workspace.OnAddFolder += WorkspaceOnOnAddFolder;
            workspace.OnUnsavedChanged += WorkspaceOnOnUnsavedChanged;

            previewImageHandler = new PreviewImage(imagePreview);

            previewImageHandler.OnOriginSet += OnOriginSet;

            //this.KeyPreview = true;
            this.KeyDown += OnKeyDown;

            itemToViewModels = new Dictionary<IWorkspaceItem, ItemViewModel>();
            //subImages = new List<PictureBox>();

            if (arguments.Length == 1) {
                string path = arguments[0];
                path = new FileInfo(path).FullName;
                LoadFolder(path);
            }

            preventRedraw = false;
        }

        private void WorkspaceOnOnUnsavedChanged(object sender, bool unsaved) {
            if (workspace.Unsaved) {
                this.Title = "BunBundle*";
            } else {
                this.Title = "BunBundle";
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)       // Ctrl-S Save
            {
                Save();
                e.Handled = true;  // Stops other controls on the form receiving event.
            }
        }

        private void WorkspaceOnOnImportSprite(object sender, (WorkspaceFolder parentFolder, Sprite sprite) e) {
            FolderViewModel parent = itemToViewModels[e.parentFolder] as FolderViewModel;
            SpriteViewModel newNode = new SpriteViewModel(e.sprite, parent);

            itemToViewModels.Add(e.sprite, newNode);

            parent?.AddItem(newNode);

            newNode.IsSelected = true;
            PopulateItemProperties();
        }


        private void WorkspaceOnOnAddFolder(object sender, (WorkspaceFolder parentFolder, WorkspaceFolder folder) e) {
            FolderViewModel parent = itemToViewModels[e.parentFolder] as FolderViewModel;

            FolderViewModel newNode = new FolderViewModel(e.folder, parent);

            itemToViewModels.Add(e.folder, newNode);

            parent?.AddItem(newNode);

            newNode.IsSelected = true;
            PopulateItemProperties();
        }

        private void Save() {
            workspace.Save();
        }

        private void LoadFolder(string path) {
            workspace.OpenFolder(path);
            PopulateTreeView();
        }

        private void PopulateTreeView() {
            rootViewModel = new FolderViewModel(workspace.RootFolder, null);
            folderView.ItemsSource = rootViewModel.Items;
            AddToDictionary(rootViewModel);
        }

        private void AddToDictionary(ItemViewModel viewModel) {
            itemToViewModels.Add(viewModel.Source, viewModel);
            if (viewModel is FolderViewModel folder) {
                foreach (ItemViewModel item in folder.Items) {
                    AddToDictionary(item);
                }
            }
        }

        private void PopulateItemProperties() {
            if (SelectedItem is Sprite sprite) {
                PopulateSpriteProperties(sprite);
            } else if (SelectedItem is WorkspaceFolder folder) {
                PopulateFolderProperties(folder);
            }
        }

        private void PopulateSpriteProperties(Sprite selectedSprite) {
            preventRedraw = true;

            OriginX = selectedSprite.OriginX;
            OriginY = selectedSprite.OriginY;
            UpdateOriginSelection(selectedSprite);

            labelTextureSize.Content = selectedSprite.Width + " x " + selectedSprite.Height;


            textBoxName.SetCurrentValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Sprite Name");
            textBoxName.Text = selectedSprite.Name;

            preventRedraw = false;

            if (SelectedItem is Sprite sprite) {
                previewImageHandler.SetImage(sprite, 0);
            }
        }

        private void PopulateFolderProperties(WorkspaceFolder folder) {
            textBoxName.SetCurrentValue(MaterialDesignThemes.Wpf.HintAssist.HintProperty, "Folder Name");
            textBoxName.Text = SelectedItem.Name;
        }

        private void UpdateOriginSelection(Sprite selectedSprite) {
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
            workspace.Build();
        }

        private void folderView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            SelectedItem = (e.NewValue as ItemViewModel)?.Source;

            PopulateItemProperties();
        }

        private void textBoxName_TextChanged(object sender, TextChangedEventArgs e) {
            if (SelectedItem == null) {
                return;
            }

            SelectedItem.Name = textBoxName.Text;
            if (itemToViewModels.TryGetValue(SelectedItem, out ItemViewModel vm)) {
                vm.Name = SelectedItem.Name;
            }
        }

        private void textBoxOriginX_TextChanged(object sender, TextChangedEventArgs e) {
            if (SelectedItem is Sprite sprite) {
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
            if (SelectedItem is Sprite sprite) {
                if (float.TryParse(textBoxOriginY.Text, out float result)) {
                    sprite.OriginY = result;
                }

                UpdateOriginSelection(sprite);
                if (preventRedraw == false) {
                    previewImageHandler.Redraw();
                }
            }
        }

        private void OnOriginSet(object sender, Point e) {
            if (SelectedItem is Sprite selectedSprite) {
                selectedSprite.OriginX = (float)e.X;
                selectedSprite.OriginY = (float)e.Y;

                OriginX = selectedSprite.OriginX;
                OriginY = selectedSprite.OriginY;
                UpdateOriginSelection(selectedSprite);

                UpdateOriginSelection(selectedSprite);

                previewImageHandler.Redraw();
            }
        }

        private void NumberOnlyPreview(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9\\-.]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void comboBoxOrigin_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Sprite selectedSprite = SelectedItem as Sprite;

            if (selectedSprite == null) {
                return;
            }

            int index = comboBoxOrigin.SelectedIndex - 1;
            if (index == -1) {
                return;
            }
            int wIndex = index % 3;
            int hIndex = index / 3;

            selectedSprite.OriginX = (selectedSprite.Width) * wIndex / 2;
            selectedSprite.OriginY = (selectedSprite.Height) * hIndex / 2;

            OriginX = selectedSprite.OriginX;
            OriginY = selectedSprite.OriginY;
        }

        private void buttonNewFolder_Click(object sender, RoutedEventArgs e) {
            if (SelectedItem is WorkspaceFolder folder) {
                workspace.CreateFolder(folder);
            } else if (SelectedItem is Sprite sprite) {
                workspace.CreateFolder(sprite.Parent);
            }
            else {
                workspace.CreateFolder(rootViewModel.Folder);
            }

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
                if (SelectedItem is WorkspaceFolder folder) {
                    workspace.ImportSprites(dialog.FileNames, folder);
                }
                else if (SelectedItem is Sprite sprite) {
                    workspace.ImportSprites(dialog.FileNames, sprite.Parent);
                }
            }
        }

        private void AddSpritesButton_Click(object sender, RoutedEventArgs e) {
            Sprite sprite = selectedItem as Sprite;

            if (sprite == null) {
                return;
            }

            OpenFileDialog dialog = MakeOpenSpritesDialog();

            if (dialog.ShowDialog() == true) {
                sprite.AddImages(dialog.FileNames);
            }
        }

        private void ReplaceSpritesButton_Click(object sender, RoutedEventArgs e) {
            Sprite sprite = selectedItem as Sprite;

            if (sprite == null) {
                return;
            }

            OpenFileDialog dialog = MakeOpenSpritesDialog();

            if (dialog.ShowDialog() == true) {
                sprite.ClearImages();
                sprite.AddImages(dialog.FileNames);
            }
        }
    }
}
