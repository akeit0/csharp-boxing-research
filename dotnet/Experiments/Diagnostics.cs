using System;
using System.Runtime.CompilerServices;

namespace DisposeResearch.DotNet;

internal static class Diagnostics
{
    internal static void DumpIl()
    {
        foreach (string name in new[] { nameof(IlCalls.Dispose), nameof(IlCalls.ConstrainedMetadata), nameof(IlCalls.CallConstrainedMetadataWithoutProof) })
        {
            var method = typeof(IlCalls).GetMethod(name)!;
            int constraints = method.GetGenericArguments()[0].GetGenericParameterConstraints().Length;
            Console.WriteLine($"{name}; constraints={constraints}; IL={Convert.ToHexString(method.GetMethodBody()!.GetILAsByteArray()!)}");
        }
        foreach (var reference in typeof(IlCalls).Assembly.GetReferencedAssemblies())
            Console.WriteLine($"IL-reference={reference.FullName}");
        var handle = typeof(IlCalls).GetMethod(nameof(IlCalls.TypeHandle))!;
        Console.WriteLine($"TypeHandle; IL={Convert.ToHexString(handle.GetMethodBody()!.GetILAsByteArray()!)}");
    }

    // Isolate the two invalid calls so AOT compilation of one cannot affect the other.
    internal static void CallInvalidType(bool violateMetadata)
    {
        int value = 7;
        try
        {
            if (violateMetadata)
                CallInvalidConstraint(ref value);
            else
                CallInvalidDispose(ref value);
            Console.WriteLine($"invalid-il unexpectedly returned; value={value}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"invalid-il={ex.GetType().FullName}; {ex.Message}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CallInvalidDispose(ref int value) => IlCalls.Dispose(ref value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CallInvalidConstraint(ref int value) => IlCalls.CallConstrainedMetadataWithoutProof(ref value);
}
