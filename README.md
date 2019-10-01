# ModularAudioSharp

## 環境要件

* .NET Framework 4.7.2（実行環境）
* Visual Studio 2019（開発・ビルド環境）

## ビルド・実行方法

* `ModularAudioSharp.sln` を Visual Studio で開く
* [ビルド - ソリューションのビルド]
* ソリューション エクスプローラーでプロジェクト `Moddl` を右クリックし、[スタートアップ プロジェクトに設定]
* 再び `Moddl` を右クリックし、[プロパティ]
* [構成] が `Debug` であることを確認（処理速度に難がある場合 `Release` にすると改善します）
* [プロパティ] の [デバッグ] シートの [コマンド ライン引数] に ModDL ファイルのパス（`exe` からの相対パス可）を記入
* メインメニューの [デバッグ - デバッグの開始] もしくは [デバッグ - デバッグなしで開始]

→ 演奏が始まります
