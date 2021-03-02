using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BunBundle.Model;
using NUnit.Framework;

namespace UnitTests.Saving {
    class SaveActionRenameTest {

        [Test]
        public void SaveActionNewFileRenameTest() {

            MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> {
                { "C:/Sprites", new MockDirectoryData()},
                { "C:/source.png", "im an image allegedly"}
            });
            
            string path = "C:/Sprites/sprites.bubu";
            
            Workspace.CreateNew(path, fileSystem.File);

            Workspace workspace = new Workspace(path, fileSystem);
            
            workspace.ImportSprites(new []{ "C:/source.png" }, workspace.RootFolder);

            Sprite sprite = workspace.RootFolder.files[0];
            sprite.Name = "source2";
            
            workspace.Save();
            
            // Check that the new name got updated
            Assert.IsTrue( fileSystem.File.Exists( "C:/Sprites/source2/source2.spr" ) );
            Assert.IsTrue(fileSystem.File.Exists("C:/Sprites/source2/img/source2.png"));
        }
    }
}
