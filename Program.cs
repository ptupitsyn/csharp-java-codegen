using System;
using System.Linq;

namespace GridGain.CodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var cls =
                ClassParser.Parse(
                    @"C:\W\ggprivate\modules\core\src\main\java\org\gridgain\grid\dr\DrSenderInMetrics.java");

            CsharpGenerator.WriteClass(cls, @"C:\W\ggprivate\modules\clients\dotnet\gridgain\gridgain\impl\datacenterreplication\DrSenderInMetrics.cs", true);
             * */

            var eventClasses = EventClassFinder.GetEventClasses().Select(ClassParser.Parse)
                .Where(x => x.Name.StartsWith("Auth")).ToList();

            foreach (var cls in eventClasses)
            {
                Console.WriteLine(cls.Name);

                //CsharpGenerator.WriteClass(cls);
            }

            //var csharpClsIds = CsharpGenerator.GenerateClsIdMap(eventClasses);

            JavaGenerator.UpdateEventWriter(eventClasses);
        }
    }
}
