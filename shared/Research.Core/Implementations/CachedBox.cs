using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DisposeResearch;

internal static class CachedBox<T> where T : struct
{
    private static readonly Stack<IDisposable> Pool = new(1);

    internal static void Dispose(ref T value, bool copyOnException = false)
    {
        if (!Pool.TryPop(out IDisposable receiver))
            receiver = (IDisposable)(object)default(T);

        Unsafe.Unbox<T>(receiver) = value;
        try
        {
            receiver.Dispose();
            if (!copyOnException)
                value = Unsafe.Unbox<T>(receiver);
        }
        finally
        {
            if (copyOnException)
                value = Unsafe.Unbox<T>(receiver);
            Unsafe.Unbox<T>(receiver) = default;
            Pool.Push(receiver);
        }
    }
}
