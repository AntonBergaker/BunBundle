using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    
    public record Settings(
        string TargetSpriteDirectory,
        string TargetFileDirectory,
        string GenerationClassName,
        string GenerationNamespace
    );
}
