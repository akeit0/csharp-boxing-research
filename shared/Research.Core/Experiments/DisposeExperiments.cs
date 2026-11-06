using System;
using System.Runtime.CompilerServices;

namespace DisposeResearch.Experiments;

internal static class DisposeExperiments
{
    private static object escapedBox;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object Capture<T>(ref T value) where T : IDefault => value.Capture();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void BoxAndEscape(ref Counter value)
    {
        object receiver = value;
        escapedBox = receiver;
        ((IDisposable)receiver).Dispose();
    }

    internal static void Run(ExperimentContext context)
    {
        RunAllocations(context);
        RunValueShapes(context.Log);
        RunExceptions(context.Log);
        ConstraintExperiments.ObserveMetadata(context.Log);
        escapedBox = null;
        context.Log("done; checksum=0");
    }

    private static void RunAllocations(ExperimentContext context)
    {
        var counter = new Counter();
        var dim = new DefaultValue();
        object captured = Capture(ref dim);
        context.Log($"DIM; capturedType={captured.GetType().FullName}; isValueType={captured.GetType().IsValueType}; originalCount={dim.Count}");
        ((IDisposable)captured).Dispose();
        context.Log($"DIM; afterDisposeOriginal={dim.Count}; boxCount={((IDefault)captured).Count}; sameObject={ReferenceEquals(captured, ((IDefault)captured).Capture())}");
        CSharpDispose.Constrained(ref dim);
        context.Log($"DIM; constrainedDefaultDisposeOriginal={dim.Count}");

        AllocationMeter meter = context.Allocations;
        meter.Measure("box-escape", () => BoxAndEscape(ref counter));
        meter.Measure("is-IDisposable", () => CSharpDispose.Pattern(ref counter));
        meter.Measure("is-IDisposable-then-cast", () => CSharpDispose.PatternThenCast(ref counter));
        var writeBack = new Counter { Count = 41 };
        CSharpDispose.PatternThenCastWithWriteBack(ref writeBack);
        context.Log($"is-IDisposable-then-cast-with-write-back; countAfterFirstCall={writeBack.Count}");
        meter.Measure("is-IDisposable-then-cast-with-write-back",
            () => CSharpDispose.PatternThenCastWithWriteBack(ref writeBack));
        meter.Measure("constrained-CSharp", () => CSharpDispose.Constrained(ref counter));
        meter.Measure("DIM-capture-escape", () => escapedBox = Capture(ref dim));
        meter.Measure("DIM-constrained-default", () => CSharpDispose.Constrained(ref dim));
        meter.Measure("cached-box", () => CachedBox<Counter>.Dispose(ref counter));
        meter.Measure("unconstrained-IL", () => IlCalls.Dispose(ref counter));
        meter.Measure("guarded-IL", () => IlCalls.TryDispose(ref counter));
        if (!context.IsIl2Cpp)
            meter.Measure("cached-reflection-dispatch", () => CachedDispatch<Counter>.Call(ref counter));
        else
            context.Log("cached-reflection-dispatch=SKIPPED; dynamic generic instantiation is not an AOT guarantee");
        context.Log($"mutation; counter={counter.Count}; dimOriginal={dim.Count}");
    }

    private static void RunValueShapes(Action<string> log)
    {
        var explicitValue = new ExplicitCounter();
        IlCalls.Dispose(ref explicitValue);
        int nonDisposable = 7;
        IlCalls.TryDispose(ref nonDisposable);
        var metadataValid = new Counter();
        IlCalls.ConstrainedMetadata(ref metadataValid);
        try
        {
            IlCalls.CallConstrainedMetadataWithoutProof(ref metadataValid);
            log("unproven-generic-call=accepted");
        }
        catch (Exception ex)
        {
            log($"unproven-generic-call={ex.GetType().FullName}; {ex.Message}");
        }
        log($"IL-guard; nonDisposable={nonDisposable}; validMetadataCalls={metadataValid.Count}");

        var referenceValue = new ReferenceCounter();
        IlCalls.Dispose(ref referenceValue);
        IDisposable interfaceValue = referenceValue;
        IlCalls.Dispose(ref interfaceValue);
        var refs = new ReferenceValue { Value = new object() };
        IlCalls.Dispose(ref refs);
        log($"IL-shapes; explicit={explicitValue.Count}; class={referenceValue.Count}; managedField={refs.Count}; fieldCleared={refs.Value == null}");
    }

    private static void RunExceptions(Action<string> log)
    {
        var value = new ThrowingCounter();
        try { IlCalls.Dispose(ref value); }
        catch (InvalidOperationException) { }
        log($"exception; directILMutation={value.Count}");

        value = default;
        try { CachedBox<ThrowingCounter>.Dispose(ref value); }
        catch (InvalidOperationException) { }
        log($"exception; articleStyleMutation={value.Count}");

        value = default;
        try { CachedBox<ThrowingCounter>.Dispose(ref value, copyOnException: true); }
        catch (InvalidOperationException) { }
        log($"exception; finallyCopyMutation={value.Count}");
    }
}
