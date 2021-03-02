using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;

namespace MonogameStandardSample {
    public partial class Sprites {


        internal static Sprite MakeSprite(ContentManager content, Vector2 origin, params string[] assetNames) {
            Texture2D[] textures = new Texture2D[assetNames.Length];
            for (int i = 0; i < assetNames.Length; i++) {
                string assetName = assetNames[i];

                textures[i] = content.Load<Texture2D>(Path.Join("Sprites", "mip0", assetName.Replace('\\', Path.DirectorySeparatorChar)));
                
            }

            return new Sprite(textures, origin);
        }

    }
}
