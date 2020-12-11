using System;
using System.Collections.Generic;
using System.Text;
using MonogameTexturePacker;

namespace MonogameTexturePacker {
    public class UpdatedSpritesData {
        public readonly Sprite Sprite;
        public readonly string OldPath;
        public readonly string NewPath;

        public UpdatedSpritesData(Sprite sprite, string newPath) {
            Sprite = sprite;
            OldPath = sprite.Path;
            NewPath = newPath;
        }
    }
}
