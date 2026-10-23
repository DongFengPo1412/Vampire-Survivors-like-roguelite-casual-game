# ROGue Survivor — 技術ノート

[简体中文](technical-notes.zh-CN.md) · [English](technical-notes.en.md) · **日本語** · [概要](../README_JA.md)

<a id="source-map"></a>

## モジュールと接続

コード：[Assets/ROGue Survivor/Codes](../Assets/ROGue%20Survivor/Codes)。

| スクリプト | 役割 |
| :--- | :--- |
| GameManager・Player・EnemyLogic | 単局状態、移動・被害、追従・被弾・死亡。 |
| ObjectManager・Spawner・Reposition | 3プレハブのプール、段階、地形・敵再配置。 |
| Weapon・Bullet・Scanner | 周回・マウス射撃、貫通回収、最寄り走査。 |
| ItemData・Item・LevelUp・Gear | 5設定、生成強化、抽出、装備補正。 |
| Hand・HUD・CameraFollow | 持物表示、5数値、画面内プレイヤー追従。 |
| Pause・Result・AudioManager | 時間停止、勝敗、BGM効果音。 |

19本をシーン・プレハブ・資源と照合しました。Weapon/GearはItemからAddComponentするため、直接参照0を未接続とは判断しません。敵死亡4アニメーションのDeadイベントが非活性化して再利用へ戻します。

[シーン](../Assets/Scenes/SampleScene.unity)、[3プレハブ](../Assets/ROGue%20Survivor/Prefabs)、[5物品](../Assets/ROGue%20Survivor/Data)、[入力](../Assets/ROGue%20Survivor/Player.inputactions)。

<a id="combat"></a>

## 移動・追従・弾丸

プレイヤーはinputVec×speed×fixedDeltaTimeでMovePositionし、シーン初速は4です。敵は正規化した目標方向へ直接移動し、その後velocityを0にします。障害物計画はありません。被弾はプレイヤーと逆へImpulse3で、弾性衝突ソルバーではありません。

```math
p_{t+1}=p_t+v\,\frac{p_{player}-p_t}{\lVert p_{player}-p_t\rVert}\Delta t
```

近接は既定150°/s、Batchは半径1.8で `360f/count` の等角配置です。拳銃は左押下中に既定0.5 s間隔で発射します。Camera.ScreenToWorldPointの投影は実行時確認が必要で、ヒットスキャンや自動照準はありません。

敵トリガーで貫通回数を減らし-1で回収するため、初期0は1回命中、初期-1の周回物は消耗しません。領域退出でも回収します。敵死亡は衝突・物理無効化、撃破・経験加算、アニメーションイベント回収を行います。プレイヤー被害はEnemyタグのみとし、シーンの敵プレハブでタグを確認しました。

<a id="pool-map"></a>

## プールと地形

Awakeは空List配列のみを確保します。Getは非activeを線形探索し、不足ならInstantiateして追加します。最悪O(n)、上限なしでゼロ割当てではありません。CircleCastAll、強化配列、新Weapon/Gear、コルーチンもProfiler対象です。

Groundは3×3ではなく4個です。Area退出で相対位置と入力符号に応じ横56か縦40へ動かします。角条件を満たさずdiffX==diffYなら移動しません。完全なトーラス位相とはしません。

有効敵の退出処理は自身に対して平行移動します。

```math
p'_{enemy}=p_{enemy}+30\,v_{input}+\epsilon,\qquad \epsilon_x,\epsilon_y\sim U(-5,5)
```

この関数内に再正規化はなく、プレイヤー原点の円錐サンプリングでもありません。入力0なら乱数変位だけです。

<a id="progression"></a>

## 成長・段階・ポーズ

| シーン段階 | 生成間隔s | HP | 速度 |
| :--- | ---: | ---: | ---: |
| 0–60 s | 0.8 | 10 | 2 |
| 60–120 s | 0.6 | 15 | 2.8 |
| 120–180 s | 0.4 | 25 | 3.3 |
| 180 s以後 | 0.3 | 37 | 3.6 |

floor(gameTime/60)を配列末端へ制限し、levelTimeは未使用で、指数的適応難度ではありません。撃破でGetExpし、経験アイテム・吸着はありません。閾値3/5/10/20/40/60/80/100/120/200、以後200です。

5物品の5段階設定、初回baseDamage/baseCount、以後base×(1+damages[level])・count加算は維持します。靴はAwake記録基速（現シーン4）から `BaseSpeed*(1+rate)` を計算し、前回強化へ重ね掛けしません。手袋は周回速度・射撃間隔を変更します。

最大レベルの非消耗品を除外し、治療は再利用可能な候補として毎回最大1件だけ含めます。有効候補から最大3件を重複なく抽出し、不足時は実数を表示します。治療への重複置換や再試行ループはなく、空の強化配列で消耗品を無効化せず、満了後の古いクリックは拒否します。300 s後の敵一掃・結果遷移は維持します。

Jumpの直接強化を削除しました。新旧入力バックエンドは併用したままで、汎用ゲームパッド・タッチ定義を完全な操作検証とは扱いません。

<a id="evidence"></a>

## 過去の画面と静的確認

| 強化 | 勝利 | 開始 |
| :---: | :---: | :---: |
| ![強化](images/demo_levelup_cards.png) | ![勝利](images/demo_victory_survived.png) | ![開始](images/demo_gameplay_start.png) |

manifest・lock・InputActions・シーンGUIDを照合しました。外部46 GUIDは本機PackageCacheに解決し、内蔵GUIDは別です。未解決3 GUIDは現在のシーン資源グラフ外のPal、Player0_*、Stand1の旧画像・アニメーションのみで、全資源の完全性は主張しません。

Unity 2022.3.17f1c1付属Roslynと既存アセンブリ参照でアプリC#19本のコンパイルが通過し、出力は隔離キャッシュ内です。Editor起動、Play Mode、プラットフォーム出力、Profilerは未実施です。敵・地形接触、靴の全段階、割り切れない周回個数は実プレイで確認が必要です。
