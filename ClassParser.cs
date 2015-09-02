using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GridGain.CodeGen
{
    public static class ClassParser
    {
        private const string ClassSigRegex = @"/\*\*\s+\*([^\*]+).*(class|interface) ([a-zA-Z]+)(<.*?>)?[^{]*{";

        private static int _clsId;

        public static Class Parse(string file)
        {
            var text = File.ReadAllText(file);
                
            //.Where(x => !x.Trim().StartsWith("@") && !x.Contains("public")).Aggregate((x, y) => x + y);

            var match = Regex.Match(text, ClassSigRegex, RegexOptions.Singleline);

            var code = text.Substring(match.Index + match.Length);

            var excludedPropNames = new[] {"ScanQueryFilter", "ContinuousQueryFilter", "Arguments"};

            return new Class
            {
                Id = _clsId++,
                Name = match.Groups[3].Value,
                Desc = match.Groups[1].Value,
                GenericArgs = match.Groups[4].Value,
                Properties = JavaConverter.GetMethods(code).Where(x => x.Annotation.Trim() != "@Override"
                                                       && !excludedPropNames.Contains(x.Name,
                                                           StringComparer.OrdinalIgnoreCase)).ToArray()
            };
        }
    }
}