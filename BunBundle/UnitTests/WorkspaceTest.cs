using System.IO.Abstractions.TestingHelpers;
using BunBundle.Model;
using NUnit.Framework;

namespace UnitTests {
    public class WorkspaceTest {

        [Test]
        public void TestWorkspaceCreate() {

            MockFileSystem fileSystem = new MockFileSystem();

            Workspace.CreateNew("C:/sprites.bubu", fileSystem.File);

            Assert.IsTrue( fileSystem.File.Exists("C:/sprites.bubu") );

        }
    }
}