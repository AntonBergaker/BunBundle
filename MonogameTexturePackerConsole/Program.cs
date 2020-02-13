using System;
using System.IO;
using MonogameTexturePacker;

namespace MonogameTexturePackerConsole {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Usage: MonogameTexturePackerConsole sprmFile/folder [path to mgcb]");
                return;
            }

            string mgcbPath = args.Length > 1 ? args[1] : "C:\\Program Files (x86)\\MSBuild\\MonoGame\\v3.0\\Tools\\MGCB.exe";

            Workspace workspace = new Workspace();

            string path = args[0];

            if (File.Exists(path)) {
                workspace.OpenFile(path);
            } else if (Directory.Exists(path)) {
                workspace.OpenFolder(path);
            } else {
                Console.WriteLine("Can not find the file/folder");
                return;
            }

            workspace.Build(mgcbPath);
        }
    }
}
