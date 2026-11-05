using System;
using System.Runtime.CompilerServices;
using InlineIL;
using static InlineIL.IL.Emit;

namespace DisposeResearch;

public static partial class IlCalls
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RuntimeTypeHandle TypeHandle<T>()
    {
        Ldtoken(typeof(T));
        return IL.Return<RuntimeTypeHandle>();
    }

    static class Traits<T>
    {
        internal static readonly bool IsDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T));
    }

    public static void TryDispose<T>(ref T value)
    {
        if (Traits<T>.IsDisposable) Dispose(ref value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Dispose<T>(ref T value)
    {
        Ldarg(nameof(value));
        Constrained(typeof(T));
        Callvirt(new MethodRef(typeof(IDisposable), nameof(IDisposable.Dispose)));
    }
}
