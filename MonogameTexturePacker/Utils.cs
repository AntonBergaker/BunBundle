using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonogameTexturePacker {
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
    }
}
