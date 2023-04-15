using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NebulaUtils
{
    public class Url
    {
        public static string Combine(string string1, string string2)
        {
            string1 = string1.Replace('\\', '/').TrimEnd('/');
            string2 = string2.Replace('\\', '/').TrimStart('/');

            string[] strs = { string1, string2 };
            string buildString = "";
            for (int i = 0; i < 2; ++i)
                if (!string.IsNullOrEmpty(strs[i]))
                    buildString += strs[i] + "/";
            return buildString.TrimEnd('/');
        }

        public static string Combine(string string1, string string2, string string3)
        {
            string1 = string1.Replace('\\', '/').TrimEnd('/');
            string2 = string2.Replace('\\', '/').TrimEnd('/').TrimStart('/');
            string3 = string3.Replace('\\', '/').TrimStart('/');

            string[] strs = { string1, string2, string3 };
            string buildString = "";
            for (int i = 0; i < 3; ++i)
                if (!string.IsNullOrEmpty(strs[i]))
                    buildString += strs[i] + "/";
            return buildString.TrimEnd('/');
        }

        public static string Combine(string string1, string string2, string string3, string string4)
        {
            string1 = string1.Replace('\\', '/').TrimEnd('/');
            string2 = string2.Replace('\\', '/').TrimEnd('/').TrimStart('/');
            string3 = string3.Replace('\\', '/').TrimEnd('/').TrimStart('/');
            string4 = string4.Replace('\\', '/').TrimStart('/');

            string[] strs = { string1, string2, string3, string4 };
            string buildString = "";
            for (int i = 0; i < 4; ++i)
                if (!string.IsNullOrEmpty(strs[i]))
                    buildString += strs[i] + "/";
            return buildString.TrimEnd('/');
        }
    }
}
