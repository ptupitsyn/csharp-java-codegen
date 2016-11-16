using System;
using System.IO;
using System.Linq;

namespace GridGain.CodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
            var cls =
                ClassParser.Parse(
                    @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\cache\CacheMetrics.java");

            //CsharpGenerator.WriteClass(cls, @"C:\W\CacheMetrics.cs", true);
            //File.WriteAllText(@"C:\W\CacheMetrics.cs", CsharpGenerator.GenerateInterfaceProperties(cls.Properties));
            File.WriteAllText(@"C:\W\CacheMetrics.cs", JavaGenerator.GenerateWriter(cls, "metrics"));


            /**
            var eventClasses = EventClassFinder.GetEventClasses().Select(ClassParser.Parse)
                .Where(x => x.Name.StartsWith("Auth")).ToList();

            foreach (var cls in eventClasses)
            {
                Console.WriteLine(cls.Name);

                //CsharpGenerator.WriteClass(cls);
            }

            //var csharpClsIds = CsharpGenerator.GenerateClsIdMap(eventClasses);

            JavaGenerator.UpdateEventWriter(eventClasses);*/
        }
    }
}
