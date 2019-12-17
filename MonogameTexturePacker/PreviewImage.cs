using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonogameTexturePacker {
    class PreviewImage : PictureBox {

        public Sprite selectedSprite;
        private string imagePath;
        private Timer fuckYouImageBox;

        protected override void OnBackgroundImageChanged(EventArgs e) {
            base.OnBackgroundImageChanged(e);
            Console.WriteLine("HI");
        }

        public void UpdateImage(string path) {
            if (path == this.imagePath) {
                return;
            }

            imagePath = path;
            ImageLocation = path;
            this.Invalidate();
            fuckYouImageBox = new Timer();
            fuckYouImageBox.Start();
            fuckYouImageBox.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e) {
            Invalidate();
            fuckYouImageBox.Stop();
        }

        public Point GetImagePoint() {
            Point p = this.PointToClient(Cursor.Position);
            Point unscaled_p = new Point();

            // image and container dimensions
            int w_i = this.Image.Width;
            int h_i = this.Image.Height;
            int w_c = this.Width;
            int h_c = this.Height;

            float imageRatio = w_i / (float)h_i; // image W:H ratio
            float containerRatio = w_c / (float)h_c; // container W:H ratio

            if (imageRatio >= containerRatio) {
                // horizontal image
                float scaleFactor = w_c / (float)w_i;
                float scaledHeight = h_i * scaleFactor;
                // calculate gap between top of container and top of image
                float filler = Math.Abs(h_c - scaledHeight) / 2;
                unscaled_p.X = (int)(p.X / scaleFactor);
                unscaled_p.Y = (int)((p.Y - filler) / scaleFactor);
            } else {
                // vertical image
                float scaleFactor = h_c / (float)h_i;
                float scaledWidth = w_i * scaleFactor;
                float filler = Math.Abs(w_c - scaledWidth) / 2;
                unscaled_p.X = (int)((p.X - filler) / scaleFactor);
                unscaled_p.Y = (int)(p.Y / scaleFactor);
            }

            return unscaled_p;
        }

        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);
            
            if (selectedSprite == null) {
                return;
            }

            if (imagePath == null) {
                return;
            }

            Bitmap image = new Bitmap(imagePath);

            Rectangle vR = new Rectangle((int)selectedSprite.OriginX, (int)selectedSprite.OriginY - 25, 1, 50);
            Rectangle hR = new Rectangle((int)selectedSprite.OriginX - 25, (int)selectedSprite.OriginY, 50, 1);
            Graphics gr = Graphics.FromImage(Image);
            gr.Clear(Color.White);

            using (Brush brush = new SolidBrush(Color.Gray)) {
                for (int x = 0; x < Image.Width; x += 32) {
                    bool shouldDraw = x % 64 == 0;
                    for (int y = 0; y < Image.Height; y += 32) {
                        if (shouldDraw = !shouldDraw) {
                            gr.FillRectangle(brush, new Rectangle(x, y, 32, 32));
                        }
                    }
                }
            }

            gr.DrawImage(image, Point.Empty);
            using (Pen pen = new Pen(Color.DeepPink, 1)) {
                gr.DrawRectangle(pen, vR);
                gr.DrawRectangle(pen, hR);
            }

            image.Dispose();
        }
    }
}
