using System;
using System.IO;
using BunBundle.Model;

namespace MonogameTexturePackerConsole {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Usage: MonogameTexturePackerConsole sprmFile/folder");
                return;
            }

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

            workspace.Build();
        }
    }
}
