using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DisposeResearch;

internal delegate void RefAction<T>(ref T value);

internal static class CSharpDispose
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Constrained<T>(ref T value) where T : IDisposable => value.Dispose();

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Pattern<T>(ref T value)
    {
        if (value is IDisposable disposable)
            disposable.Dispose();
    }
}

internal static class CachedDispatch<T>
{
    internal static readonly RefAction<T> Call = Create();

    private static RefAction<T> Create()
    {
        if (!typeof(IDisposable).IsAssignableFrom(typeof(T)))
            return Ignore;

        MethodInfo method = typeof(CSharpDispose)
            .GetMethod(nameof(CSharpDispose.Constrained), BindingFlags.Static | BindingFlags.NonPublic)
            .MakeGenericMethod(typeof(T));
        return (RefAction<T>)Delegate.CreateDelegate(typeof(RefAction<T>), method);
    }

    private static void Ignore(ref T value) { }
}
