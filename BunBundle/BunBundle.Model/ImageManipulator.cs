using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BunBundle.Model {
    static class ImageManipulator {

        public static void StripImage(string file, int count, IEnumerable<string> destinationNames) {
            Image image = Image.FromFile(file);

            int width = image.Width / count;
            int height = image.Height;

            using var enumerator = destinationNames.GetEnumerator();

            Bitmap map = new Bitmap(width, height);

            for (int i = 0; i < count; i++) {
                var graphics = Graphics.FromImage(map);
                graphics.Clear(Color.Transparent);
                graphics.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(width*i, 0, width, height), GraphicsUnit.Pixel);
                if (enumerator.MoveNext() == false) {
                    throw new ArgumentException("Not enough destinationNames", nameof(destinationNames));
                }
                graphics.Flush();
                graphics.Dispose();
                map.Save(enumerator.Current);
            }

            
        }

    }
}
