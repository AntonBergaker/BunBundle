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


        public static string PascalToSnakecase(string pascal) {
            StringBuilder sb = new StringBuilder(pascal.Length + pascal.Length / 4 + 1);
            bool first = true;
            foreach (char c in pascal) {
                if (char.IsUpper(c)) {
                    if (first == false) {
                        sb.Append("_");
                    }

                    sb.Append(char.ToLower(c));
                } else {
                    sb.Append(c);
                }

                first = false;
            }

            return sb.ToString();
        }

        public static string SnakecaseToPascal(string snake) {
            string[] split = snake.Split('_');
            return string.Concat(split.Select(x => FirstLetterToUpper(x)));
        }
    }
}
