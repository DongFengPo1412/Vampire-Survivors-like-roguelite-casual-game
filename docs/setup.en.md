# ROGue Survivor — Setup and operation

[简体中文](setup.zh-CN.md) · **English** · [日本語](setup.ja.md) · [Overview](../README_EN.md)

<a id="environment"></a>

## Editor version and restore

[ProjectVersion](../ProjectSettings/ProjectVersion.txt) records Unity2022.3.17f1c1; prefer that version and validate any migration separately. Manifest/lock specify Input System1.7.0, URP14.0.9, Cinemachine2.9.7, and other packages. Add the root through Unity Hub and allow package restoration.

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

Click Start to receive the orbiting weapon. WASD/arrows move; aim with the mouse and hold left after obtaining the pistol. Choose cards on level-up; Esc pauses/resumes. Retry loads scene0; Application.Quit does not close the Editor.

SampleScene is the only enabled build scene. Use Unity Build/Build And Run for the actual target. No separate CLI build method, verified platform release, or current build artifact is supplied; a fictitious one-command export is not documented. The remaining Jump upgrade debug path affects balance.

<a id="checks"></a>

## Audit scope and next checks

This pass reviewed19 scripts/serialized connections without running gameplay or benchmarks.

| Problem | Inspect |
| :--- | :--- |
| No movement/fire | Both input backends, PlayerInput notification, focus, pistol acquisition. |
| Dead enemy not recycled | Animator/Dead event, collision/isALive. |
| Fewer than three cards | Multiple max-level items become one heal object. |
| Unexpected shoe speed | Scene4 versus Gear baseline3. |
| Old resources missing | Separate active-scene resources from unused history; do not substitute unrelated assets. |
| Performance differs | No controlled baseline; profile load, allocation, and frame time. |

Validate correctness/balance before pooling performance. Visual/audio license records remain incomplete; root MIT does not supply them.
