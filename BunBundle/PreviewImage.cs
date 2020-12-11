using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BunBundle.Model;
using MaterialDesignThemes.Wpf;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace BunBundle {
    class PreviewImage {
        private readonly Image image;

        private Sprite sprite;
        private BitmapImage? source;
        private int index;

        private Sprite oldImportSprite;
        private string oldImportPath = "";

        private double oldRedrawOriginX;
        private double oldRedrawOriginY;
        private Sprite oldRedrawSprite;
        private string oldRedrawPath = "";

        private Task? imageImportImageTask;
        private CancellationTokenSource imageImportCancelToken;

        private Task redrawTask;
        private CancellationTokenSource redrawCancelToken;


        public PreviewImage(Image image) {
            this.image = image;
            image.MouseMove += ImageOnMouseMove;
            image.MouseDown += ImageOnMouseDown;
        }

        private void ImageOnMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed) {
                return;
            }

            Point p = GetImagePoint(e);
            OnOriginSet?.Invoke(this, p);
        }

        private void ImageOnMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed) {
                return;
            }

            Point p = GetImagePoint(e);
            OnOriginSet?.Invoke(this, p);
        }

        public EventHandler<Point> OnOriginSet;


        private Point GetImagePoint(MouseEventArgs e) {
            Point p = e.GetPosition(image);
            Point unscaled_p = new Point();

            // image and container dimensions
            double w_i = this.sprite.Width;
            double h_i = this.sprite.Height; 
            double w_c = image.ActualWidth;
            double h_c = image.ActualHeight;

            double imageRatio = w_i / h_i; // image W:H ratio
            double containerRatio = w_c / h_c; // container W:H ratio

            if (imageRatio >= containerRatio) {
                // horizontal image
                double scaleFactor = w_c / (float)w_i;
                double scaledHeight = h_i * scaleFactor;
                // calculate gap between top of container and top of image
                double filler = Math.Abs(h_c - scaledHeight) / 2;
                unscaled_p.X = (int)(p.X / scaleFactor);
                unscaled_p.Y = (int)((p.Y - filler) / scaleFactor);
            } else {
                // vertical image
                double scaleFactor = h_c / h_i;
                double scaledWidth = w_i * scaleFactor;
                double filler = Math.Abs(w_c - scaledWidth) / 2;
                unscaled_p.X = (int)((p.X - filler) / scaleFactor);
                unscaled_p.Y = (int)(p.Y / scaleFactor);
            }

            return unscaled_p;
        }

        public void Redraw() {
            if (oldRedrawOriginX == sprite.OriginX && oldRedrawOriginY == sprite.OriginY
                                                   && oldRedrawSprite == sprite &&
                                                   oldRedrawPath == sprite.ImageAbsolutePaths[index]) {
                return;
            }

            oldRedrawOriginX = sprite.OriginX;
            oldRedrawOriginY = sprite.OriginY;
            oldRedrawSprite = sprite;
            oldRedrawPath = sprite.ImageAbsolutePaths[index];

            if (redrawTask != null && redrawTask.IsCompleted == false) {
                redrawCancelToken.Cancel();
                redrawTask = null;
            }

            redrawCancelToken = new CancellationTokenSource();

            redrawTask = new Task(() => { RedrawImage(redrawCancelToken); }, redrawCancelToken.Token);


            redrawTask.Start();
            
        }

        private void RedrawImage(CancellationTokenSource cancelToken) {
            if (sprite == null) {
                return;
            }


            Rect limits;
            if (source == null) { 
                limits = new Rect(0, 0, 256, 256);
            }
            else {
                limits = new Rect(0, 0, source.PixelWidth, source.PixelHeight);
            }

            DrawingGroup imageDrawings = new DrawingGroup();
            imageDrawings.ClipGeometry = new RectangleGeometry(limits);


            imageDrawings.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(255, 255, 255)), null, new RectangleGeometry(limits)));
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(128, 128, 128));

            for (int x = 0; x < limits.Width; x += 32) {
                bool shouldDraw = x % 64 == 0;
                for (int y = 0; y < limits.Height; y += 32) {
                    shouldDraw = !shouldDraw;
                    if (shouldDraw) {
                        GeometryDrawing square = new GeometryDrawing(brush, null, new RectangleGeometry(new Rect(x, y, 32, 32)));
                        imageDrawings.Children.Add(square);
                    }
                }

                if (cancelToken != null && cancelToken.Token.IsCancellationRequested) {
                    return;
                }
            }

            if ( source != null) {
                imageDrawings.Children.Add(new ImageDrawing(source, limits));
            }
            else {
                brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00));
                Pen pen = new Pen(brush, 5);
                int s = 40;
                int o = (int)limits.Width - s;
                imageDrawings.Children.Add(new GeometryDrawing(brush, pen, new LineGeometry(new Point(s, s), new Point(o, o))));
                imageDrawings.Children.Add(new GeometryDrawing(brush, pen, new LineGeometry(new Point(o, s), new Point(s, o))));
            }


            float size = Math.Max(sprite.Width, sprite.Height);
            int crossSize = Math.Max(1, (int)(size / 10f));

            int oX = (int)sprite.OriginX;
            int oY = (int)sprite.OriginY;
            int thickness = Math.Max(1, crossSize / 10);

            Rect hR = new Rect(oX - crossSize, oY, 2 * crossSize, thickness);

            Rect vR = new Rect(oX, oY - crossSize, thickness, 2 * crossSize);

            brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x14, 0x93));

            imageDrawings.Children.Add(new GeometryDrawing(brush, null, new RectangleGeometry(vR)));
            imageDrawings.Children.Add(new GeometryDrawing(brush, null, new RectangleGeometry(hR)));

            imageDrawings.Freeze();

            Application.Current.Dispatcher.Invoke(() => {
                image.Visibility = Visibility.Visible;
                return image.Source = new DrawingImage(imageDrawings);
            });
        }


        public void SetImage(Sprite sprite, int index) {
            this.sprite = sprite;
            this.index = index;

            string path = sprite.ImageAbsolutePaths[index];


            if (path != oldImportPath || sprite != oldImportSprite) {
                if (File.Exists(path) == false) {
                    source = null;
                }
                else {
                    oldImportSprite = sprite;
                    oldImportPath = path;

                    image.Visibility = Visibility.Hidden;

                    if (imageImportImageTask != null && imageImportImageTask.IsCompleted == false) {
                        this.imageImportCancelToken.Cancel();
                        imageImportImageTask = null;
                    }

                    CancellationTokenSource imageImportCancelToken = new CancellationTokenSource();
                    this.imageImportCancelToken = imageImportCancelToken;

                    imageImportImageTask = new Task(() => {
                        source = new BitmapImage();
                        source.BeginInit();
                        source.UriSource = new Uri(path);
                        source.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.EndInit();
                        source.Freeze();

                        if (imageImportCancelToken.IsCancellationRequested) {
                            return;
                        }

                        Redraw();
                        imageImportImageTask = null;
                    }, imageImportCancelToken.Token);


                    imageImportImageTask.Start();
                }

            }
            else {
                if (imageImportImageTask == null) {
                    Redraw();
                }
            }
        }
    }
}
