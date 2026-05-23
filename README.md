# ROGue Survivor (类吸血鬼幸存者 Roguelite 休闲游戏)

![Game Logo](Assets/ROGue%20Survivor/Logo.png)

**ROGue Survivor** 是一款基于 Unity 引擎开发的 2D 复古像素风 Roguelite 休闲动作游戏。玩家在游戏中扮演主角，在无尽涌出的怪物潮中生存，通过消灭怪物获取经验值升级，解锁和强化各种武器与被动装备，最终在规定时间内击败敌人存活下来。

---

## 🎮 控制说明

本项目使用 Unity 的 **新版输入系统 (Input System)**。以下是默认的游戏控制键位：

| 按键 / 操作 | 功能描述 |
| :--- | :--- |
| **W / A / S / D** 或 **方向键** | 控制角色移动（上下左右） |
| **鼠标移动** | 控制远程武器的射击方向 |
| **鼠标左键 (LMB)** | 点击或按住进行远程武器发射（如手枪） |
| **空格键 (Space)** | (测试用) 快速提升武器等级 |
| **ESC 键** | 暂停 / 恢复游戏 |

---

## 🛠️ 项目环境依赖

在 Unity Editor 中成功加载并运行此场景，您需要确保已通过 **Package Manager** 安装以下 Unity 官方包：

1. **Cinemachine**：用于实现平滑的 2D 摄像机跟随，跟随玩家移动。
2. **Input System**：新版输入系统，用于处理角色的移动与射击输入。

> [!IMPORTANT]
> 请在打开项目后，进入 **Window -> Package Manager**，选择 **Unity Registry** 搜索并安装上述两个依赖包，否则可能会出现输入或摄像机相关的报错。

---

## 📂 代码结构与核心架构

项目的所有 C# 脚本均存放在 `Assets/ROGue Survivor/Codes/` 文件夹下。以下是核心代码文件的结构及各自负责的功能：

### 核心管理与系统控制
*   [GameManager.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/GameManager.cs) - **游戏主管理器**
    *   管理整体游戏状态（游戏开始、暂停、恢复、胜利、失败、退出）。
    *   记录游戏全局数据（游戏时间、玩家当前生命值、最大生命值、等级、杀敌数、当前经验值、升级所需经验表）。
    *   控制游戏时间尺度（`Time.timeScale`）来实现游戏暂停与恢复。
*   [ObjectManager.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/ObjectManager.cs) - **轻量级对象池管理器**
    *   针对频繁生成和销毁的物体（如子弹、怪物）实现复用机制，避免高频 `Instantiate` 和 `Destroy` 引起的内存碎片与垃圾回收 (GC) 卡顿。
*   [AudioManager.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/AudioManager.cs) - **音频管理器**
    *   控制全局背景音乐 (BGM) 的播放与切换，以及各类音效 (SFX) 的播放（例如射击、受击、升级、胜利/失败音效）。

### 角色与战斗机制
*   [Player.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Player.cs) - **玩家控制脚本**
    *   接收新版 Input System 的移动输入并控制 `Rigidbody2D` 进行移动。
    *   控制角色动画状态机（移动速度、受击与死亡）。
    *   处理与怪物的碰撞伤害，并在血量归零时触发死亡流程和 GameOver。
*   [Weapon.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Weapon.cs) - **武器发射与升级逻辑**
    *   实现不同类型的武器逻辑：
        *   **近战武器 (ID 0 - 撬棍)**：围绕玩家旋转，具有无限穿透力，升级可增加撬棍数量和旋转速度。
        *   **远程武器 (ID 1 - 手枪)**：朝着鼠标指针方向发射子弹，升级可提升发射速率与伤害。
    *   管理武器的初始化、升级以及触发 Gear 增益的应用。
*   [Bullet.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Bullet.cs) - **子弹与碰撞判定**
    *   设置子弹伤害、穿透次数（Perforation）以及运动方向。
    *   处理与敌人的碰撞检测：造成伤害、减少穿透次数，并在穿透次数耗尽时返还至对象池。
*   [Scanner.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Scanner.cs) - **范围扫描器**
    *   利用圆周物理射线检测（`Physics2D.CircleCastAll`）自动扫描玩家周围的敌人，并锁定距离最近的目标。

### 敌人生存与生成机制
*   [EnemyLogic.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/EnemyLogic.cs) - **怪物 AI 逻辑**
    *   利用 `Rigidbody2D` 实现平滑地向玩家位置进行追踪移动。
    *   处理怪物属性（当前生命值、最大生命值、移动速度、受击动画与死亡回退）。
    *   在受到子弹攻击时执行受击反馈、扣血并检测死亡；死亡时掉落经验并增加玩家杀敌计数。
*   [Spawner.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Spawner.cs) - **怪物刷怪控制器**
    *   根据游戏进行的时间，动态调整刷怪的波次、怪物类型（`SpawnData`）和刷怪间隔，使游戏难度曲线逐渐上升。
*   [Reposition.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Reposition.cs) - **地图与敌人动态重定位 (无限地图)**
    *   **无限地图滚动**：当玩家移出当前地面网格边界（`Area`）时，该脚本自动将相反方向的地面网格平移拼接至玩家前进方向，从而实现“无边界”的无限平铺地图。
    *   **敌人回收重置**：当玩家走得太远，脱离屏幕的怪物会被自动重新瞬移至玩家周围，保持怪物浓度并避免怪物无限制散落在过远区域。

### 装备与升级系统
*   [ItemData.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/ItemData.cs) - **物品静态数据 (ScriptableObject)**
    *   定义武器和装备的属性模版（ID、名称、类型、描述、基础伤害、基础数量、图标、子弹预制体等），方便在 Unity 编辑器中进行配置与扩展。
*   [Item.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Item.cs) - **升级卡片 UI 控制**
    *   控制升级选择界面上的卡片显示（图标、名称、当前等级描述）。
    *   处理点击升级卡片后的逻辑：创建新武器/装备、升级已有武器属性、或是为角色回满血量。
*   [Gear.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Gear.cs) - **被动装备逻辑**
    *   控制非武器的被动加成装备：
        *   **手套 (Glove)**：缩短所有武器的攻击间隔 / 增加近战武器旋转速度。
        *   **鞋子 (Shoe)**：增加玩家角色的基础移动速度。

### 用户界面 (UI)
*   [HUD.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/HUD.cs) - **游戏主界面数据显示**
    *   实时在屏幕上更新经验值条（Slider）、血量条（Slider）、生存计时、玩家当前等级和杀敌数。
*   [LevelUp.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/LevelUp.cs) - **升级界面控制**
    *   当玩家经验条满时弹出升级选择框，并随机挑选出几种未满级或可解锁的卡片供玩家选择，选择后恢复游戏。
*   [Pause.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Pause.cs) - **暂停菜单**
    *   控制游戏在玩家按下 ESC 时的暂停界面显示与隐藏。
*   [Result.cs](file:///C:/Unity%20game%20projects/My%20project%20%28ROG%29/Assets/ROGue%20Survivor/Codes/Result.cs) - **结果结算界面**
    *   游戏通关（坚持存活到最大时间）或游戏失败时弹出，显示最终的战斗报告，并提供重新开始或退出选项。

---

## 🔒 Git 版本控制与忽略规则

为了保证仓库的精简性与可维护性，本项目配置了标准的 `.gitignore` 忽略规则，自动排除不需要（且不适合）纳入版本管理的文件。

### 🚫 被排除的部分及原因

1.  **`Library/` 目录**：包含 Unity 导入资源后生成的元数据和本地缓存。此目录非常庞大，且完全可以通过 `Assets/` 的原始文件在另一台电脑上重新生成，因此不适合上传。
2.  **`Temp/` 目录**：Unity 运行编辑器时产生的临时交互数据，关闭项目后通常会自动清除，无需上传。
3.  **`Obj/` 和 `bin/` 目录**：Mono/Visual Studio 编译 C# 脚本产生的临时编译目标文件。
4.  **`Logs/` 目录**：Unity 编辑器的日志文件，仅对本地调试有用。
5.  **`UserSettings/` 目录**：包含本地编辑器的个性化窗口布局、视图状态和本地设置，不需要在团队或多设备间共享。
6.  **`*.csproj` / `*.sln` / `*.suo` / `*.user` (解决方案与项目文件)**：由 IDE 自动生成的临时项目索引文件。由于每个人本地安装的 Unity 路径或 Visual Studio 版本不同，这些文件应在本地加载项目时自动生成，提交会导致版本冲突。
7.  **`artifacts/` 目录**：Unity 现代缓存框架的资产依赖缓存。
8.  **`.vs/`, `.idea/`, `.vscode/` 目录**：本地开发环境 Visual Studio、Rider 或 VS Code 自动生成的本地工作区缓存、调试配置与索引。
