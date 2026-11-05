# C# Boxing Research

構造体の `Dispose` を型制約なしで呼ぶ方法と、ボックスを手動でスタック上に配置する `StackBox<T>` の実験コードです。.NET 10 / 11 RC の CoreCLR・Native AOT と、Unity 6.3 / Unity 7 alpha の Mono・CoreCLR・IL2CPP を比較しています。対象は Windows x64 です。

ランタイムの挙動を調べるための実験です。`unsafe` な操作や内部のオブジェクトレイアウトに依存する実装を含み、実用は推奨しません。

## 主な結果

- IL（中間言語）の `constrained.` 命令を直接使うと、ジェネリック制約なしで構造体の `Dispose` を呼べた。
- `StackBox<T>` の構築とインターフェイス呼び出しは、全環境で反復時の割り当てがなかった。
- 型情報は `TypeHandle` から取得でき、型ヘッダーを読み取るための初回のボックス化も省けた。IL2CPP は初回も割り当て 0 B、Mono は型初期化を含む割り当てが残った。

[調査結果](docs/findings.md) · [実験環境と再実行方法](docs/experiments.md) · [測定レポート](results/README.md) · [参照資料](docs/sources.md)

## 実行

.NET SDK 10.0.403 と PowerShell 7 を使用します。次のコマンドは .NET 10 で呼び出し方式と `StackBox<T>` を比較します。

```powershell
git clone https://github.com/akeit0/csharp-boxing-research.git
cd csharp-boxing-research
./scripts/Run-DotNet.ps1 -Modes safe,stack-box

# .NET 11 RC も含めて JIT / Native AOT を比較
./scripts/Run-DotNet.ps1 -IncludePreview -Modes safe,stack-box
./scripts/Run-NativeAot.ps1 -IncludePreview
```

Native AOT の発行には Windows の C++ ビルドツールも必要です。`Run-NativeAot.ps1 -IncludePreview` では .NET SDK 11.0.100-rc.3.26522.148 を使用します。JIT で 11 RC を実行する場合は、対応する .NET ランタイムが必要です。

Unity は対応する Editor・Windows 向けビルドモジュール・Unity CLI をインストールして実行します。

```powershell
./scripts/Run-Unity.ps1 -Modes stack-box
```

実行時のレポートと生ログは `artifacts/runs/`、Unity Player は `builds/` に生成します。これらは Git の対象外です。収録した測定レポートと生成コードの抜粋は [results](results/README.md) にあります。

## ソースの構成

| 場所 | 内容 |
| --- | --- |
| [shared/Research.Core/Implementations](shared/Research.Core/Implementations) | `StackBox<T>`、ボックスの再利用、C# の呼び出し方式 |
| [shared/Research.Core/Experiments](shared/Research.Core/Experiments) | 値型、割り当て計測、型制約、参照を含む構造体の実験 |
| [shared/Research.IL](shared/Research.IL) | InlineIL.Fody による呼び出しと TypeHandle の取得 |
| [dotnet/Experiments](dotnet/Experiments) | .NET の JIT / Native AOT 共通ホストと IL の表示 |
| [unity](unity) | Unity 向けの起動・計測・ビルド用コードとプロジェクト |
| [scripts](scripts) | ビルドと実験の実行 |

共通コードは .NET SDK で `netstandard2.1` の DLL にし、.NET と Unity から使用します。Unity の起動・ビルド用コードの編集元は `unity/ResearchPlayer.cs` と `unity/ResearchEditor.cs` です。`Sync-Unity.ps1` が共通 DLL とともに各 Unity プロジェクトへ配置します。
