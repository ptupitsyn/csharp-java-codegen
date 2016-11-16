using System;
using System.IO;
using System.Reflection;

namespace GridGain.CodeGen
{
    public static class CodePath
    {
        static CodePath()
        {
            GgPrivatePath = @"c:\w\ggprivate";

            IncubatorIgnitePath = Path.GetFullPath(GgPrivatePath + @"\..\incubator-ignite");

            // ReSharper disable once AssignNullToNotNullAttribute
            CodegenProjectPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\"));

            TargetPath = @"c:\w";

            JavaEventWriterClassPath = GgPrivatePath + @"\modules\core\src\main\java\org\gridgain\grid\internal\interop\InteropContext.java";
        }

        public static string GgPrivatePath { get; private set; }

        public static string IncubatorIgnitePath { get; private set; }
        
        public static string CodegenProjectPath { get; private set; }
        
        public static string TargetPath { get; private set; }
        
        public static string JavaEventWriterClassPath { get; private set; }
    }
}