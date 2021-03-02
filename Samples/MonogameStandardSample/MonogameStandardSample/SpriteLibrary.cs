using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
namespace MonogameStandardSample {
	public static partial class Sprites {
		public static _GroupFolderExample FolderExample {private set; get;}
		public class _GroupFolderExample {
			public Sprite ScarePardner {get;}
			
			internal _GroupFolderExample(ContentManager content) {
				ScarePardner = Sprites.MakeSprite(content, 
					new Vector2(265, 271),
					"FolderExample\\ScarePardner0"
				);
				
			}
		}
		public static Sprite ScareEyes {private set; get;}
		
		internal static void ImportSprites(ContentManager content) {
			FolderExample = new _GroupFolderExample(content);
			
			ScareEyes = Sprites.MakeSprite(content, 
				new Vector2(211, 296),
				"ScareEyes0"
			);
			
		}
	}
}
