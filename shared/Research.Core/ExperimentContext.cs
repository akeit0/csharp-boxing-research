using System;

namespace DisposeResearch;

public sealed class ExperimentContext
{
    public Action<string> Log { get; }
    public bool IsIl2Cpp { get; }
    public bool HasMonoRuntimeType { get; } = Type.GetType("Mono.Runtime") != null;
    public bool UsesCoreClrLayout => !IsIl2Cpp && !HasMonoRuntimeType;
    public StackBoxRuntime Runtime => IsIl2Cpp ? StackBoxRuntime.Il2Cpp :
        HasMonoRuntimeType ? StackBoxRuntime.Mono : StackBoxRuntime.CoreClr;
    internal AllocationMeter Allocations { get; }

    public ExperimentContext(Action<string> log, bool isIl2Cpp = false,
        Func<Action, int, long> countAllocationEvents = null, Func<long> readAllocationBytes = null)
    {
        Log = log ?? throw new ArgumentNullException(nameof(log));
        IsIl2Cpp = isIl2Cpp;
        Allocations = new AllocationMeter(log, countAllocationEvents, readAllocationBytes);
    }

    internal void RequireCoreClrLayoutX64()
    {
        if (!UsesCoreClrLayout || IntPtr.Size != 8)
            throw new NotSupportedException("This case requires the CoreCLR / Native AOT x64 layout.");
    }
}
