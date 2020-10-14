using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BunBundle.Model {
    static class Utils {
        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action) {
            int i = 0;
            foreach (var e in ie) action(e, i++);
        }

        public static void Each<T>(this IEnumerable<T> ie, Action<T> action) {
            foreach (var e in ie) action(e);
        }

        public static string FirstLetterToUpper(string str) {
            char[] chars = str.ToCharArray();
            chars[0]  = char.ToUpper(chars[0]);
            return new string(chars);
        }

        public static string GeneratePath(IWorkspaceItem item) {
            StringBuilder sb = new StringBuilder();

            bool first = true;
            while (item.Parent != null) {
                sb.Insert(0, item.Name + (first ? "" : Path.DirectorySeparatorChar.ToString()));
                first = false;
                item = item.Parent;
            }

            // Item should now be root
            return Path.Combine(item.Storage.Path, sb.ToString());
        }
    }
}
