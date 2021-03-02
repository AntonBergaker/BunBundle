using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameStandardSample {
    public class Sprite {

        public Vector2 Origin { get; }

        public Texture2D[] Textures { get; }

        public Sprite(Texture2D[] textures, Vector2 origin) {
            Textures = textures;
            Origin = origin;
        }
    }
}
