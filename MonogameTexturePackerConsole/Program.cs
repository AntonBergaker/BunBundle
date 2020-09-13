using System;
using System.IO;
using BunBundle.Model;

namespace MonogameTexturePackerConsole {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Usage: BunBundle sprmFile/folder");
                return;
            }

            string path = args[0];

            Workspace workspace = new Workspace(path);

            workspace.Build();
        }
    }
}
