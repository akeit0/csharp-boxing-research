# C# のボックス化と Dispose の調査結果

2026 年 11 月 5～6 日（JST）。Windows x64、.NET 10 / 11 RC（CoreCLR / Native AOT）、Unity 6000.3.28f1、Unity 7000.0.0a12。

型制約なしの `constrained.` 呼び出しと `StackBox<T>` は、全環境で反復時の割り当てなしに動作した。`StackBox<T>` は参照を含む構造体でも使用でき、CoreCLR では GC による参照先の移動にも追従した。

## 結果の概要

| 項目 | 結果 |
| --- | --- |
| 型制約なしの Dispose | constrained. !!T と callvirt による直接呼び出しが全環境で成立 |
| ボックスの手動スタック配置 | 共通の `StackBox<T>` が CoreCLR / Native AOT / Mono / IL2CPP で動作。全ランタイムで TypeHandle から型ヘッダーを取得し、型ヘッダー取得用のボックスは不要 |
| 参照フィールドと GC | 全環境で参照先が生存。CoreCLR では移動時に `StackBox<T>.Value` の同じ参照スロットが更新された |
| インターフェイスの既定実装による参照の保持 | 保存した `this` が、クリア後や再利用中のボックスの値を観測した |
| メタデータ上のジェネリック制約 | Mono / IL2CPP は定数を返すメソッドを制約違反の型引数で実行。CoreCLR は拒否 |

比較対象のキャッシュ方式は、[元記事](https://qiita.com/Omochaya/items/c396ca2910e1c5e90e92)の copy-in / Dispose / copy-out を実装している。

## Dispose 方式ごとの割り当て

1,000 回のウォームアップ後に 100,000 回実行した。表のバイト数は各カウンターが報告した 1 呼び出し当たりの値。

| 呼び方 | .NET 10 / 11 RC | Unity 7 CoreCLR | Unity 6.3 Mono | Unity 6.3 / 7 IL2CPP |
| --- | --- | --- | --- | --- |
| ボックスを object に保持して Dispose | 24 B | 24 B | 20 B | 20 B |
| is IDisposable | 24 B | 24 B | 20 B | 20 B |
| C# の制約付き呼び出し、型自身が実装 | 0 B | 0 B | 0 B | 0 B |
| 既定実装の Capture を object に保持 | 24 B | 24 B | 20 B | 20 B |
| 既定実装に任せた既定 Dispose | 24 B | 24 B | 20 B | 20 B |
| ボックスのキャッシュを再利用 | 0 B | 0 B | 0 B | 0 B |
| 制約なし IL | 0 B | 0 B | 0 B | 0 B |
| 型判定をキャッシュして制約なし IL を呼ぶ | 0 B | 0 B | 0 B | 0 B |
| `StackBox<Counter>` の構築と interface 呼び出し | 0 B | 0 B | 0 B | 0 B |
| 参照を含む `StackBox<Payload>` の構築と interface 呼び出し | 0 B | 0 B | 0 B | 0 B |
| Reflection delegate をキャッシュ | 0 B | 0 B | 0 B | — |

CoreCLR / Native AOT は `GC.GetAllocatedBytesForCurrentThread()`、Unity Mono / IL2CPP は `ProfilerRecorder` の `GC Allocated In Frame.CurrentValue` で測定した。後者は同一フレーム内の全スレッドの割り当てを含む。[計測条件](experiments.md#計測条件)、[実測ログ](../results/allocation-bytes)。

[実験コード](../shared/Research.Core/Experiments/DisposeExperiments.cs)、[NET 10](../results/allocation-bytes/dotnet/dotnet-net10.0-safe.txt)、[Mono](../results/allocation-bytes/unity/unity63-mono-safe.txt)、[CoreCLR Player](../results/allocation-bytes/unity/unity7-coreclr-safe.txt)、[IL2CPP](../results/allocation-bytes/unity/unity63-il2cpp-safe.txt)。

## 割り当てカウンターの比較

Unity の各 Windows Development Player で API を比較した。カウンターの読み取りは事前にウォームアップし、各 API の区間を分離している。

| API・カウンター | Unity 6.3 Mono | Unity 6.3 IL2CPP | Unity 7 CoreCLR | Unity 7 IL2CPP |
| --- | --- | --- | --- | --- |
| `GC.GetAllocatedBytesForCurrentThread()`、object 1 個の差分 | 0 B | 0 B | 24 B | 0 B |
| `GC.GetTotalAllocatedBytes(true)`、object 1 個の差分 | API なし | API なし | 24 B | 0 B |
| `GC.GetTotalAllocatedBytes(false)`、object 1 個の差分 | API なし | API なし | 0 B | 0 B |
| `GC.GetTotalMemory(false)`、object 1 個の差分 | 0 B | 0 B | 0 B | 0 B |
| `Profiler.GetMonoUsedSizeLong()`、object 1 個の差分 | 0 B | 0 B | 0 B | 0 B |
| `Profiler.GetTotalAllocatedMemoryLong()`、object 1 個の差分 | 0 B | 0 B | 0 B | 0 B |
| `GC Allocated In Frame.CurrentValue`、object 1 個の差分 | 16 B | 16 B | 24 B | 16 B |
| 同上、object 100,000 個の差分 | 1,600,000 B | 1,600,000 B | 2,400,000 B | 1,600,000 B |
| 同上、参照解除と GC の区間の差分 | 0 B | 0 B | 0 B | 0 B |

`GC.GetTotalMemory(false)` と `Profiler.GetMonoUsedSizeLong()` は大量の割り当てで増え、回収すると減った。これは累積の割り当て量ではなく、使用中のヒープ量である。IL2CPP の `gc/BoehmGC.cpp` も `GC_get_heap_size() - GC_get_free_bytes()` を返す。object 1 個の計測では変化を検出しなかった。

Unity 7 IL2CPP の `icalls_monoaot/System.Private.CoreLib/System/GC.cpp` は `GetAllocatedBytesForCurrentThread()` と `GetTotalAllocatedBytes(bool)` がどちらも `return 0;`。Unity 6.3 の mscorlib には `GetTotalAllocatedBytes` がなかった。`Profiler.GetTotalAllocatedMemoryLong()` は Unity 内部のアロケーターが使う量を返す API で、今回の managed object の割り当ては検出しなかった。

`GC Allocated In Frame` は全 4 環境で単位が Bytes、no-op の差分は 0、object 1 個と 100,000 個を検出し、GC によって値が減らなかった。`CurrentValue` は即時に読み取れるため、同一フレーム内の短い区間の割り当てバイト数に使用できた。フレーム全体・全スレッドの値なので、前後の読み取りでフレームをまたがない形にしている。

[比較コード](../unity/ResearchPlayer.cs)、[各バックエンドのログ](../results/allocation-bytes/unity)、[Profiler のカウンター一覧](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/profiler/counters-reference)、[CurrentValue](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.CurrentValue.html)。

## 型制約なし IL の直接呼び出し

[IlCalls.Dispose\<T\>](../shared/Research.IL/IlCalls.cs) は次の 4 命令になる。

```il
.method public static void Dispose<T>(!!T& value) cil managed
{
    ldarg.0
    constrained. !!T
    callvirt instance void System.IDisposable::Dispose()
    ret
}
```

GenericParamConstraint は 0 個。明示的 interface 実装、参照型、interface 型の変数、参照型フィールドを持つ構造体でも呼び出しが成立した。Dispose による変更は元の変数に反映された。

.NET 10 の機械語では Counter.Dispose への直接 call になり、既定実装の Capture / 既定 Dispose は CORINFO_HELP_NEWSFAST を呼んだ。IL2CPP の C++ も、Counter* をそのまま渡す直接 call と、既定実装の Box(...) に分かれた。

[IL と制約](../results/codegen/dotnet-net10.0-dump-il.txt)、[JIT 出力](../results/codegen/dispose-jit.asm)、[Unity 6.3 の C++](../results/codegen/unity63-dispose.cpp)、[Unity 7 の C++](../results/codegen/unity7-dispose.cpp)。

`TryDispose<T>` は IDisposable の実装有無を型ごとにキャッシュする。int は no-op。CoreCLR の `Dispose<int>` の直接呼び出しは EntryPointNotFoundException になった。`Nullable<Counter>` の型判定も false だった。[不適合型の呼び出し](../results/dotnet/dotnet-net10.0-invalid-il.txt)

## Native AOT の結果

.NET 10.0.14 / 11.0.0-rc.3.26522.148 の Native AOT でも、共通ライブラリの型制約なし IL 呼び出しと `TryDispose<T>` が動作した。ウォームアップ後はいずれも 0 B。`StackBox<T>` には CoreCLR と同じヘッダー配置を使用し、`typeof(T).TypeHandle.Value` から型情報を取得できた。

| 測定・操作 | Native AOT .NET 10 | Native AOT .NET 11 RC |
| --- | --- | --- |
| 型制約なし IL / TryDispose の反復 | 0 B | 0 B |
| StackBox の初回（Counter / Payload / `GenericCounter<object>`） | 各 0 B | 各 32 B |
| StackBox の構築・呼び出し・読み戻しの反復 | 0 B | 0 B |
| GC 後の参照先の生存・移動・スロット更新 | すべて確認 | すべて確認 |
| 無制約の外側 T から制約付き Dispose を Counter で呼ぶ | 成功 | 成功 |
| IDisposable 制約付きの定数メソッドを int で IL 呼び出し | 123 | 123 |
| 同じ定数メソッドを Reflection で int にする | ArgumentException | ArgumentException |
| 制約付き Dispose の MakeGenericMethod(int) | ArgumentException | ArgumentException |
| 制約なし Dispose<int> の直接呼び出し | プロセス終了 0xC0000602 | プロセス終了 0xC0000602 |
| 制約付き Dispose<int> の IL による直接呼び出し | プロセス終了 0xC0000602 | プロセス終了 0xC0000602 |

Native AOT はメタデータ上の制約を持つメソッドでも、IL 経由の呼び出しを受理するケースがあった。同じ型引数を Reflection で指定した場合は制約チェックで拒否した。未実装型を直接呼ぶ実験は `try/catch` のあるホストから起動したが、例外をログする前にプロセスが終了した。終了コードは [STATUS_FAIL_FAST_EXCEPTION](https://learn.microsoft.com/ja-jp/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55) に相当する。

11 RC の初回 32 B は、`StackBox.AsObject` の CoreClr 分岐を `IlCalls.TypeHandle<T>().Value` に変更した [ldtoken 直接取得の比較](../results/nativeaot/ldtoken-net11.0/nativeaot-net11.0-stack-box.txt)でも同じだった。割り当て元の特定はしていない。

[.NET 10 のログと SDK 記録](../results/nativeaot/net10.0)、[.NET 11 RC のログと SDK 記録](../results/nativeaot/net11.0)、[CoreCLR の再測定](../results/dotnet)。

## ジェネリック制約のランタイム差

制約付きのターゲットと、外側の型制約を省いた IL 呼び出しを比較した。[実装](../shared/Research.IL/IlCalls.ConstraintExperiments.cs)

| 操作 | CoreCLR（.NET / Unity 7） | Unity 6.3 Mono | Unity 6.3 / 7 IL2CPP |
| --- | --- | --- | --- |
| `ConstrainedMetadata<Counter>` を通常呼び出し | 成功 | 成功 | 成功 |
| 外側の無制約 T を制約付きメソッドへ渡し、Counter で実行 | VerificationException | 成功 | 成功 |
| IDisposable 制約付きで定数 123 を返すメソッドを、int で IL 呼び出し | VerificationException | 123 | 123 |
| 同じ定数メソッドを int で Reflection 呼び出し | ArgumentException | ArgumentException | 123 |
| 制約付き Dispose の MakeGenericMethod(int) | ArgumentException | ArgumentException | MethodInfo の作成を受理 |
| 作成した `Dispose<int>` の MethodInfo を Invoke | 作成時に拒否 | 作成時に拒否 | アクセス違反 0xC0000005 |

CoreCLR は、外側 T に制約の証明がない呼び出しを Counter でも拒否した。Mono は IL と Reflection で結果が分かれ、IL2CPP は定数メソッドを両経路で実行した。

[NET 10](../results/dotnet/dotnet-net10.0-constraint-only.txt)、[Mono](../results/calls-unity/unity63-mono-constraint-only.txt)、[IL2CPP の定数メソッド](../results/calls-unity/unity63-il2cpp-constraint-only.txt)、[IL2CPP の Dispose](../results/calls-unity/unity63-il2cpp-metadata-constraint.txt)。

## インターフェイスの既定実装からボックスへの参照を保存できる

インターフェイスの既定実装内の `this` は、呼び出し先のボックス自身を指す。[CachedBox\<T\>](../shared/Research.Core/Implementations/CachedBox.cs) のようにボックスをプールする方式でも、その参照を保存するとクリア後や再利用中の内容を読める。スタック上に配置したボックスへの参照も保存できる。このような実装は通常ほぼなく、避けるべきもの。[確認コード](../shared/Research.Core/Experiments/DimReceiverEscape.cs)、[確認結果](../results/calls-dotnet/dotnet-net10.0-dim-receiver-escape.txt)。

## ボックスの手動スタック配置

オブジェクトヘッダーと値型のペイロードをスタック上に配置し、box への参照として使用する。CoreCLR の資料にも [stack-allocated boxes](https://github.com/dotnet/runtime/issues/104936) の呼称がある。

| ランタイム | object 参照の先頭 | ペイロードのオフセット | 前置領域 |
| --- | --- | --- | --- |
| CoreCLR x64 | MethodTable* | +8 B | -8 B に同期用ヘッダー |
| Unity Mono x64 | vtable*、同期用ポインター | +16 B | なし |
| IL2CPP x64 | klass/vtable*、monitor* | +16 B | なし |

実在する Counter のボックスから型情報の先頭ワードとペイロードのオフセットを取得し、同期情報をゼロにしたヘッダーを作成した。[実験コード](../shared/Research.Core/Experiments/RawStackBoxExperiment.cs)

全環境で Count=41 から最初の Dispose で 42、追加 100,000 回後に 100042、GC 後のもう 1 回で 100043 になった。CoreCLR の呼び出しループは割り当て 0 B。TypeHandle.Value を使う構成では 型ヘッダー取得用のボックスも省けた。

[NET 10](../results/calls-dotnet/dotnet-net10.0-forge-gc.txt)、[Mono](../results/calls-unity/unity63-mono-forge-gc.txt)、[CoreCLR Player](../results/calls-unity/unity7-coreclr-forge-gc.txt)、[IL2CPP](../results/calls-unity/unity7-il2cpp-forge-gc.txt)、[TypeHandle から取得](../results/calls-dotnet/dotnet-net10.0-forge-typehandle.txt)。

## 共通の `StackBox<T>` とランタイム別ヘッダー

全ランタイムで同じ Sequential なジェネリック構造体 を使用する。

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct StackBox<T> where T : struct
{
    public IntPtr Prefix0;
    public IntPtr Prefix1;
    public T Value;
}
```

`StackBox.AsObject(ref storage, runtime)` は `Value` の実際の位置から直前の 2 ワードにヘッダーを書く。CoreCLR では同期用ワード・MethodTable の順、Mono / IL2CPP では vtable または klass・monitor の順に配置する。返す参照はそれぞれ `Value - 8 B`、`Value - 16 B` になる。

型ヘッダーは全ランタイムで TypeHandle から取得する。Mono / IL2CPP は閉じた型 `T` ごとに結果をキャッシュし、初回も `T` をボックス化しない。

| ランタイム | 型ヘッダーの取得経路 |
| --- | --- |
| CoreCLR | `typeof(T).TypeHandle.Value` が MethodTable を指す |
| Mono | `MonoType*` → `mono_class_from_mono_type` → `MonoClass*` → `mono_class_vtable(mono_domain_get(), klass)` → `MonoVTable*` |
| IL2CPP | `Il2CppType*` → `il2cpp_class_from_type` → `Il2CppClass*` |

Mono の vtable は domain ごとのものを取得する。Windows Player では `mono-2.0-bdwgc.dll`、IL2CPP は `GameAssembly.dll` に公開されている変換関数を P/Invoke で呼ぶ。[共通実装](../shared/Research.Core/Implementations/StackBox.cs)、[ネイティブ関数の宣言](../shared/Research.Core/Implementations/RuntimeTypeMetadata.cs)、[Mono の API](https://github.com/Unity-Technologies/mono/blob/unity-6000.3-mbe/mono/metadata/class.h)。

Mono / IL2CPP のハンドル取得には、`IlCalls.TypeHandle<T>()` で `ldtoken` を直接返す。`System.Type` の取得を経由しない。

```il
.method public static valuetype System.RuntimeTypeHandle TypeHandle<T>() cil managed
{
    ldtoken !!T
    ret
}
```

Unity 6.3 IL2CPP では `typeof(T).TypeHandle` を使った初回の Counter / Payload が各 1 割り当てイベント、直接の `ldtoken` では各 0 イベントだった。[typeof 経由の測定](../results/codegen/unity63-typeof-first-call.txt)、[IL](../results/codegen/dotnet-net10.0-dump-il.txt)、[Unity 6.3 の生成 C++](../results/codegen/unity63-typehandle.cpp)、[Unity 7 の生成 C++](../results/codegen/unity7-typehandle.cpp)。

CoreCLR はジェネリック型の Explicit layout を受け付けないため Sequential にしている。[Explicit layout の issue](https://github.com/dotnet/runtime/issues/97526)

```csharp
StackBox<Counter> storage = default;
storage.Value = value;
object receiver = StackBox.AsObject(ref storage, StackBoxRuntime.Mono);
((IDisposable)receiver).Dispose();
value = storage.Value;
```

`runtime` は `CoreClr` / `Mono` / `Il2Cpp` から選ぶ。省略時は `CoreClr`。実験ホストはビルド設定から選択する。

| Player | ペイロードのオフセット | 整数型の構築・呼び出し 100,000 回 | 参照を含む型の構築・呼び出し 100,000 回 |
| --- | --- | --- | --- |
| Unity 6000.3.28f1 Mono | +16 B | 0 B | 0 B |
| Unity 6000.3.28f1 IL2CPP | +16 B | 0 B | 0 B |
| Unity 7000.0.0a12 CoreCLR | +8 B | 0 B | 0 B |
| Unity 7000.0.0a12 IL2CPP | +16 B | 0 B | 0 B |

測定区間にはローカルの `StackBox<T>` の初期化、ヘッダーの構築、interface 呼び出し、変更の読み戻しを含む。型ヘッダーのキャッシュ初期化は区間外。整数型は Count=41 から 42 になり、参照を含む型も参照と変更後の Count を読み戻せた。型引数を指定したジェネリック型の `GenericCounter<object>` も、参照を保ったまま Count が 42 になった。.NET 10 / 11 RC も両反復ケース 0 B。

ヘッダーのキャッシュが未初期化の最初の呼び出しも、型ごとに 1 回計測した。delegate・計測器・参照先オブジェクトの作成は区間外。

| 環境 | Counter の初回 | Payload の初回 | `GenericCounter<object>` の初回 |
| --- | --- | --- | --- |
| Unity 6.3 Mono | 240 B | 144 B | 144 B |
| Unity 6.3 IL2CPP | 0 B | 0 B | 0 B |
| Unity 7 CoreCLR | 0 B | 0 B | 0 B |
| Unity 7 IL2CPP | 0 B | 0 B | 0 B |
| .NET 10 / 11 RC CoreCLR | 0 B | 0 B | 0 B |
| .NET 10 Native AOT | 0 B | 0 B | 0 B |
| .NET 11 RC Native AOT | 32 B | 32 B | 32 B |

Mono の初回計測にはキャッシュ用のジェネリック型と vtable の初期化も含む。[Unity Mono の vtable 作成コード](https://github.com/Unity-Technologies/mono/blob/unity-6000.3-mbe/mono/metadata/object.c#L2124)は `MonoReflectionType` を取得して vtable に保持する。型ヘッダー取得用のボックス化を省いた後も、初回全体の割り当ては残った。

[実験コード](../shared/Research.Core/Experiments/StackGcExperiments.cs)、[Mono](../results/allocation-bytes/unity/unity63-mono-stack-box.txt)、[Unity 6.3 IL2CPP](../results/allocation-bytes/unity/unity63-il2cpp-stack-box.txt)、[Unity 7 CoreCLR](../results/allocation-bytes/unity/unity7-coreclr-stack-box.txt)、[Unity 7 IL2CPP](../results/allocation-bytes/unity/unity7-il2cpp-stack-box.txt)、[NET 10](../results/allocation-bytes/dotnet/dotnet-net10.0-stack-box.txt)。

## `StackBox<T>` の参照フィールドと GC

参照フィールドを持つ Payload を T に指定し、Dispose 後に GC を 1 回実行した。GC 後は弱参照と数値アドレスから、対象の生存とペイロード内のスロットを確認する。

| 環境 | 対象の生存 | 実際に移動 | スロットと対象のアドレスが一致 |
| --- | --- | --- | --- |
| .NET 10 / 11 RC CoreCLR / Native AOT | true | true | true |
| Unity 7 CoreCLR | true | true | true |
| Unity 6.3 Mono | true | false | true |
| Unity 6.3 / 7 IL2CPP | true | false | true |

[実験コード](../shared/Research.Core/Experiments/StackGcExperiments.cs)、[NET 10](../results/allocation-bytes/dotnet/dotnet-net10.0-stack-box.txt)、[Unity 7 CoreCLR](../results/allocation-bytes/unity/unity7-coreclr-stack-box.txt)。

CoreCLR v10.0.0 は、[address-exposed なローカルを lvTracked=0 にし](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/jit/lclvars.cpp#L2816)、[フレーム上の struct の GC フィールドを GC_SLOT_UNTRACKED として登録する](https://github.com/dotnet/runtime/blob/v10.0.0/src/coreclr/jit/gcencode.cpp#L3917)。このスロットは [メソッド全体で live として報告される](https://github.com/dotnet/runtime/blob/v10.0.0/docs/design/coreclr/jit/ryujit-overview.md#gc-info)。

## 例外時の copy-out

ThrowingCounter.Dispose は Count を 1 増やしてから例外を投げる。

| 呼び方 | catch 後の元の Count |
| --- | --- |
| 制約なし IL の直接呼び出し | 1 |
| copy-out を成功時だけ行うキャッシュ方式 | 0 |
| copy-out を finally に移したキャッシュ方式 | 1 |

copy-out を finally に移すと例外時の Count も直接呼び出しと一致した。[実装](../shared/Research.Core/Implementations/CachedBox.cs)、[実験コード](../shared/Research.Core/Experiments/DisposeExperiments.cs)、[NET 10](../results/allocation-bytes/dotnet/dotnet-net10.0-safe.txt)
