using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisposeResearch.Experiments;

internal static class GcProbe
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Payload : IDisposable
    {
        public object Value;
        public int Count;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Dispose() => Count++;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static WeakReference Populate(ref Payload payload)
    {
        object gap = new byte[32000];
        object target = new byte[8192];
        payload.Value = target;
        var weak = new WeakReference(target);
        GC.KeepAlive(gap);
        return weak;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static IntPtr WitnessAddress(WeakReference witness)
    {
        object target = witness.Target;
        return target == null ? IntPtr.Zero : Unsafe.As<object, IntPtr>(ref target);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Compact() =>
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
}
