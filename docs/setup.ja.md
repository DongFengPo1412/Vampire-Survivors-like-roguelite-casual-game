# ROGue Survivor — 導入と操作

[简体中文](setup.zh-CN.md) · [English](setup.en.md) · **日本語** · [概要](../README_JA.md)

<a id="environment"></a>

## 版と復元

[ProjectVersion](../ProjectSettings/ProjectVersion.txt)はUnity2022.3.17f1c1です。まず同版を使い、移行は別途確認します。manifestとlockはInput System1.7.0、URP14.0.9、Cinemachine2.9.7等です。Unity Hubでルートを追加しパッケージ復元を待ちます。

```text
.
├── Assets/Scenes/SampleScene.unity
├── Assets/ROGue Survivor/
│   ├── Codes/      # 19スクリプト
│   ├── Data/       # 5 ItemData
│   ├── Prefabs/    # Enemyと2弾丸
│   ├── Animations/ / Audio/ / Sprites/ / Tiles/
│   └── Player.inputactions
├── Packages/
├── ProjectSettings/
└── docs/
```

Library・Temp・Buildsは生成物です。授業の旧実行ファイルは本機にありますが、現HEAD一致の根拠はなく現工程の検証に代用しません。

<a id="scene"></a>

## シーンと入力

GameManager参照、ObjectManager3スロット、Spawner4段階、Item5設定、OnClick/Hide、敵Deadイベント、Ground/Area/Enemy/Bulletタグとレイヤーを確認します。C#だけをコピーせずmetaを保ちます。

activeInputHandler=2で新旧入力併用です。PlayerInputはPlayer図とメッセージ通知、WASD・矢印をMoveへ接続します。銃は旧GetMouseButtonとmousePositionを使い、新入力だけに切り替えて同一動作とはしません。汎用機器定義は完全なゲームパッド・モバイル対応の証明ではありません。

<a id="operation"></a>

## 操作とビルド

開始で周回武器を取得します。WASD／矢印、マウス照準、拳銃取得後に左保持、強化はカード、Escはポーズ／再開です。再試行はScene0、Application.QuitはEditorを閉じません。

Jumpの直接強化を削除しました。新旧入力バックエンドは併用したままで、汎用ゲームパッド・タッチ定義を完全な操作検証とは扱いません。

<a id="checks"></a>

## 確認範囲と今後

19本と接続の静的確認のみで、実行・性能試験は行っていません。

| 現象 | 確認 |
| :--- | :--- |
| 移動・射撃不能 | 新旧入力、通知、フォーカス、拳銃取得。 |
| 死亡敵が戻らない | Animator・Deadイベント、衝突・isALive。 |
| 3カード未満 | 複数最大項目が1回復へ代替される挙動。 |
| 靴の値 | シーン基速と設定倍率で各段階を確認。固定3は使用しません。 |
| 古い素材Missing | 現シーンと未参照履歴を区別し、無関係素材で代用しません。 |
| 性能が旧表と違う | 制御された基準なし。負荷・割当て・フレーム時間を記録します。 |

正しさとバランスを先に確認してから再利用効果を測ります。素材ライセンスは未整備で、MITから推定しません。

Unity 2022.3.17f1c1付属Roslynと既存アセンブリ参照でアプリC#19本のコンパイルが通過し、出力は隔離キャッシュ内です。Editor起動、Play Mode、プラットフォーム出力、Profilerは未実施です。敵・地形接触、靴の全段階、割り切れない周回個数は実プレイで確認が必要です。

### 強化選択の隔離回帰

実際の `LevelUp.Next` をUnityインターフェースの隔離スタブで動かし、通常・治療のみ・残り2件・候補なし・入力3件未満の5状態を計500回検査しました。アプリC#19本のコンパイルも成功。[独立テスト](../Tests/UpgradeSelectionRegression.cs)はAssets外で、Play Modeの代わりではありません。

独立プロジェクトには.NET 6 SDKが必要です。今回はSDKコマンドではなくUnity付属Roslyn/.NETで同じソースを実行しました。生成物はリポジトリ外へ出力します。

```powershell
$task = Join-Path $env:USERPROFILE '.codex\cache\tasks\game-check-<unique-id>'
dotnet build Tests/UpgradeSelectionRegression.csproj -p:BaseIntermediateOutputPath="$task/obj/" -p:OutputPath="$task/bin/"
dotnet "$task/bin/UpgradeSelectionRegression.dll"
```
