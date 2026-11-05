# Native AOT の測定

2026 年 11 月 6 日（JST）、Windows x64。`Run-NativeAot.ps1 -IncludePreview` で、.NET 10 / 11 RC を Release・win-x64・PublishAot=true で発行した。

- `net10.0`: SDK 10.0.403、ランタイム 10.0.14。
- `net11.0`: SDK 11.0.100-rc.3.26522.148、ランタイム 11.0.0-rc.3.26522.148。
- `ldtoken-net11.0`: `StackBox.AsObject` の CoreClr 分岐だけを `typeof(T).TypeHandle.Value` から `IlCalls.TypeHandle<T>().Value` に変更した比較。初回の 32 B は変わらなかった。

`net10.0` / `net11.0` は型制約と不適合型の呼び出し、`ldtoken-net11.0` はハンドル取得方法の比較を収録する。割り当て量と StackBox の測定は [allocation-bytes/nativeaot](../allocation-bytes/nativeaot) にある。

各フォルダの `compiler.json` は発行設定、`run.json` は共通 DLL の SHA-256 を記録する。`invalid-il` と `invalid-constraint` の -1073740286 は `0xC0000602` に対応する。2 つの呼び出しは別の非インラインメソッドと別プロセスに分離した。

`runtime=CoreClr` は StackBox のレイアウト選択であり、ホストの `execution=NativeAOT; dynamicCodeCompiled=False` が実行方式を示す。

[結果と考察](../../docs/findings.md#native-aot-の結果)、[実行方法](../../docs/experiments.md#native-aot)。
