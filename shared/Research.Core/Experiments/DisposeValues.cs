using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisposeResearch.Experiments;

internal interface IDefault : IDisposable
{
    int Count { get; set; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    object Capture() => this;

    void IDisposable.Dispose() => Count++;
}

internal struct DefaultValue : IDefault
{
    public int Count { get; set; }
}

[StructLayout(LayoutKind.Sequential)]
internal struct Counter : IDisposable
{
    public int Count;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Dispose() => Count++;
}

internal struct GenericCounter<T> : IDisposable
{
    public T Value;
    public int Count;

    public void Dispose() => Count++;
}

internal sealed class ReferenceCounter : IDisposable
{
    public int Count;
    public void Dispose() => Count++;
}

internal struct ExplicitCounter : IDisposable
{
    public int Count;
    void IDisposable.Dispose() => Count++;
}

internal struct ThrowingCounter : IDisposable
{
    public int Count;

    public void Dispose()
    {
        Count++;
        throw new InvalidOperationException("after mutation");
    }
}

internal struct ReferenceValue : IDisposable
{
    public object Value;
    public int Count;

    public void Dispose()
    {
        Count++;
        Value = null;
    }
}
