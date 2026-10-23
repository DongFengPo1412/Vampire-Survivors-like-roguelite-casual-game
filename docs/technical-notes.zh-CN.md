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

近战默认150°/s，Batch在半径1.8上按 `360f/count` 等角放置；手枪默认按住左键每0.5 s发射。方向使用Camera.ScreenToWorldPoint，运行时投影仍需检查；没有射线命中或自动瞄准。

敌人触发时子弹减少穿透计数并在-1时回收，初值0可命中一次，初值-1的环绕物不会耗尽；离区也回收。敌人死亡禁用碰撞/物理、结算击杀经验并由动画事件回收。玩家持续碰撞现在只接受Enemy标签；场景Enemy预制体已核对该标签。

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

五种物品保留原五级配置、首次baseDamage/baseCount与后续base×(1+damages[level])、count累加。鞋子使用Awake捕获的基速（当前场景4），按 `BaseSpeed*(1+rate)` 计算，不叠乘上次结果；手套仍改变环绕速度/射击间隔。

候选先过滤满级非消耗品，治疗作为可重复使用但每轮仅一个的候选；从有效池无放回选最多三项。少于三项时显示实际数量，无重复治疗回退，也不依赖重试循环。治疗不会因空的升级数组被禁用，过期的满级点击被拒绝。300 s 后仍清场并延时结算。

正式路径已移除Jump直接升级；新旧输入后端仍同时启用。通用InputActions中的手柄/触屏绑定不等于完整的平台操作验收。

<a id="evidence"></a>

## 历史画面与静态验证

| 升级选择 | 胜利记录 | 开局 |
| :---: | :---: | :---: |
| ![升级](images/demo_levelup_cards.png) | ![胜利](images/demo_victory_survived.png) | ![开局](images/demo_gameplay_start.png) |

本轮解析manifest、lock、InputActions并追踪场景及GUID；46个非项目GUID在本机PackageCache找到，内置GUID单列。另三个未找到的旧图片／动画GUID仅出现在未被当前场景资源图引用的Pal、Player0_*及Stand1历史资源，不据此宣称全仓库所有资源都完整。

使用本机Unity 2022.3.17f1c1自带Roslyn和现有程序集引用，完整编译19个应用C#脚本通过，输出全部在隔离缓存。未启动Editor、运行Play Mode、导出平台包或执行Profiler；测试仍需覆盖敌人接触/地形接触、鞋子各级和非整除数量环绕布局。
