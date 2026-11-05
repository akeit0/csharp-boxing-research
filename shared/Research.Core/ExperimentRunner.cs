using System;
using DisposeResearch.Experiments;

namespace DisposeResearch;

public static class ExperimentRunner
{
    public static void Run(string name, ExperimentContext context)
    {
        context.Log($"mode={name}; pointerSize={IntPtr.Size}; mono={context.HasMonoRuntimeType}");
        context.Allocations.Calibrate();

        switch (name)
        {
            case "safe": DisposeExperiments.Run(context); break;
            case "forge": RawStackBoxExperiment.Run(context, collect: false, useTypeHandle: false); break;
            case "forge-gc": RawStackBoxExperiment.Run(context, collect: true, useTypeHandle: false); break;
            case "forge-typehandle": RawStackBoxExperiment.Run(context, collect: false, useTypeHandle: true); break;
            case "forge-reference-root": RawReferenceRootControl.Run(context); break;
            case "stack-box": StackGcExperiments.Run(context); break;
            case "dim-receiver-escape": DimReceiverEscape.Run(context); break;
            case "constraint-only": ConstraintExperiments.RunConstant(context); break;
            case "metadata-constraint": ConstraintExperiments.RunDispose(context); break;
            default: throw new ArgumentException($"Unknown experiment: {name}", nameof(name));
        }
    }
}
