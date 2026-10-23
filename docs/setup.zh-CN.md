# ROGue Survivor — 部署与操作

**简体中文** · [English](setup.en.md) · [日本語](setup.ja.md) · [项目首页](../README.md)

<a id="environment"></a>

## 项目版本与恢复

[ProjectVersion](../ProjectSettings/ProjectVersion.txt)记录Unity2022.3.17f1c1；优先用该版，其他版本迁移需另行验证。`Packages/manifest.json`和`packages-lock.json`描述Input System1.7.0、URP14.0.9、Cinemachine2.9.7等依赖。Unity Hub把工程根目录作为项目打开，先让包恢复完成。

```text
.
├── Assets/Scenes/SampleScene.unity
├── Assets/ROGue Survivor/
│   ├── Codes/      # 19个游戏脚本
│   ├── Data/       # 5个ItemData
│   ├── Prefabs/    # Enemy和两类弹丸
│   ├── Animations/ / Audio/ / Sprites/ / Tiles/
│   └── Player.inputactions
├── Packages/
├── ProjectSettings/
└── docs/
```

Library、Temp、Builds等为忽略生成内容。桌面课程归档有旧可执行文件，但没有证据证明与当前HEAD完全一致，不用它替代当前工程验证。

<a id="scene"></a>

## 场景与输入

打开SampleScene，核对GameManager的player/pool/UI/result引用、ObjectManager三预制体顺序、Spawner四组参数、Item五配置与按钮OnClick/Hide、敌人Dead动画事件、Ground/Area/Enemy/Bullet标签及层。保留meta，不能只复制.cs丢失GUID。

ProjectSettings的activeInputHandler=2，即新旧输入并存。PlayerInput用Player动作图／消息通知；WASD和方向键绑定Move。枪械仍用旧Input.GetMouseButton(0)与mousePosition，因此不能只切成新输入后宣称行为一致。InputActions其他设备通用绑定并不证明完整手柄／手机玩法。

<a id="operation"></a>

## 游玩与构建

点击开始，默认获得环绕武器。WASD／方向键移动，鼠标瞄准，持有手枪后左键按住持续开火。升级点击卡牌；Esc暂停／恢复。结果页的重试调用Scene0；退出按钮调用Application.Quit，在Editor内不退出编辑器。

正式路径已移除Jump直接升级；新旧输入后端仍同时启用。通用InputActions中的手柄/触屏绑定不等于完整的平台操作验收。

<a id="checks"></a>

## 本轮检查与后续

本轮完成19脚本及序列化连接静态审查，没有运行游戏或性能测试。

| 问题 | 核对 |
| :--- | :--- |
| 无法移动／射击 | 两套输入、PlayerInput通知、焦点和获得手枪状态。 |
| 敌人死后不归池 | 动画控制器与Dead事件、碰撞和isALive状态。 |
| 升级不足三卡 | 满级替换同一治疗项的已知行为。 |
| 鞋子数值 | 使用场景基速与配置倍率；检查不同等级，不再沿用固定3。 |
| 旧资源Missing | 区分当前场景与未引用历史素材，不把缺失图改成其他资源冒充。 |
| 性能不符合旧表 | 尚无受控基准；Profiler记录负载、分配、帧时后再报告。 |

后续先做正确性与平衡验证，再评价对象池效果。美术和音频许可明细需补齐，不能由根MIT推导。

使用本机Unity 2022.3.17f1c1自带Roslyn和现有程序集引用，完整编译19个应用C#脚本通过，输出全部在隔离缓存。未启动Editor、运行Play Mode、导出平台包或执行Profiler；测试仍需覆盖敌人接触/地形接触、鞋子各级和非整除数量环绕布局。

### 升级选择的隔离回归

本轮另用实际 `LevelUp.Next` 和隔离的 Unity 接口替身，对 5 种候选状态执行 500 轮选择检查：正常、只剩治疗、仅两项、没有有效项及不足三项输入；均通过。19 个应用 C# 脚本仍编译通过。测试源码在 [Tests](../Tests/UpgradeSelectionRegression.cs)，不位于 Assets，也不替代 Play Mode。

独立项目需要 .NET 6 SDK；本轮使用 Unity 自带 Roslyn/.NET 运行时执行同一源码，未运行 SDK 构建命令。将构建产物放到仓库外。

```powershell
$task = Join-Path $env:USERPROFILE '.codex\cache\tasks\game-check-<unique-id>'
dotnet build Tests/UpgradeSelectionRegression.csproj -p:BaseIntermediateOutputPath="$task/obj/" -p:OutputPath="$task/bin/"
dotnet "$task/bin/UpgradeSelectionRegression.dll"
```
