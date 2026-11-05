using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DisposeResearch;

public enum StackBoxRuntime
{
    CoreClr,
    Mono,
    Il2Cpp
}

[StructLayout(LayoutKind.Sequential)]
public struct StackBox<T> where T : struct
{
    public IntPtr Prefix0;
    public IntPtr Prefix1;
    public T Value;
}

public static class StackBox
{
    public static unsafe object AsObject<T>(ref StackBox<T> storage,
        StackBoxRuntime runtime = StackBoxRuntime.CoreClr) where T : struct
    {
        IntPtr typeWord = runtime switch
        {
            StackBoxRuntime.CoreClr => typeof(T).TypeHandle.Value,
            StackBoxRuntime.Mono => MonoHeader<T>.TypeWord,
            StackBoxRuntime.Il2Cpp => Il2CppHeader<T>.TypeWord,
            _ => throw new ArgumentOutOfRangeException(nameof(runtime))
        };
        byte* start = (byte*)Unsafe.AsPointer(ref storage.Prefix0);
        byte* payload = (byte*)Unsafe.AsPointer(ref storage.Value);
        if (payload - start < 2 * IntPtr.Size)
            throw new NotSupportedException("Insufficient prefix before the payload.");
        IntPtr* header = (IntPtr*)(payload - 2 * IntPtr.Size);
        bool coreClr = runtime == StackBoxRuntime.CoreClr;
        header[0] = coreClr ? IntPtr.Zero : typeWord;
        header[1] = coreClr ? typeWord : IntPtr.Zero;
        IntPtr raw = (IntPtr)(coreClr ? header + 1 : header);
        return Unsafe.As<IntPtr, object>(ref raw);
    }

    private static class MonoHeader<T> where T : struct
    {
        internal static readonly IntPtr TypeWord;

        // Explicit initialization keeps the native call at the first field read.
        static MonoHeader() => TypeWord = RuntimeTypeMetadata.MonoVTable(IlCalls.TypeHandle<T>());
    }

    private static class Il2CppHeader<T> where T : struct
    {
        internal static readonly IntPtr TypeWord;

        static Il2CppHeader() => TypeWord = RuntimeTypeMetadata.Il2CppClass(IlCalls.TypeHandle<T>());
    }
}
