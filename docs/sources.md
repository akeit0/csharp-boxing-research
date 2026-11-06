# 参照資料

2026 年 11 月 5～6 日に確認。仕様、ランタイム実装、計測 API の参照先。

| 資料 | 調査に使う点 |
| --- | --- |
| [元の記事](https://qiita.com/Omochaya/items/c396ca2910e1c5e90e92) | ボックスを再利用する copy-in / call / copy-out |
| [C# の default interface methods 仕様案](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-8.0/default-interface-methods.md) | 既定実装と値型・インターフェイス呼び出しの関係 |
| [OpCodes.Constrained](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.constrained) | managed pointer を渡し、値型が実装する場合に直接呼び出す規則 |
| [InlineIL.Fody](https://github.com/ltrzesniewski/InlineIL.Fody) | ビルド時 IL 埋め込み、PrivateAssets、実行時参照の除去 |
| [System.Runtime.CompilerServices.Unsafe](https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.1.2) | Microsoft が提供する低水準 API と対応ターゲット |
| [CoreCLR v10.0.0 object.h](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/vm/object.h) | MethodTable と負オフセットのオブジェクトヘッダー |
| [Native AOT のビルド要件と発行方法](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) | PublishAot、Windows の C++ ビルドツール |
| [Native AOT v10.0.0 ObjectLayout.h](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/nativeaot/Runtime/ObjectLayout.h) | 型情報ポインターと、その手前にあるオブジェクトヘッダー |
| [Native AOT v10.0.0 RuntimeTypeHandle.cs](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/nativeaot/System.Private.CoreLib/src/System/RuntimeTypeHandle.cs) | MethodTable を指すハンドル |
| [NTSTATUS の一覧](https://learn.microsoft.com/ja-jp/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55) | 0xC0000602 = STATUS_FAIL_FAST_EXCEPTION |
| [CoreCLR v10.0.0 typehandle.h](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/vm/typehandle.h) | 値型の TypeHandle と MethodTable |
| [PR #103361](https://github.com/dotnet/runtime/pull/103361) | .NET 9 の JIT による、エスケープしないボックスのスタック配置 |
| [PR #104411](https://github.com/dotnet/runtime/pull/104411) | Native AOT / ReadyToRun のスタック配置対応 |
| [Stack Allocation Enhancements #104936](https://github.com/dotnet/runtime/issues/104936) | オブジェクトのスタック配置の改善項目 |
| [Issue #126501](https://github.com/dotnet/runtime/issues/126501) | 構造体から既定実装を呼ぶ際のボックス化を省く提案 |
| [Generic 型の Explicit layout](https://github.com/dotnet/runtime/issues/97526) | `StackBox<T>` には Sequential を使用 |
| [CoreCLR の GC 設計](https://github.com/dotnet/runtime/blob/v10.0.0/docs/design/coreclr/botr/garbage-collection.md) | GC の root と参照の更新 |
| [GC.KeepAlive](https://learn.microsoft.com/en-us/dotnet/api/system.gc.keepalive) | object 参照の生存期間の指定 |
| [RyuJIT v10.0.0 の GC Info](https://github.com/dotnet/runtime/blob/v10.0.0/docs/design/coreclr/jit/ryujit-overview.md#gc-info) | untracked なスタック位置をメソッド全体で live として報告する仕組み |
| [CoreCLR v10.0.0 lclvars.cpp](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/jit/lclvars.cpp#L2816) | address-exposed なローカルを細かな liveness の追跡対象から外す |
| [CoreCLR v10.0.0 gcencode.cpp](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/jit/gcencode.cpp#L3917) | フレーム上の struct の GC フィールドを GC_SLOT_UNTRACKED として登録 |
| [Mono object.h](https://github.com/mono/mono/blob/main/mono/metadata/object.h) | MonoObject の vtable と同期用ポインター |
| [Unity Mono の class.h](https://github.com/Unity-Technologies/mono/blob/unity-6000.3-mbe/mono/metadata/class.h) | `mono_class_from_mono_type` と `mono_class_vtable` の公開宣言 |
| [Unity Mono の class.c](https://github.com/Unity-Technologies/mono/blob/unity-6000.3-mbe/mono/metadata/class.c#L2011) | MonoType から通常の型・generic 型の MonoClass を取得する経路 |
| [Unity Mono の object.c](https://github.com/Unity-Technologies/mono/blob/unity-6000.3-mbe/mono/metadata/object.c#L1775) | domain ごとの vtable の取得・作成と MonoReflectionType の保持 |
| [Unity 6.3 の C# 環境](https://docs.unity3d.com/6000.3/Documentation/Manual/csharp-compiler.html) | Mono / IL2CPP の Boehm GC |
| [Unity 7 の CoreCLR](https://docs.unity3d.com/7000.0/Documentation/Manual/scripting-backends-coreclr.html) | CoreCLR バックエンドとビルド設定 |
| [Profiler のメモリーカウンター](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/profiler/counters-reference) | GC Allocated In Frame の managed 割り当てバイト数 |
| [Profiler.GetMonoUsedSizeLong](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Profiling.Profiler.GetMonoUsedSizeLong.html) | 生存中と未回収の managed メモリー量 |
| [Profiler.GetTotalAllocatedMemoryLong](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Profiling.Profiler.GetTotalAllocatedMemoryLong.html) | Unity 内部アロケーターの使用量 |
| [ProfilerRecorder の CurrentValue](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.CurrentValue.html) | 同期的なカウンター・マーカー観測 |
| [ProfilerRecorderOptions](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorderOptions.html) | スレッドの限定とフレーム合算の制御 |

IL2CPP のヘッダーは、各 Editor のインストール先を基準にした次のファイルを確認した。対象は 6000.3.28f1 と 7000.0.0a12。

```text
Editor/Data/il2cpp/libil2cpp/il2cpp-object-internals.h
```

`Il2CppObject` の先頭は `klass` または `vtable` の union と `monitor`。Windows x64 では計 16 B。生成された C++ は各プロジェクトの `Library/Bee/artifacts/WinPlayerBuildProgram/il2cppOutput/cpp/` にあり、[生成コードの抜粋](../results/codegen)にも抜粋を保存している。

同じ `libil2cpp` 内の `il2cpp-api-functions.h` と `il2cpp-api.cpp` に `il2cpp_class_from_type(const Il2CppType*)` が宣言・実装されている。実装は `vm/Class.cpp` の `Class::FromIl2CppType` を呼び、通常の値型と generic instance を解決する。`ldtoken` から変換関数までの生成コードは [Unity 6.3](../results/codegen/unity63-typehandle.cpp) と [Unity 7](../results/codegen/unity7-typehandle.cpp) の抜粋に保存した。

IL2CPP のカウンター実装は、Editor 付属の `libil2cpp/gc/BoehmGC.cpp`、`icalls/mscorlib/System/GC.cpp`、Unity 7 の `icalls_monoaot/System.Private.CoreLib/System/GC.cpp` を確認した。
