using System;
using System.Runtime.CompilerServices;

namespace DisposeResearch.Experiments;

internal static class RawStackBoxExperiment
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static unsafe void Run(ExperimentContext context, bool collect, bool useTypeHandle)
    {
        if (useTypeHandle)
            context.RequireCoreClrLayoutX64();

        bool twoWordHeader = context.HasMonoRuntimeType || context.IsIl2Cpp;
        object seed = useTypeHandle ? null : (object)new Counter();
        int payloadOffset;
        IntPtr typeWord;
        if (useTypeHandle)
        {
            payloadOffset = IntPtr.Size;
            typeWord = typeof(Counter).TypeHandle.Value;
        }
        else
        {
            fixed (int* payload = &Unsafe.Unbox<Counter>(seed).Count)
            {
                IntPtr reference = Unsafe.As<object, IntPtr>(ref seed);
                payloadOffset = checked((int)((byte*)payload - (byte*)reference));
                typeWord = *(IntPtr*)reference;
            }
        }

        int prefix = twoWordHeader ? 0 : IntPtr.Size;
        int expectedOffset = (twoWordHeader ? 2 : 1) * IntPtr.Size;
        if (payloadOffset != expectedOffset)
            throw new NotSupportedException($"Unexpected boxed payload offset: {payloadOffset}");
        context.Log($"forge-layout; payloadOffset={payloadOffset}; prefix={prefix}; headerWord=0x{typeWord.ToInt64():x}; il2cpp={context.IsIl2Cpp}; seedBox={!useTypeHandle}");

        byte* storage = stackalloc byte[128];
        for (int i = 0; i < 128; i++)
            storage[i] = 0;
        byte* address = storage + 32 + prefix;
        *(IntPtr*)address = typeWord;
        *(int*)(address + payloadOffset) = 41;
        IntPtr raw = (IntPtr)address;
        object receiver = Unsafe.As<IntPtr, object>(ref raw);
        IDisposable disposable = (IDisposable)receiver;
        disposable.Dispose();
        int first = *(int*)(address + payloadOffset);

        long before = context.Allocations.ReadBytes();
        for (int i = 0; i < AllocationMeter.Iterations; i++)
            disposable.Dispose();
        long bytes = context.Allocations.BytesSince(before);
        if (collect)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            disposable.Dispose();
        }

        int final = *(int*)(address + payloadOffset);
        bool outerBytesIntact = storage[0] == 0 && storage[127] == 0;
        disposable = null;
        receiver = null;
        GC.KeepAlive(seed);
        context.Log($"forge-result; first={first}; final={final}; bytes={bytes}; forcedGC={collect}; outerBytesIntact={outerBytesIntact}");
    }
}
