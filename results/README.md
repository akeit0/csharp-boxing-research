# 測定レポートと生成コード

2026 年 11 月 5～6 日（JST）、Windows x64 で取得した測定レポートと生成コードの抜粋です。

| 場所 | 内容 |
| --- | --- |
| [allocation-bytes](allocation-bytes) | 全環境の割り当てバイト数、StackBox、Unity の計測 API 比較 |
| [dotnet](dotnet) | .NET 10 / 11 RC CoreCLR の型制約、不適合型の呼び出し、IL |
| [nativeaot](nativeaot) | .NET 10 / 11 RC Native AOT の型制約とハンドル取得方法の比較 |
| [calls-dotnet](calls-dotnet) | .NET 10 の既定実装による参照の保持と、raw バッファによるスタック配置 |
| [calls-unity](calls-unity) | Unity 各バックエンドの型制約と raw バッファによるスタック配置 |
| [codegen](codegen) | IL、JIT が生成した機械語、IL2CPP が生成した C++ |

各測定フォルダの `run.json` は、取得日時と使用した共通 DLL の SHA-256 を記録しています。`compiler.json` は Native AOT の SDK・ターゲット・発行設定です。`codegen` 内の `typehandle-*-run.json` はハンドル直接取得時、`typeof-unity-run.json` は `typeof(T).TypeHandle` 経由の比較時の記録です。

`bytes=-1` / `events=-1` は、そのカウンターを使用しなかったことを表します。`events` は `GC.Alloc` のイベント数であり、`allocation-bytes` ではバイト数も測定しています。[計測条件](../docs/experiments.md#計測条件)

ログ内の `DIM` はインターフェイスの既定実装を指す識別子です。`seedBox` は型ヘッダー取得用のボックスを作ったかどうかを表します。

実行時の生ログとビルド出力は `artifacts/runs/`、Unity Player は `builds/` に生成されます。どちらも Git の対象外です。
