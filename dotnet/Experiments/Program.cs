using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DisposeResearch;
using DisposeResearch.DotNet;

Console.WriteLine("runtime=" + RuntimeInformation.FrameworkDescription + "; arch=" + RuntimeInformation.ProcessArchitecture);
Console.WriteLine($"execution={(RuntimeFeature.IsDynamicCodeSupported ? "JIT" : "NativeAOT")}; dynamicCodeCompiled={RuntimeFeature.IsDynamicCodeCompiled}");
Console.WriteLine("corelib=" + typeof(object).Assembly.FullName);
string mode = args.Length == 0 ? "safe" : args[0];
switch (mode)
{
    case "dump-il":
        if (RuntimeFeature.IsDynamicCodeSupported) Diagnostics.DumpIl();
        else Console.WriteLine("dump-il=SKIPPED; Native AOT has no managed IL bodies.");
        break;
    case "invalid-il": Diagnostics.CallInvalidType(violateMetadata: false); break;
    case "invalid-constraint": Diagnostics.CallInvalidType(violateMetadata: true); break;
    default: ExperimentRunner.Run(mode, new ExperimentContext(Console.WriteLine)); break;
}
