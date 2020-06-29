using System;
using System.Collections.Generic;
using System.Text;
using MonogameTexturePacker;

namespace MonogameTexturePacker {
    public class UpdatedSpritesData {
        public readonly Sprite Sprite;
        public readonly string OldPath;
        public readonly string NewPath;
        public readonly bool CheckSprites;

        public UpdatedSpritesData(Sprite sprite, string newPath, bool checkSprites) {
            Sprite = sprite;
            OldPath = sprite.Path;
            NewPath = newPath;
            CheckSprites = checkSprites;
        }
    }
}
