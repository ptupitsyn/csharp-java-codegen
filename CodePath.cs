using System;
using System.IO;
using System.Reflection;

namespace GridGain.CodeGen
{
    public static class CodePath
    {
        static CodePath()
        {
            var asm = Assembly.GetExecutingAssembly().Location;

            var modulesClientsDotnet = @"\modules\clients\dotnet";
            var idx = asm.IndexOf(modulesClientsDotnet, StringComparison.Ordinal);

            if (idx < 0)
                throw new InvalidOperationException("Can't find repo root.");

            GgPrivatePath = asm.Substring(0, idx);

            IncubatorIgnitePath = Path.GetFullPath(GgPrivatePath + @"\..\incubator-ignite");

            // ReSharper disable once AssignNullToNotNullAttribute
            CodegenProjectPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(asm), @"..\..\"));

            TargetPath = asm.Substring(0, idx + modulesClientsDotnet.Length) + @"\gridgain\gridgain\events";

            JavaEventWriterClassPath = GgPrivatePath + @"\modules\core\src\main\java\org\gridgain\grid\internal\interop\InteropContext.java";
        }

        public static string GgPrivatePath { get; private set; }

        public static string IncubatorIgnitePath { get; private set; }
        
        public static string CodegenProjectPath { get; private set; }
        
        public static string TargetPath { get; private set; }
        
        public static string JavaEventWriterClassPath { get; private set; }
    }
}