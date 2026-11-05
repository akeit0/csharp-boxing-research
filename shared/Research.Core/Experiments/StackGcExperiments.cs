using System;
using System.Runtime.CompilerServices;
using Payload = DisposeResearch.Experiments.GcProbe.Payload;

namespace DisposeResearch.Experiments;

internal static class StackGcExperiments
{
    private static int observed;

    internal static void Run(ExperimentContext context)
    {
        context.Log($"stack-box; runtime={context.Runtime}; headerSource=TypeHandle; seedBox=False");
        Counter counter = default;
        context.Allocations.MeasureFirstCall("stack-box-counter-first-call", () =>
            counter = Invoke(new Counter { Count = 41 }, context.Runtime));
        context.Log($"stack-box; case=counter; count={counter.Count}");
        if (counter.Count != 42)
            throw new InvalidOperationException("Counter mutation was not written to storage.Value.");
        var value = new Payload { Value = new object(), Count = 41 };
        Payload changed = default;
        context.Allocations.MeasureFirstCall("stack-box-reference-first-call", () =>
            changed = Invoke(value, context.Runtime));
        if (changed.Count != 42 || !ReferenceEquals(value.Value, changed.Value))
            throw new InvalidOperationException("The reference-bearing payload did not round-trip.");
        GenericCounter<object> generic = default;
        context.Allocations.MeasureFirstCall("stack-box-generic-first-call", () =>
            generic = Invoke(new GenericCounter<object> { Value = value.Value, Count = 41 }, context.Runtime));
        bool sameReference = ReferenceEquals(value.Value, generic.Value);
        context.Log($"stack-box; case=closed-generic; count={generic.Count}; sameReference={sameReference}");
        if (generic.Count != 42 || !sameReference)
            throw new InvalidOperationException("The closed generic payload did not round-trip.");
        ReferenceGc(context);
        context.Allocations.Measure("stack-box-counter", () =>
            observed = Invoke(new Counter { Count = 41 }, context.Runtime).Count);
        context.Allocations.Measure("stack-box-reference", () =>
            observed = Invoke(value, context.Runtime).Count);
        context.Log($"stack-box; observedCount={observed}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T Invoke<T>(T value, StackBoxRuntime runtime) where T : struct
    {
        StackBox<T> storage = default;
        storage.Value = value;
        object receiver = StackBox.AsObject(ref storage, runtime);
        ((IDisposable)receiver).Dispose();
        return storage.Value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe void ReferenceGc(ExperimentContext context)
    {
        StackBox<Payload> storage = default;
        WeakReference witness = GcProbe.Populate(ref storage.Value);
        IntPtr before = Unsafe.As<object, IntPtr>(ref storage.Value.Value);
        object receiver = StackBox.AsObject(ref storage, context.Runtime);
        IntPtr raw = Unsafe.As<object, IntPtr>(ref receiver);
        long offset = (byte*)Unsafe.AsPointer(ref storage.Value) - (byte*)raw;
        if (offset != (context.UsesCoreClrLayout ? 1 : 2) * IntPtr.Size || Unsafe.AsPointer(ref Unsafe.Unbox<Payload>(receiver)) != Unsafe.AsPointer(ref storage.Value))
            throw new NotSupportedException("The payload does not alias the managed struct.");

        ((IDisposable)receiver).Dispose();

        // After Dispose, observe only the weak witness and numeric addresses.
        GcProbe.Compact();
        bool alive = witness.IsAlive;
        IntPtr after = GcProbe.WitnessAddress(witness);
        IntPtr slot = *(IntPtr*)((byte*)raw + offset);
        int count = *(int*)((byte*)raw + offset + IntPtr.Size);
        context.Log($"stack-box-gc; payloadOffset={offset}; aliveAfter={alive}; moved={alive && before != after}; slotMatchesTarget={alive && slot == after}; count={count}");
        if (!alive || slot != after || count != 1)
            throw new InvalidOperationException("The stack box lost its reference or payload mutation.");
    }
}
