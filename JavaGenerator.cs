using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GridGain.CodeGen
{
    public static class JavaGenerator
    {
        public static void UpdateEventWriter(ICollection<Class> classes)
        {
            var file = CodePath.JavaEventWriterClassPath;

            var text = File.ReadAllText(file);

            // writers
            var idx = text.IndexOf("// Write specific fields", StringComparison.Ordinal) - 1;

            var endIdx = text.IndexOf("\n    }", idx, StringComparison.Ordinal);

            text = text.Substring(0, idx) + "// Write specific fields\n" + GenerateClassWriters(classes) + text.Substring(endIdx);

            // cls ids
            /*
            const string builder = "<Class, Integer>builder()";
            idx = text.IndexOf(builder, StringComparison.Ordinal) + builder.Length;
            var idx2 = text.IndexOf(".build();", StringComparison.Ordinal);

            text = text.Remove(idx, idx2 - idx).Insert(idx, GenerateClsIds(classes));*/

            File.WriteAllText(file, text);
        }

        private static string GenerateClsIds(IEnumerable<Class> classes)
        {
            return classes.Select(x => string.Format("\n.put({0}.class, {1})", x.Name, x.Id))
                .Aggregate((x, y) => x + y);
        }

        private static string GenerateClassWriters(IEnumerable<Class> classes)
        {
            var sb = new StringBuilder();

            foreach (var cls in classes)
            {
                sb.AppendFormat("if (event instanceof {0}){{\n", cls.Name);
                sb.AppendFormat("{0} event0 = ({0})event;\n\n", cls.Name);

                // Write props
                foreach (var prop in cls.Properties)
                {
                    sb.AppendFormat("writer.write{0}(event0.{1}());\n", GetPropType(prop.Type), prop.Name);
                }

                sb.AppendLine("\nreturn;\n}");
            }

            return sb.ToString();
        }

        private static string GetPropType(string propType)
        {
            return propType.ToUpperCamel().Replace("UUID", "Uuid");
        }
    }
}