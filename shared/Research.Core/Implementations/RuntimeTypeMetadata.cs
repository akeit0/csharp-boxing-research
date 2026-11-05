using System;
using System.Runtime.InteropServices;

namespace DisposeResearch;

// Native runtime entry points exported by the Windows Unity Players.
internal static class RuntimeTypeMetadata
{
    internal static IntPtr MonoVTable(RuntimeTypeHandle type)
    {
        IntPtr klass = mono_class_from_mono_type(type.Value);
        IntPtr vtable = mono_class_vtable(mono_domain_get(), klass);
        return vtable != IntPtr.Zero ? vtable : throw new TypeLoadException("mono_class_vtable returned null.");
    }

    internal static IntPtr Il2CppClass(RuntimeTypeHandle type) => il2cpp_class_from_type(type.Value);

    [DllImport("mono-2.0-bdwgc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr mono_domain_get();

    [DllImport("mono-2.0-bdwgc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr mono_class_from_mono_type(IntPtr type);

    [DllImport("mono-2.0-bdwgc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr mono_class_vtable(IntPtr domain, IntPtr klass);

    [DllImport("GameAssembly", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr il2cpp_class_from_type(IntPtr type);
}
