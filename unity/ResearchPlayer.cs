using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DisposeResearch;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.Profiling;

public static class ResearchPlayer
{
    static object[] retainedAllocations;

    static void RunAllocationCounters(Action<string> log)
    {
        using var allocatedInFrame = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
        var counters = new List<(string Name, Func<long> Read)>
        {
            ("current-thread", GC.GetAllocatedBytesForCurrentThread),
            ("total-memory", () => GC.GetTotalMemory(false)),
            ("mono-used", Profiler.GetMonoUsedSizeLong),
            ("unity-total-allocated", Profiler.GetTotalAllocatedMemoryLong)
        };
        log($"counter-api; profiler-gc-allocated-in-frame-valid={allocatedInFrame.Valid}; unit={allocatedInFrame.UnitType}");
        if (allocatedInFrame.Valid)
            counters.Add(("profiler-gc-allocated-in-frame", () => allocatedInFrame.CurrentValue));
        var method = typeof(GC).GetMethod("GetTotalAllocatedBytes", new[] { typeof(bool) });
        if (method == null)
            log("counter-api; total-allocated=unavailable");
        else
        {
            var total = (Func<bool, long>)Delegate.CreateDelegate(typeof(Func<bool, long>), method);
            counters.Add(("total-allocated-precise", () => total(true)));
            counters.Add(("total-allocated-approximate", () => total(false)));
        }

        const int count = 100_000;
        retainedAllocations = new object[count];
        // Warm up the readers before taking any differences.
        foreach (var counter in counters)
            for (int i = 0; i < 10; i++) counter.Read();
        Action one = () => retainedAllocations[0] = new object();
        Action many = () =>
        {
            for (int i = 0; i < count; i++) retainedAllocations[i] = new object();
        };
        Action collect = () =>
        {
            Array.Clear(retainedAllocations, 0, count);
            GC.Collect();
        };
        one();
        foreach (var counter in counters)
        {
            Array.Clear(retainedAllocations, 0, count);
            GC.Collect();
            Observe("no-op", () => { }, counter);
            Observe("one-object", one, counter);
            Observe("retained-100000-objects", many, counter);
            Observe("release-and-collect", collect, counter);
        }
        retainedAllocations = null;

        void Observe(string name, Action action, (string Name, Func<long> Read) counter)
        {
            int collections = GC.CollectionCount(0);
            long before = counter.Read();
            action();
            long after = counter.Read();
            int collected = GC.CollectionCount(0) - collections;
            // Formatting and logging are outside the measured interval.
            log($"counter-probe; case={name}; counter={counter.Name}; before={before}; after={after}; delta={after - before}; collections={collected}");
        }
    }

    static long CountAllocationEvents(Action action, int iterations)
    {
        using (var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc", iterations + 128,
            ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
        {
            if (!recorder.Valid) return -1;
            int countBefore = recorder.Count;
            for (int i = 0; i < iterations; i++) action();
            int countAfter = recorder.Count;
            return recorder.WrappedAround ? -1 : countAfter - countBefore;
        }
    }
    public static string Argument(string name, string fallback)
    {
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
    }

    public static void Run()
    {
        string path = Argument("--research-output", Path.Combine(Application.persistentDataPath, "research.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        using (var writer = new StreamWriter(path))
        {
            writer.AutoFlush = true;
            Action<string> log = line => { writer.WriteLine(line); Debug.Log("RESEARCH " + line); };
            log("unity=" + Application.unityVersion + "; editor=" + Application.isEditor + "; runtime=" + RuntimeInformation.FrameworkDescription + "; arch=" + RuntimeInformation.ProcessArchitecture);
            log("corelib=" + typeof(object).Assembly.FullName);
#if ENABLE_IL2CPP
            log("backend=IL2CPP");
            const bool isIl2Cpp = true;
#elif ENABLE_CORECLR
            log("backend=CoreCLR");
            const bool isIl2Cpp = false;
#else
            log("backend=JIT; mono=" + (Type.GetType("Mono.Runtime") != null));
            const bool isIl2Cpp = false;
#endif
            string mode = Argument("--research-case", "safe");
            if (mode == "allocation-counters") RunAllocationCounters(log);
            else
            {
                using var allocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
#if ENABLE_CORECLR && !ENABLE_IL2CPP
                Func<long> readBytes = null;
                log("allocation-byte-counter=GC.GetAllocatedBytesForCurrentThread; scope=current-thread");
#else
                Func<long> readBytes = allocated.Valid ? () => allocated.CurrentValue : null;
                log($"allocation-byte-counter=GC Allocated In Frame; valid={allocated.Valid}; unit={allocated.UnitType}; scope=all-threads-in-current-frame");
#endif
                var context = new ExperimentContext(log, isIl2Cpp, CountAllocationEvents, readBytes);
                ExperimentRunner.Run(mode, context);
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Start()
    {
        if (Application.isEditor) return;
        try { Run(); Application.Quit(0); }
        catch (Exception ex) { Debug.LogException(ex); Application.Quit(2); }
    }
}
