using System;
using System.Runtime.CompilerServices;

namespace DisposeResearch.Experiments;

internal static class RawReferenceRootControl
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe WeakReference WriteUntrackedReference(IntPtr slot)
    {
        object target = new byte[8192];
        var weak = new WeakReference(target);
        *(IntPtr*)slot = Unsafe.As<object, IntPtr>(ref target);
        return weak;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object CreateReferenceControl(out WeakReference weak)
    {
        object target = new byte[8192];
        weak = new WeakReference(target);
        return (object)new ReferenceValue { Value = target };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ReferenceSlotMatches(object receiver, WeakReference weak) =>
        ReferenceEquals(Unsafe.Unbox<ReferenceValue>(receiver).Value, weak.Target);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe void Run(ExperimentContext context)
    {
        context.RequireCoreClrLayoutX64();
        byte* storage = stackalloc byte[128];
        for (int i = 0; i < 128; i++)
            storage[i] = 0;
        byte* address = storage + 40;
        *(IntPtr*)address = typeof(ReferenceValue).TypeHandle.Value;
        IntPtr raw = (IntPtr)address;
        object receiver = Unsafe.As<IntPtr, object>(ref raw);
        WeakReference weak = WriteUntrackedReference((IntPtr)(address + IntPtr.Size));
        object control = CreateReferenceControl(out WeakReference controlWeak);
        bool before = weak.IsAlive;
        bool slotMatches = ReferenceSlotMatches(receiver, weak);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        bool after = weak.IsAlive;
        bool controlAfter = controlWeak.IsAlive;
        *(IntPtr*)(address + IntPtr.Size) = IntPtr.Zero;
        GC.KeepAlive(receiver);
        GC.KeepAlive(control);
        context.Log($"forge-reference-root; slotMatches={slotMatches}; aliveBefore={before}; aliveAfter={after}; controlAliveAfter={controlAfter}; forgedReceiverKeptAlive=True; deadTargetDereferenced=False");
    }
}
