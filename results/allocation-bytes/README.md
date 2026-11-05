# 割り当てバイト数の測定

2026 年 11 月 6 日（JST）、Windows x64。Unity の全 4 バックエンドで `allocation-counters`、`safe`、`stack-box` を実行した。.NET 10 / 11 RC の CoreCLR と Native AOT でも、共通の計測コードを使って `safe` / `stack-box` を実行した。

| 場所 | 内容 |
| --- | --- |
| [unity](unity) | Unity の API 比較と、Profiler のバイトカウンターを使った呼び出し方式・StackBox の測定 |
| [dotnet](dotnet) | .NET 10 / 11 RC CoreCLR の測定 |
| [nativeaot](nativeaot) | .NET 10 / 11 RC Native AOT の測定と発行設定 |

Unity Mono / IL2CPP のバイトカウンターは `ProfilerRecorder` の `GC Allocated In Frame.CurrentValue`、CoreCLR / Native AOT は `GC.GetAllocatedBytesForCurrentThread()`。Unity の値は全スレッドを含み、同一フレーム内で前後の差分を取った。各カウンターが報告する割り当てサイズであり、ヒープの使用量の差分とは異なる。

`allocation-counters` は API ごとに別々の区間を測る。読み取りをウォームアップし、何もしない処理、object 1 個、保持する object 100,000 個、参照の解除と GC を比較する。保持先の配列・delegate・ログ出力は計測区間外。`total-allocated=unavailable` は API が存在しないことを表す。`total-allocated-precise` / `approximate` は `GC.GetTotalAllocatedBytes(true / false)`。

`safe` / `stack-box` の `bytes` は区間全体、`bytesPerCall` は 1 回当たりの値。`events` は補助的に残した `GC.Alloc` のイベント数。`run.json` に共通 DLL の SHA-256 を記録している。

[計測条件](../../docs/experiments.md#計測条件)、[API の比較と結果](../../docs/findings.md#割り当てカウンターの比較)。
