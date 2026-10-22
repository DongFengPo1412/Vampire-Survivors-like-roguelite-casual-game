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

近接Weaponは既定150°/sで回転し、Batchは半径1.8、整数360/countで配置します。拳銃は左保持中、既定0.5 sごとに1発です。Camera.ScreenToWorldPointのz差符号は実行時の投影確認が必要で、ヒットスキャンや自動照準ではありません。

弾速10、Enemy接触でperforationを減らして-1で回収するため初期0は1回命中、初期-1は周回の無制限接触です。Area退出でも非活性化します。敵は被害・撃退後、死亡で当たり判定・物理を止め、killと経験を加え、アニメーション後に無効化します。プレイヤーOnCollisionStayはEnemyタグを選別せず、実際の被害対象はシーンのレイヤー次第です。

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

近接・拳銃・手袋・靴・全回復の5設定は5項目の成長配列です。初取得はbaseDamage/baseCount、後続はbase×(1+damages[level])とcount加算で、旧6段表ではありません。靴は基速3、シーン初速4なので減速する可能性があります。手袋は周回速度・射撃間隔を変えます。

5索引から重複拒否で3つを抽出し、レア度重みはありません。最大レベル項目を同じitems[4]回復へ代替するため表示数が減っても再抽出しません。300 sで10000被害のEnemyCleaner、0.5 s後に勝利、敗北も0.5 s後です。ポーズ・強化は時間0、AudioHighPassFilterでありローパスではありません。

Weapon.UpdateにJump入力でLevelUp(10,1)するテストが残ります。新旧入力を併用し、汎用InputActionsのゲームパッド・タッチ定義を完全なプラットフォーム対応とは扱いません。

<a id="evidence"></a>

## 過去の画面と静的確認

| 強化 | 勝利 | 開始 |
| :---: | :---: | :---: |
| ![強化](images/demo_levelup_cards.png) | ![勝利](images/demo_victory_survived.png) | ![開始](images/demo_gameplay_start.png) |

manifest・lock・InputActions・シーンGUIDを照合しました。外部46 GUIDは本機PackageCacheに解決し、内蔵GUIDは別です。未解決3 GUIDは現在のシーン資源グラフ外のPal、Player0_*、Stand1の旧画像・アニメーションのみで、全資源の完全性は主張しません。

Editor・C#完全コンパイル・Play Mode・出力・Profilerは未実行です。画面のレベル・撃破は一時点で、144+ FPS・500体・0 B/Frame・描画回数・メモリ表に生記録がなく削除しました。コード・設定・シーン・素材・metaは未変更です。
