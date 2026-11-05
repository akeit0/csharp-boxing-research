using System;
using System.Runtime.CompilerServices;
using InlineIL;
using static InlineIL.IL.Emit;

namespace DisposeResearch;

public static partial class IlCalls
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ConstrainedMetadata<T>(ref T value) where T : IDisposable
    {
        Ldarg(nameof(value));
        Constrained(typeof(T));
        Callvirt(new MethodRef(typeof(IDisposable), nameof(IDisposable.Dispose)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void CallConstrainedMetadataWithoutProof<T>(ref T value)
    {
        Ldarg(nameof(value));
        Call(new MethodRef(typeof(IlCalls), nameof(ConstrainedMetadata)).MakeGenericMethod(typeof(T)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ConstraintOnly<T>() where T : IDisposable => 123;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CallConstraintOnlyWithoutProof<T>()
    {
        Call(new MethodRef(typeof(IlCalls), nameof(ConstraintOnly)).MakeGenericMethod(typeof(T)));
        return IL.Return<int>();
    }
}
