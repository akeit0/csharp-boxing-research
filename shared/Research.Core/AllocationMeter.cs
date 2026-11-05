using System;
using System.Globalization;

namespace DisposeResearch;

internal sealed class AllocationMeter
{
    internal const int Iterations = 100_000;
    private const int WarmupIterations = 1_000;
    private static object calibrationSink;
    private readonly Action<string> log;
    private readonly Func<Action, int, long> countEvents;
    private readonly Func<long> readByteCounter;
    private bool byteCounterAvailable;

    internal AllocationMeter(Action<string> log, Func<Action, int, long> countEvents, Func<long> readByteCounter)
    {
        this.log = log;
        this.countEvents = countEvents;
        this.readByteCounter = readByteCounter ?? GC.GetAllocatedBytesForCurrentThread;
    }

    internal void Calibrate()
    {
        long before = readByteCounter();
        calibrationSink = new object();
        long bytes = readByteCounter() - before;
        byteCounterAvailable = bytes > 0;
        calibrationSink = null;
        log($"allocation-calibration; objectBytes={bytes}; byteCounterValid={byteCounterAvailable}");
    }

    internal long ReadBytes() => byteCounterAvailable ? readByteCounter() : -1;
    internal long BytesSince(long before) => before < 0 ? -1 : ReadBytes() - before;

    internal void MeasureFirstCall(string name, Action action)
    {
        long bytes = -1;
        Action measured = () =>
        {
            long before = ReadBytes();
            action();
            bytes = BytesSince(before);
        };
        long events = -1;
        if (countEvents == null)
            measured();
        else
            events = countEvents(measured, 1);
        log($"allocation; case={name}; iterations=1; bytes={bytes}; events={events}");
    }

    internal void Measure(string name, Action action)
    {
        for (int i = 0; i < WarmupIterations; i++)
            action();

        long before = ReadBytes();
        for (int i = 0; i < Iterations; i++)
            action();
        long bytes = BytesSince(before);
        long events = countEvents == null ? -1 : countEvents(action, Iterations);
        string perCall = bytes < 0 ? "unavailable" : ((double)bytes / Iterations).ToString("F3", CultureInfo.InvariantCulture);
        log($"allocation; case={name}; iterations={Iterations}; bytes={bytes}; events={events}; bytesPerCall={perCall}");
    }
}
