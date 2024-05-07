using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CosmeticsParser
{
    public static class Utils
    {
        public static string RefactorName(string text)
        {
            text = text.Replace(@"""", @"\""");
            text = @"""" + text + @"""";
            return text;
        }

        public static string RefactorCollectionName(string text)
        {
            text = text.Replace(" ", " "); //nbsp
            return text;
        }

        public static string RefactorText(string text)
        {
            text = text.Replace(@"""", @"\""");
            text = @"""" + text + @"""";
            text = Regex.Replace(text, @"<span class=\\""FlavorText\\"">(“|(\\""))?(.+?)(”|\\"")?( [—-](.*))?<\/span>", @""" .. quote(""$3"", ""$6"") .. """);
            text = text
                .Replace("<br>", @""" .. br .. """)
                .Replace("\n", @""" .. br .. """);
            Regex quotes = new Regex(@"\.\. """"");
            text = quotes.Replace(text, string.Empty);
            text = text
                .Replace("  ", string.Empty)
                .Replace("br..", "br ..");

            //text = text.Replace(@"""""")

            return text;
        }
    }
}
