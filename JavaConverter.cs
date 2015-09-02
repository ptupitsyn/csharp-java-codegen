using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GridGain.CodeGen
{
    public static class JavaConverter
    {
        public const string MethodSigRegex =
            @"/\*\*([^;]*?)(@return.*?)?\*/\s+(@[a-zA-Z\(\)""{}]+)?\s+public\s((<[^>]+>\s)?[a-zA-Z\[\]]+(<[^>]+>)?)\s([a-zA-Z]+)\(([^\)]*)\)";

        public static IEnumerable<Method> GetMethods(string code)
        {
            return from Match match in Regex.Matches(code, MethodSigRegex, RegexOptions.Singleline)
                   select match.Groups into g
                   select new Method
                   {
                       Name = g[7].Value,
                       Type = g[4].Value,
                       Desc = g[1].Value,
                       Annotation = g[3].Value,
                       Arguments = g[8].Value,
                       Returns = g[2].Value.Replace("@return", "")
                   };
        }

        public static string ToUpperCamel(this string s)
        {
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        public static string ToLowerCamel(this string s)
        {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        public static string CleanupComment(this string s)
        {
            // links
            s = Regex.Replace(s, "{@link (.*?)#(.*?)}",
                match =>
                {
                    var clsName = match.Groups[1].Value;
                    var memberName = match.Groups[2].Value.ToUpperCamel().Replace("()", "");
                    
                    if (!string.IsNullOrWhiteSpace(clsName))
                        memberName = clsName + "." + memberName;

                    return string.Format("<see cref=\"{0}\" />", memberName);
                });

            s = Regex.Replace(s, "{@code (.*?)}", "$1");
            s = Regex.Replace(s, "@{code (.*?)}", "$1");

            return s
                .Replace("*", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(@"<p>", "<para />")
                .Replace(@"<p/>", "<para />")
                .Replace(@"<p>", "")
                .Replace(@"<p/>", "")
                .Replace(@"<ul>", " ")
                .Replace(@"</ul>", " ")
                .Replace(@"<li>", " ")
                .Replace(@"</li>", " ")
                .Replace(@"<br>", " ")
                .Replace(@"<b>", " ")
                .Replace(@"</b>", " ")
                .Replace(@"<tt>", " ")
                .Replace(@"</tt>", " ")
                .Replace("      ", " ")
                .Replace("     ", " ")
                .Replace("    ", " ")
                .Replace("   ", " ")
                .Replace("  ", " ")
                .Trim(' ', '\n', '\r');
        }

        public static string ToCsharpType(this string s)
        {
            if (s.Contains("<"))
                return "object";

            return s
                .Replace("boolean", "bool")
                .Replace("UUID", "Guid")
                .Replace("IgniteUuid", "GridGuid")
                .Replace("ClusterNode", "IClusterNode")
                .Replace("Object", "object")
                .Replace("K", "object")
                .Replace("V", "object")
                .Replace("String", "string");
        }

        public static IEnumerable<string> WordWrap(this string s, int maxLegth)
        {
            var originalLines = s.Split(new[] { " " }, StringSplitOptions.None);

            var actualLine = new StringBuilder();

            foreach (var item in originalLines)
            {
                if (actualLine.Length + item.Length > maxLegth)
                {
                    yield return actualLine.ToString();
                    actualLine.Clear();
                }

                actualLine.Append(item + " ");
            }

            if (actualLine.Length > 0)
                yield return actualLine.ToString();
        }
    }
}