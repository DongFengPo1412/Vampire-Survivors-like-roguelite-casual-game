# ROGue Survivor — Setup and operation

[简体中文](setup.zh-CN.md) · **English** · [日本語](setup.ja.md) · [Overview](../README_EN.md)

<a id="environment"></a>

## Editor version and restore

[ProjectVersion](../ProjectSettings/ProjectVersion.txt) records Unity 2022.3.17f1c1; prefer that version and validate any migration separately. Manifest/lock specify Input System 1.7.0, URP 14.0.9, Cinemachine 2.9.7, and other packages. Add the root through Unity Hub and allow package restoration.

```text
.
├── Assets/Scenes/SampleScene.unity
├── Assets/ROGue Survivor/
│   ├── Codes/      # 19 scripts
│   ├── Data/       # 5 ItemData assets
│   ├── Prefabs/    # Enemy and two projectile types
│   ├── Animations/ / Audio/ / Sprites/ / Tiles/
│   └── Player.inputactions
├── Packages/
├── ProjectSettings/
└── docs/
```

Library/Temp/Builds are ignored generated content. An old course executable exists locally, without proof it matches current HEAD; it is not current-project validation.

<a id="scene"></a>

## Scene and input

Inspect GameManager player/pool/UI/result fields, three ObjectManager prefab slots, four spawn stages, five Item assets and OnClick/Hide callbacks, enemy Dead animation events, and Ground/Area/Enemy/Bullet tags/layers. Preserve meta files instead of copying only C# scripts.

activeInputHandler=2 enables both backends. PlayerInput uses Player action-map message notifications; WASD/arrows bind Move. Guns still use legacy Input.GetMouseButton(0)/mousePosition. Switching to new-input-only is not equivalent. Generic device bindings do not establish complete gamepad/mobile support.

<a id="operation"></a>

## Play and build

Click Start to receive the orbiting weapon. WASD/arrows move; aim with the mouse and hold left after obtaining the pistol. Choose cards on level-up; Esc pauses/resumes. Retry loads scene 0; Application.Quit does not close the Editor.

The direct Jump upgrade has been removed. Both input backends remain enabled; generic gamepad/touch InputActions bindings are not a complete platform-control acceptance test.

<a id="checks"></a>

## Audit scope and next checks

This pass reviewed 19 scripts/serialized connections without running gameplay or benchmarks.

| Problem | Inspect |
| :--- | :--- |
| No movement/fire | Both input backends, PlayerInput notification, focus, pistol acquisition. |
| Dead enemy not recycled | Animator/Dead event, collision/isALive. |
| Fewer than three cards | Multiple max-level items become one heal object. |
| Unexpected shoe speed | Scene 4 versus Gear baseline 3. |
| Old resources missing | Separate active-scene resources from unused history; do not substitute unrelated assets. |
| Performance differs | No controlled baseline; profile load, allocation, and frame time. |

Validate correctness/balance before pooling performance. Visual/audio license records remain incomplete; root MIT does not supply them.

All 19 application C# scripts compile with the installed Unity 2022.3.17f1c1 Roslyn compiler and existing assembly references, with outputs isolated in task cache. No Editor, Play Mode, platform export or Profiler run occurred. Enemy/terrain contact, all shoe levels and non-divisor orbit counts still need gameplay validation.

### Isolated upgrade-selection regression

The actual `LevelUp.Next` passes 500 selections across five availability scenarios using isolated Unity-interface stubs: normal, heal only, two remaining, no eligible item and fewer than three inputs. All 19 application C# scripts also compile. The [standalone harness](../Tests/UpgradeSelectionRegression.cs) is outside Assets and does not replace Play Mode.

The standalone project requires the .NET 6 SDK. This run compiled the same source with Unity Roslyn/.NET runtime rather than running the SDK build command. Keep build products outside the repository.

```powershell
$task = Join-Path $env:USERPROFILE '.codex\cache\tasks\game-check-<unique-id>'
dotnet build Tests/UpgradeSelectionRegression.csproj -p:BaseIntermediateOutputPath="$task/obj/" -p:OutputPath="$task/bin/"
dotnet "$task/bin/UpgradeSelectionRegression.dll"
```
