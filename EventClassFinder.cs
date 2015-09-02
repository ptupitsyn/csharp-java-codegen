using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GridGain.CodeGen
{
    public static class EventClassFinder
    {
        private static readonly string[] EventClasses =
        {
            @"C:\W\ggprivate\modules\core\src\main\java\org\gridgain\grid\events\AuthenticationEvent.java",
            @"C:\W\ggprivate\modules\core\src\main\java\org\gridgain\grid\events\AuthorizationEvent.java",
            @"C:\W\ggprivate\modules\core\src\main\java\org\gridgain\grid\events\LicenseEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\CacheEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\CacheQueryExecutedEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\CacheQueryReadEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\CacheRebalancingEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\CheckpointEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\DeploymentEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\DiscoveryEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\IgfsEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\JobEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\SwapSpaceEvent.java",
            @"C:\W\incubator-ignite\modules\core\src\main\java\org\apache\ignite\events\TaskEvent.java"
        };

        public static IEnumerable<string> GetEventClasses(bool rescan = false)
        {
            if (!rescan) 
                return EventClasses;

            var classes = FindEventClasses().ToArray();

            UpdateThisFile(classes);

            return classes;
        }

        private static void UpdateThisFile(IEnumerable<string> classes)
        {
            var classesCode = classes.Select(x => string.Format("@\"{0}\"", x))
                .Aggregate((x, y) => string.Format("{0},\n{1}", x, y));

            var classFinderFile = Path.Combine(CodePath.CodegenProjectPath, "EventClassFinder.cs");
            var classFinderCode = File.ReadAllText(classFinderFile);

            classFinderCode = Regex.Replace(classFinderCode, "string\\[] EventClasses =.*?};", "string[]" +
                                                                                               " EventClasses = { " +
                                                                                               classesCode + " };",
                RegexOptions.Singleline);

            File.WriteAllText(classFinderFile, classFinderCode);
        }

        private static IEnumerable<string> FindEventClasses()
        {
            return new[] {CodePath.GgPrivatePath, CodePath.IncubatorIgnitePath}
                .SelectMany(dir => Directory.EnumerateFiles(dir, "*.java", SearchOption.AllDirectories))
                .Where(IsEventClass);
        }

        private static bool IsEventClass(string file)
        {
            foreach (var line in File.ReadLines(file))
            {
                if (line.Contains("extends EventAdapter"))
                    return true;

                if (line.StartsWith("/*") || line.StartsWith(" * ") || line.StartsWith(" */"))
                    continue;

                if (line.Contains("{"))
                    return false;
            }

            return false;
        }
    }
}