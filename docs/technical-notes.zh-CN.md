# ROGue Survivor — 技术说明

**简体中文** · [English](technical-notes.en.md) · [日本語](technical-notes.ja.md) · [项目首页](../README.md)

<a id="source-map"></a>

## 模块与资源连接

代码目录：[Assets/ROGue Survivor/Codes](../Assets/ROGue%20Survivor/Codes)。

| 脚本 | 角色 |
| :--- | :--- |
| GameManager、Player、EnemyLogic | 单局状态、移动／受伤、追踪／受击／死亡。 |
| ObjectManager、Spawner、Reposition | 三预制体池、刷怪阶段、场地／敌人重定位。 |
| Weapon、Bullet、Scanner | 旋转／鼠标射击、穿透与退出回收、最近目标扫描。 |
| ItemData、Item、LevelUp、Gear | 五份配置、创建／升级、抽样、装备倍率。 |
| Hand、HUD、CameraFollow | 持物表现、五种数值显示、玩家屏幕坐标跟随。 |
| Pause、Result、AudioManager | 时间暂停、胜负标题、BGM与效果音。 |

19份脚本的场景／预制体／资源引用已检查；Weapon与Gear由Item通过AddComponent动态创建，不能因无序列化引用判断它们没接入。四种敌人死亡动画均有`Dead`事件，最终SetActive(false)归还池。

[场景](../Assets/Scenes/SampleScene.unity)、[三预制体](../Assets/ROGue%20Survivor/Prefabs)、[五物品](../Assets/ROGue%20Survivor/Data)、[输入](../Assets/ROGue%20Survivor/Player.inputactions)。

<a id="combat"></a>

## 移动、追踪与弹丸

玩家FixedUpdate使用inputVec×speed×fixedDeltaTime进行MovePosition，场景初始速度4。EnemyLogic使用归一化的目标方向直接移动，无障碍规划；之后把velocity清零。击退为离开玩家方向的3单位Impulse，并非弹性碰撞求解：

```math
p_{t+1}=p_t+v\,\frac{p_{player}-p_t}{\lVert p_{player}-p_t\rVert}\Delta t
```

撬棍由父Weapon按默认150°/s旋转，Batch把实例放在半径1.8处，角度代码为整数`360/count`。手枪每默认0.5 s、按住左键发一颗弹；方向来自Camera.ScreenToWorldPoint，z差符号的平面投影还需实机核对。它不是射线命中检测或自动锁敌。

Bullet速度10；perforation每次Enemy触发减1，到-1回收，因此起始0可命中一次；-1用于环绕武器，表示不因碰撞耗尽。出Area失活。敌人扣血并击退，死亡关碰撞和刚体模拟、加kill与经验，动画事件再失活。玩家OnCollisionStay不筛Enemy标签，实际伤害范围依赖场景碰撞层。

<a id="pool-map"></a>

## 对象池与场地复用

ObjectManager.Awake只分配空List数组，不创建实体。Get按桶foreach寻找非active对象，无闲置才Instantiate并加入List。单次查找最坏O(n)，池只增长无上限，也没有全局零分配承诺。Scanner每物理帧的CircleCastAll结果、卡牌数组、新Weapon/Gear与协程等也需Profiler检查。

当前场景是四个Ground块，不是3×3九块。Area退出时按相对距离与输入符号平移横56或纵40；diffX==diffY且不满足角落条件时不移动。不能把这一特定规则写成完整拓扑环面。

活动敌人的离区处理直接相对自身平移：

```math
p'_{enemy}=p_{enemy}+30\,v_{input}+\epsilon,\qquad \epsilon_x,\epsilon_y\sim U(-5,5)
```

没有在此函数内再归一化，也不是以玩家为原点生成锥形采样；输入为零时只随机偏移。

<a id="progression"></a>

## 成长、波次和暂停

| 场景阶段 | 刷怪间隔s | 生命 | 速度 |
| :--- | ---: | ---: | ---: |
| 0–60 s | 0.8 | 10 | 2 |
| 60–120 s | 0.6 | 15 | 2.8 |
| 120–180 s | 0.4 | 25 | 3.3 |
| 180 s之后 | 0.3 | 37 | 3.6 |

源码用floor(gameTime/60)并钳制到数组末项，`levelTime`未使用，不是指数动态难度。每次敌人死亡直接GetExp，无经验拾取物／吸附算法。经验阈值为3/5/10/20/40/60/80/100/120/200，后续钳在200。

五个物品是近战、手枪、手套、鞋、满血治疗；每份配置有五级数组。第一次武器选择用baseDamage/baseCount，后续伤害为base×(1+damages[level])、count累加；不是整张旧六级数值表。鞋子从源码基速3计算，而场景玩家起始4，会有“升级反而降速”的风险。手套改变环绕速度或射击间隔。

升级从五个索引拒绝重复抽三个，不按稀有度或权重。满级项替换为同一个items[4]治疗，多个替换可合并为一个可见选项；未补抽。游戏胜利300 s后激活10000伤害的EnemyCleaner，0.5 s后结算；失败也等待0.5 s。暂停／升级时间归零，BGM是AudioHighPassFilter高通而非低通。

Weapon.Update残留`Input.GetButtonDown("Jump")`直接LevelUp(10,1)的测试路径；项目同时启用新旧输入。不要把调试行为或通用InputActions内的手柄／触屏绑定当作完整平台操作支持。

<a id="evidence"></a>

## 历史画面与静态验证

| 升级选择 | 胜利记录 | 开局 |
| :---: | :---: | :---: |
| ![升级](images/demo_levelup_cards.png) | ![胜利](images/demo_victory_survived.png) | ![开局](images/demo_gameplay_start.png) |

本轮解析manifest、lock、InputActions并追踪场景及GUID；46个非项目GUID在本机PackageCache找到，内置GUID单列。另三个未找到的旧图片／动画GUID仅出现在未被当前场景资源图引用的Pal、Player0_*及Stand1历史资源，不据此宣称全仓库所有资源都完整。

未运行Editor、C#完整编译、Play Mode、导出或Profiler；截图中的等级／击杀只是一个展示时刻。144+ FPS、500敌人、0 B/Frame、draw calls与内存成绩无脚本和原日志，已删除。应用代码、配置、场景、素材、meta均未改动。
