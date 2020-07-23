using System;
using System.Collections.Generic;
using System.Text;

namespace BunBundle.Model {
    class CodeBuilder {
        private StringBuilder builder;
        private int indent;

        public CodeBuilder() {
            builder = new StringBuilder();
            indent = 0;
        }

        public void AddLine(string line) {
            builder.AppendLine(new string('\t', indent) + line);
        }

        public void AddLines(params string[] lines) {
            foreach (string line in lines) {
                builder.AppendLine(new string('\t', indent) + line);
            }
        }

        public void Indent() {
            indent++;
        }

        public void Unindent() {
            indent--;
        }

        public override string ToString() {
            return builder.ToString();
        }
    }
}
