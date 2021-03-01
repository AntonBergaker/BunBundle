using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {

    public class Settings {
        public string TargetDirectory { get; set; }
        public string TargetSpriteDirectory { get; set; }
        public string TargetFilePath { get; set; }
        public string ClassName { get; set; }
        public string Namespace { get; set; }

        public Settings(string targetDirectory, string targetSpriteDirectory, string targetFilePath, string className, string @namespace) {
            this.TargetDirectory = targetDirectory;
            this.TargetSpriteDirectory = targetSpriteDirectory;
            this.TargetFilePath = targetFilePath;
            this.ClassName = className;
            this.Namespace = @namespace;
        }
    }
}
