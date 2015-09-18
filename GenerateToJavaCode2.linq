<Query Kind="Statements">
  <Namespace>System.Globalization</Namespace>
</Query>

Func<string, string> camelToUnderscore = x => Regex.Replace(x.Trim(), "([A-Z])", "_$1").ToUpper();
Func<string, string> replaceTypes = x => x.Replace("long", "long long");
Func<string, string> firstToUpper = x => char.ToUpper(x[0]) + x.Substring(1);
Func<string, string> firstToLower = x => char.ToLower(x[0]) + x.Substring(1);
Func<string, string> getJniSig = x => x.Replace("long", "J").Replace("bool", "Z").Replace("void", "V");

var prefix = "AtomicLong";
var javaNs = "datastructures";
var prefix2 = camelToUnderscore(prefix).Substring(1);

var moduleDefMaxId = Regex.Matches(File.ReadAllText(@"C:\W\incubator-ignite\modules\platform\src\main\cpp\common\project\vs\module.def"), "[0-9]+").OfType<Match>().Select(x=>int.Parse(x.Value)).Max();


var src = @"C:\W\incubator-ignite\modules\platform\src\main\dotnet\Apache.Ignite.Core\DataStructures\IAtomicLong.cs";

var methods = Regex.Matches(File.ReadAllText(src), @"([a-zA-Z]+) ([a-zA-Z]+)\((.*?)\);")
.OfType<Match>().Select(x => x.Groups.OfType<Group>().Select(g => g.Value).ToArray()).Select(x =>
	new { 
	Ret = x[1], 
	RetSig = getJniSig(x[1]), 
	RetCpp = replaceTypes(x[1]),
	Name = x[2], 
	Name2 = camelToUnderscore(x[2]), 
	Args = string.IsNullOrEmpty(x[3]) ? "" : ", " + x[3],
	ArgsCpp = string.IsNullOrEmpty(x[3]) ? "" : ", " + replaceTypes(x[3]),
	ArgNames = string.IsNullOrEmpty(x[3]) ? "" : ", " + string.Join(", ", x[3].Split(',').Select(s=>s.Split(' ').Last())),
	ArgSig = string.IsNullOrEmpty(x[3]) ? "" : string.Concat(x[3].Split(',').Select(s=>getJniSig(s.Split(' ').First())))
	}).ToArray();
//methods.Dump();


// UnmanagedUtils
methods.Select(x => $"private delegate {x.Ret} {prefix}{x.Name}Delegate(void* ctx, void* target{x.Args});").Dump();

methods.Select(x => $"private static readonly {prefix}{x.Name}Delegate {prefix2}{x.Name2};").Dump();

methods.Select(x => $"private const string Proc{prefix}{x.Name} = \"Ignite{prefix}{x.Name}\";").Dump();

methods.Select(x => $"{prefix2}{x.Name2} = CreateDelegate<{prefix}{x.Name}Delegate>(Proc{prefix}{x.Name});").Dump();

methods.Select(x => $"internal static {x.Ret} {prefix}{x.Name}(IUnmanagedTarget target{x.Args})\n {{ \nreturn {prefix2}{x.Name2}(target.Context, target.Target{x.ArgNames});\n }}").Dump();

// module.def
methods.Select((x, i) => $"Ignite{prefix}{x.Name} @{i+moduleDefMaxId+1}").Dump("module.def");

// exports.h
methods.Select(x => $"{x.RetCpp} IGNITE_CALL Ignite{prefix}{x.Name}(gcj::JniContext* ctx, void* obj{x.ArgsCpp});").Dump("exports.h");

// exports.cpp
methods.Select(x => $"{x.RetCpp} IGNITE_CALL Ignite{prefix}{x.Name}(gcj::JniContext* ctx, void* obj{x.ArgsCpp}) {{ \n return ctx->{prefix}{x.Name}(static_cast<jobject>(obj){x.ArgNames}); \n}}\n").Dump("exports.cpp");

// java.h
methods.Select(x => $"{x.RetCpp} {prefix}{x.Name}(jobject obj{x.ArgsCpp});").Dump("java.h");

new[] { $"jclass c_Platform{prefix};"}.Concat(methods.Select(x => $"jmethodID m_Platform{prefix}_{firstToLower(x.Name)};")).Dump();

// java.cpp
methods.Select(x => $"{x.RetCpp} JniContext::{prefix}{x.Name}(jobject obj{x.ArgsCpp})\n {{ \n JNIEnv* env = Attach(); \n\n {x.RetCpp} res = env->Call{firstToUpper(x.Ret)}Method(obj, jvm->GetMembers().m_PlatformAtomicLong_read); \n\n ExceptionCheck(env); \n\n return res; \n }} \n").Dump("java.cpp");

new[] { $"const char* C_PLATFORM_{prefix2} = \"org/apache/ignite/internal/processors/platform/{javaNs}/Platform{prefix}\";" }.Concat(
methods.Select(x => $"JniMethod M_PLATFORM_{prefix2}{x.Name2} = JniMethod(\"{firstToLower(x.Name)}\", \"({x.ArgSig}){x.RetSig}\", false);")
).Dump();

new[] { $"jclass c_Platform{prefix} = FindClass(env, C_PLATFORM_{prefix2});"}.Concat(
methods.Select(x => $"m_Platform{prefix}_{firstToLower(x.Name)} = FindMethod(env, c_Platform{prefix}, M_PLATFORM_{prefix2}{x.Name2});")).Dump();