# 実験の構成と再実行方法

## 共通コードとホスト

`Research.IL` と `Research.Core` は .NET SDK でビルドする `netstandard2.1` ライブラリ。.NET と Unity は同じ DLL を使用する。

| 場所 | 役割 |
| --- | --- |
| [Implementations](../shared/Research.Core/Implementations) | `StackBox<T>`、`CachedBox<T>`、C# の constrained / pattern 呼び出し、Reflection delegate のキャッシュ |
| [Experiments](../shared/Research.Core/Experiments) | 比較対象の値型、実験ケース、GC 観測用コード |
| [IlCalls.cs](../shared/Research.IL/IlCalls.cs) | 型制約なしの IL 呼び出し、型判定付きの入口、ldtoken による TypeHandle の取得 |
| [RuntimeTypeMetadata.cs](../shared/Research.Core/Implementations/RuntimeTypeMetadata.cs) | Windows Player の Mono / IL2CPP が公開する型情報変換 API |
| [IlCalls.ConstraintExperiments.cs](../shared/Research.IL/IlCalls.ConstraintExperiments.cs) | 制約付きターゲットと制約の証明を省いた呼び出し |
| [ExperimentRunner](../shared/Research.Core/ExperimentRunner.cs) | モードから実験ケースへの振り分け |
| [ExperimentContext](../shared/Research.Core/ExperimentContext.cs) | バックエンド、ログ出力、割り当て計測 |
| [dotnet/Experiments](../dotnet/Experiments) | .NET の起動、埋め込み後の IL の表示、不適合型を使う呼び出し |
| [unity/ResearchPlayer.cs](../unity/ResearchPlayer.cs) | Unity の起動、バックエンド表示、ProfilerRecorder による計測 |
| [unity/ResearchEditor.cs](../unity/ResearchEditor.cs) | 空の実験シーンの生成と Windows Player のビルド |

`Sync-Unity.ps1` は共通 DLL とアダプターを Unity プロジェクトへ配置する。Unity 6.3 には Unsafe の互換 DLL も配置する。

## 使用環境

測定日: 2026 年 11 月 5～6 日（JST）。Windows x64。

| 環境 | バージョン |
| --- | --- |
| .NET SDK | 10.0.403。global.json で固定 |
| .NET 10 | 10.0.14 |
| .NET 11 RC | 11.0.0-rc.3.26522.148。JIT は net10.0 のホストを --fx-version で実行 |
| Native AOT | .NET 10.0.14 / 11.0.0-rc.3.26522.148。SDK 10.0.403 / 11.0.100-rc.3.26522.148、Release、win-x64 |
| Unity 6.3 | 6000.3.28f1。Editor Mono、Windows Mono / IL2CPP Player |
| Unity 7 alpha | 7000.0.0a12。Editor CoreCLR、Windows CoreCLR / IL2CPP Player |
| Unity CoreCLR の FrameworkDescription | .NET 10.0.10-dev |
| Unity CLI | 1.0.0-beta.16 |
| IL 埋め込み | InlineIL.Fody 1.10.2、Fody 6.9.3 |
| Unity 6.3 の Unsafe | System.Runtime.CompilerServices.Unsafe 6.1.2 の netstandard2.0 DLL |
| スクリプト | PowerShell 7 |

Unity 6.3 の Windows IL2CPP ビルドには `windows-il2cpp` モジュールを使用する。Unity 7 は組み込みの Unsafe を参照する。

Unity 7 IL2CPP の FrameworkDescription は CoreCLR と同じ文字列になる。Unity 6.3 IL2CPP には Mono.Runtime 型も存在する。バックエンドは Player のビルド設定と `ENABLE_IL2CPP` / `ENABLE_CORECLR` から記録する。

## 実験モード

| モード | 比較内容 | 実行対象 |
| --- | --- | --- |
| safe | Dispose 各方式の割り当て、値型・参照型・interface、例外時の変更 | 全環境 |
| allocation-counters | 累積割り当て量・使用中ヒープ量・Profiler のバイトカウンターを比較 | Unity |
| dim-receiver-escape | インターフェイスの既定実装が保存した this とボックス再利用。CoreCLR ではスタック上のボックスも比較 | 全環境 |
| forge | 実在するボックスの型情報を使う手動スタック配置 | 全環境 |
| forge-gc | 上記に GC と呼び出しを追加 | 全環境 |
| forge-typehandle | TypeHandle からヘッダーを構築し、型ヘッダー取得用のボックスを省く | CoreCLR x64 |
| stack-box | `StackBox<T>` の初回と反復時の割り当て、型引数を指定したジェネリック型、参照を含む構造体の GC。CoreCLR では移動も比較 | 全環境 |
| constraint-only | 定数を返す制約付きメソッドを T=int で呼ぶ | 全環境 |
| metadata-constraint | 制約付き Dispose を Reflection で T=int にして呼ぶ | 全環境 |
| invalid-il | 制約なし `Dispose<int>` を直接呼ぶ | .NET ホスト |
| invalid-constraint | 制約付きメソッドへ int を渡す IL 呼び出し | .NET ホスト |
| dump-il | IL バイト列、メタデータ制約、参照アセンブリの表示 | .NET ホスト |
| forge-reference-root | raw バッファと実在するボックスの参照保持の対照実験 | CoreCLR x64、個別指定 |

モード名は結果ファイルの識別子にも使う。標準の実行対象は `scripts/Common.ps1` で定義している。`dim-receiver-escape` は個別指定で実行する。

## 計測条件

割り当ての比較は 1,000 回ウォームアップ後に 100,000 回実行する。delegate の作成、観測用配列・WeakReference の生成、ログ出力は測定区間の外。

.NET は子プロセスごとに `DOTNET_TieredCompilation=0` を指定する。Unity は Development Player と既定のランタイム設定を使う。

`AllocationMeter` は、static フィールドに保存する `new object()` でカウンターの応答を確認する。CoreCLR / Native AOT は `GC.GetAllocatedBytesForCurrentThread()`、Unity Mono / IL2CPP は `ProfilerRecorder` の `GC Allocated In Frame.CurrentValue` の差分を使用する。比較用の object 1 個は前者が 24 B、後者が 16 B だった。

`GC Allocated In Frame` はフレーム単位・全スレッドのバイト数なので、読み取り前後を同一フレーム内で実行する。`CurrentValue` を読むため、フレーム終了を待たずに差分を取れる。カウンターは測定区間の前に開始する。`GC.Alloc` のイベント数も補助的に記録し、現在スレッドに限定、フレーム合算を無効にしてサンプル数差を取っている。

`allocation-counters` はカウンターごとに測定区間を分ける。読み取りをウォームアップし、no-op、object 1 個、配列に保持する object 100,000 個、参照の解除と GC を比較する。配列・delegate は事前に作成し、各区間の後でログを出力する。[結果と各 API の違い](findings.md#割り当てカウンターの比較)。

ボックスのキャッシュはウォームアップ後を測定する。`forge` / `forge-gc` は測定前に 型ヘッダー取得用のボックスを生成する。`forge-typehandle` と `StackBox<T>` は TypeHandle を使用する。Mono / IL2CPP の `StackBox<T>` は ldtoken から直接得たハンドルをネイティブ API で変換し、型ヘッダーをキャッシュする。型ヘッダー取得用のボックス化は行わない。

`stack-box` はヘッダーキャッシュ未初期化の Counter / Payload / `GenericCounter<object>` を各 1 回計測し、その後に反復計測を行う。初回計測は ProfilerRecorder を起動してから 1 回だけ呼び、同じ呼び出しのバイト数とイベント数を取る。反復計測には毎回のスタック上の格納領域の初期化、ヘッダー構築、interface 呼び出し、読み戻しを含む。delegate と参照先のオブジェクト生成は区間外。

GC の実験は、参照を含む構造体を配置し、Dispose 後に GC を 1 回実行する 1 ケースにしている。対象配列の手前に解放可能な配列を配置して compaction を促し、弱参照と数値アドレスで生存・移動・参照スロットの一致を確認する。Dispose 後に `storage` やボックスを managed な参照として使うコードはない。実装は `GcProbe` と `StackGcExperiments.ReferenceGc` にある。

## 再実行

```powershell
# .NET 10 と 11 RC（JIT）
./scripts/Run-DotNet.ps1 -IncludePreview

# .NET 10 の呼び出し方式と StackBox
./scripts/Run-DotNet.ps1 -Modes safe,stack-box

# Native AOT、.NET 10 と 11 RC
./scripts/Run-NativeAot.ps1 -IncludePreview

# 全 Unity Player のビルドと実行
./scripts/Run-Unity.ps1

# Unity の計測 API、呼び出し方式、StackBox をバイト数で比較
./scripts/Run-Unity.ps1 -Modes allocation-counters,safe,stack-box

# 特定の Player と実験
./scripts/Run-Unity.ps1 -Names unity7-coreclr -Modes stack-box

# 既存 Player の実行
./scripts/Run-Player.ps1 -Name unity7-coreclr -Modes stack-box

# 共通 DLL の配置のみ
./scripts/Sync-Unity.ps1

# ホストを直接起動
dotnet run --project dotnet/Experiments -c Release -f net10.0 -- dump-il
```

`-OutputDirectory` で出力先を指定できる。Player は 60 秒、Editor によるビルドは 900 秒を上限とする。結果ファイルには終了コードも保存する。

`metadata-constraint` の IL2CPP 実行ではアクセス違反 `0xC0000005` が観測される。このモードは終了コードも結果として扱う。それ以外の異常終了とタイムアウトではスクリプトを停止する。

## 結果の保存

実行時のレポートとビルド・Player の生ログは、`artifacts/runs/<UTC時刻>-dotnet`、`<UTC時刻>-unity`、`<UTC時刻>-nativeaot` に保存する。このディレクトリは Git の対象外。`run.json` は実行日時と共通 DLL の SHA-256 を含む。

収録した測定レポートは [results](../results/README.md) に、呼び出し方式・StackBox・生成コードごとにまとめている。

## Native AOT

.NET ホストを `NativeAot=true` で発行すると、そのプロジェクトだけに `PublishAot=true` を設定する。共通ライブラリは `netstandard2.1` のまま使用する。`Run-NativeAot.ps1` は対象ごとの SDK を実行結果フォルダの `global.json` で選び、`ExperimentFramework` を `net10.0` / `net11.0` に切り替える。ワークスペースの SDK 指定は変更しない。

既定の実験は `safe`、`stack-box`、`constraint-only`、`metadata-constraint`、`invalid-il`、`invalid-constraint`。`dump-il` は Native AOT ではスキップする。発行時に出る IL2026 は、JIT ホスト用の IL 表示コード（`GetMethodBody` など）に対する警告である。

未実装型の直接呼び出しと制約違反の直接呼び出しは、別々の非インラインメソッドに分離している。各モードを別プロセスで起動し、標準出力・標準エラーと終了コードを保存する。Native AOT の両バージョンでは、これらの直接呼び出しが `0xC0000602`（符号付き終了コード -1073740286）で終了した。

`execution=NativeAOT; dynamicCodeCompiled=False` をホストから記録する。`stack-box; runtime=CoreClr` は `StackBoxRuntime.CoreClr` のレイアウト選択を表す。Native AOT でも同じ Windows x64 のヘッダー配置を使っている。

[Native AOT の結果](../results/nativeaot)、[公式のビルド要件と発行方法](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)。
