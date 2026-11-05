using System;

namespace DisposeResearch.Experiments;

internal static class ConstraintExperiments
{
    internal static void RunConstant(ExperimentContext context)
    {
        Action<string> log = context.Log;
        log($"constraint-only-metadata-count={typeof(IlCalls).GetMethod(nameof(IlCalls.ConstraintOnly)).GetGenericArguments()[0].GetGenericParameterConstraints().Length}");
        try { log($"constraint-only-IL-result={IlCalls.CallConstraintOnlyWithoutProof<int>()}"); }
        catch (Exception ex) { log($"constraint-only-IL-error={ex.GetType().FullName}; {ex.Message}"); }

        try
        {
            var method = typeof(IlCalls).GetMethod(nameof(IlCalls.ConstraintOnly)).MakeGenericMethod(typeof(int));
            log($"constraint-only-reflection-constructed={method}");
            log($"constraint-only-reflection-result={method.Invoke(null, null)}");
        }
        catch (Exception ex) { LogException("constraint-only-reflection", ex, log); }
    }

    internal static void RunDispose(ExperimentContext context)
    {
        Action<string> log = context.Log;
        try
        {
            var definition = typeof(IlCalls).GetMethod(nameof(IlCalls.ConstrainedMetadata));
            log($"metadata-definition-constraints={definition.GetGenericArguments()[0].GetGenericParameterConstraints().Length}");
            var constructed = definition.MakeGenericMethod(typeof(int));
            log($"metadata-construction={constructed}");
            constructed.Invoke(null, new object[] { 7 });
            log("metadata-invocation=returned");
        }
        catch (Exception ex)
        {
            log($"metadata-invocation={ex.GetType().FullName}; {ex.Message}");
            if (ex.InnerException != null)
                log($"metadata-inner={ex.InnerException.GetType().FullName}; {ex.InnerException.Message}");
        }
    }

    internal static void ObserveMetadata(Action<string> log)
    {
        try
        {
            typeof(IlCalls).GetMethod(nameof(IlCalls.ConstrainedMetadata)).MakeGenericMethod(typeof(int));
            log("metadataConstraint=construction accepted; invocation is a separate experiment");
        }
        catch (Exception ex) { log($"metadataConstraint={ex.GetType().FullName}"); }
        log($"IL-genericConstraintCount={typeof(IlCalls).GetMethod(nameof(IlCalls.Dispose)).GetGenericArguments()[0].GetGenericParameterConstraints().Length}");
        log($"nullableAssignable={typeof(IDisposable).IsAssignableFrom(typeof(Counter?))}; nullableBoxType={((object)(Counter?)new Counter()).GetType().FullName}");
    }

    private static void LogException(string prefix, Exception ex, Action<string> log)
    {
        log($"{prefix}-error={ex.GetType().FullName}; {ex.Message}");
        if (ex.InnerException != null)
            log($"{prefix}-inner={ex.InnerException.GetType().FullName}; {ex.InnerException.Message}");
    }
}
