using System;
using System.Runtime.CompilerServices;

namespace DisposeResearch.Experiments;

internal static class DimReceiverEscape
{
    public interface IRetainingDisposable : IDisposable
    {
        int Tag { get; set; }

        void IDisposable.Dispose()
        {
            CapturedTag = Tag;
            PreviousCaptureTag = Retained is IRetainingDisposable previous ? previous.Tag : -1;
            Retained = this;
        }
    }

    public struct State : IRetainingDisposable { public int Tag { get; set; } }

    public static object Retained;
    public static int CapturedTag;
    public static int PreviousCaptureTag;

    static void CachedBox(Action<string> log)
    {
        State value = new State { Tag = 101 };
        DisposeResearch.CachedBox<State>.Dispose(ref value);
        object firstCapture = Retained;
        int firstTag = CapturedTag;
        int afterReset = ((IRetainingDisposable)firstCapture).Tag;
        value.Tag = 202;
        DisposeResearch.CachedBox<State>.Dispose(ref value);
        log("dim-escape; case=cached-box; firstCapturedTag=" + firstTag
            + "; firstCaptureAfterReset=" + afterReset
            + "; firstCaptureDuringSecondDispose=" + PreviousCaptureTag
            + "; secondCapturedTag=" + CapturedTag
            + "; sameReceiver=" + ReferenceEquals(firstCapture, Retained));
        Retained = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static unsafe void StackReceiver(Action<string> log)
    {
        StackBox<State> storage = default;
        storage.Value.Tag = 303;
        object receiver = StackBox.AsObject(ref storage);
        IntPtr raw = Unsafe.As<object, IntPtr>(ref receiver);
        try
        {
            ((IDisposable)receiver).Dispose();
            bool same = ReferenceEquals(receiver, Retained);
            bool sameAddress = Unsafe.As<object, IntPtr>(ref Retained) == raw;
            int captured = CapturedTag;
            storage.Value.Tag = 404;
            int afterWrite = ((IRetainingDisposable)Retained).Tag;
            log("dim-escape; case=stack-box; capturedTag=" + captured
                + "; sameReceiver=" + same + "; retainedAddressEqualsStack=" + sameAddress
                + "; retainedTagAfterStackWrite=" + afterWrite);
        }
        finally { Retained = null; }
    }

    internal static void Run(ExperimentContext context)
    {
        Retained = null;
        try
        {
            CachedBox(context.Log);
            if (IntPtr.Size == 8 && context.UsesCoreClrLayout)
                StackReceiver(context.Log);
        }
        finally { Retained = null; }
    }
}
