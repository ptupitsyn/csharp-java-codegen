using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GridGain.CodeGen
{
    public static class CsharpGenerator
    {
        private static readonly string Template = File.ReadAllText(CodePath.CodegenProjectPath + "\\TemplateClass.txt");

        public static void WriteClass(Class cls, string targetPath = null, bool inheritDoc = false)
        {
            targetPath = targetPath ?? string.Format("{0}\\{1}.cs", CodePath.TargetPath, cls.Name);

            File.WriteAllText(targetPath, GenerateDtoClass(cls, inheritDoc));
        }

        public static string GenerateClsIdMap(IEnumerable<Class> classes)
        {
            return classes.Select(x => string.Format("{{ {0}, typeof({1}) }},\n", x.Id, x.Name.ToUpperCamel()))
                .Aggregate((x, y) => x + y);
        }

        public static string GenerateInterfaceMethods(IEnumerable<Method> methods)
        {
            var sb = new StringBuilder();

            foreach (var meth in methods)
            {
                var propName = meth.Name.ToUpperCamel();

                AppendComment(sb, meth);

                sb.AppendFormat("        public {0} {1}({2});\n", meth.Type.ToCsharpType(),
                    propName, meth.Arguments);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string GenerateInterfaceProperties(IEnumerable<Method> methods)
        {
            var sb = new StringBuilder();

            foreach (var meth in methods)
            {
                var propName = meth.Name.ToUpperCamel();

                if (propName.StartsWith("Get"))
                    propName = propName.Substring(3);

                AppendComment(sb, meth);

                sb.AppendFormat("        {0} {1} {{ get; }}\n", meth.Type.ToCsharpType(), propName);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static void AppendComment(StringBuilder sb, Method meth)
        {
            sb.AppendLine("        /// <summary>");
            foreach (var commentLine in meth.Desc.CleanupComment().WordWrap(108))
                sb.AppendLine("        /// " + commentLine);
            sb.AppendLine("        /// </summary>");

            // TODO: arguments

            if (!string.IsNullOrWhiteSpace(meth.Returns))
            {
                sb.AppendLine("        /// <returns>");
                foreach (var commentLine in meth.Returns.CleanupComment().WordWrap(108))
                    sb.AppendLine("        /// " + commentLine);
                sb.AppendLine("        /// </returns>");
            }
        }

        public static string GenerateDtoClass(Class cls, bool inheritDoc = false)
        {
            var props = new StringBuilder();
            var fields = new StringBuilder();
            var writePort = new StringBuilder();
            var readPort = new StringBuilder();

            foreach (var prop in cls.Properties)
            {
                var propName = prop.Name.ToUpperCamel();

                if (propName.StartsWith("Get"))
                    propName = propName.Substring(3);

                var fieldName = propName.ToLowerCamel().Replace("internal", "@internal");

                // Property
                if (inheritDoc)
                    props.AppendLine("        /** <inheritDoc /> */");
                else
                    AppendComment(props, prop);

                props.AppendFormat("        public {0} {1} {{ get {{ return {2}; }} }}\n", prop.Type.ToCsharpType(),
                    propName, fieldName);
                props.AppendLine();

                // Field
                fields.AppendLine(  "        /** */");
                fields.AppendFormat("        private readonly {0} {1};\n\n", prop.Type.ToCsharpType(), fieldName);

                // Portable read/write
                var propTypeRw = prop.Type.ToUpperCamel()
                    .Replace("UUID", "Guid").Replace("K", "Object").Replace("V", "Object");

                if (propTypeRw.Contains("<"))
                    propTypeRw = "Object";

                writePort.AppendFormat("            w.Write{0}({1});\n", propTypeRw, propName);
                readPort.AppendFormat("            {1} = r.Read{0}();\n",
                    propTypeRw.Replace("Object", "Object<object>"), fieldName);
            }

            return Template.Replace("CLS_NAME", cls.Name.ToUpperCamel())
                .Replace("CLS_DESC", cls.Desc.CleanupComment())
                .Replace("CLS_CODE", props.ToString().TrimEnd('\n', '\r'))
                .Replace("CLS_FIELDS", fields.ToString().TrimEnd('\n'))
                .Replace("WRITE_PORTABLE", writePort.ToString().TrimEnd('\n'))
                .Replace("READ_PORTABLE", readPort.ToString().TrimEnd('\n'))
                .Replace("CLS_TO_SHORT_STRING", GenerateToShortString(cls.Properties))
                .Replace("r.ReadClusterNode();", "ReadNode(r);")
                .Replace("r.ReadIgniteUuid();", "GridGuid.ReadPortable(r);")
                .Replace("r.ReadGuid();", "r.ReadGuid() ?? Guid.Empty;");
        }

        private static string GenerateToShortString(Method[] properties)
        {
            // "{0}: IsNear={1}, Key={2}, HasNewValue={3}, HasOldValue={4}, NodeId={5}", Name, isNear, key, HasNewValue, HasOldValue, Node.Id
            var fmt = properties.Select((x, i) => string.Format("{0}={{{1}}}", x.Name.ToUpperCamel(), i + 1))
                .Aggregate((x, y) => string.Format("{0}, {1}", x, y));

            var props = properties.Select(x => x.Name.ToUpperCamel())
                .Aggregate((x, y) => string.Format("{0}, {1}", x, y));

            return string.Format("\"{{0}}: {0}\", Name, {1}", fmt, props);
        }
    }
}