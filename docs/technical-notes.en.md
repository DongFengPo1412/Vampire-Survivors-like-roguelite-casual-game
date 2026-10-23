# ROGue Survivor — Technical notes

[简体中文](technical-notes.zh-CN.md) · **English** · [日本語](technical-notes.ja.md) · [Overview](../README_EN.md)

<a id="source-map"></a>

## Modules and serialized connections

Code directory: [Assets/ROGue Survivor/Codes](../Assets/ROGue%20Survivor/Codes).

| Scripts | Responsibility |
| :--- | :--- |
| GameManager, Player, EnemyLogic | Run state, movement/damage, pursuit/hits/death. |
| ObjectManager, Spawner, Reposition | Three prefab pools, spawn stages, terrain/enemy relocation. |
| Weapon, Bullet, Scanner | Orbit/mouse fire, penetration/recycling, nearest-target scanning. |
| ItemData, Item, LevelUp, Gear | Five configurations, creation/upgrades, sampling, modifiers. |
| Hand, HUD, CameraFollow | Held-item visuals, five HUD values, player screen-position following. |
| Pause, Result, AudioManager | Time pause, result titles, BGM/SFX. |

All 19 scripts were checked against scene/prefab/assets. Weapon/Gear are added dynamically by Item, so no serialized reference does not mean unintegrated code. Four enemy death animations call `Dead`, ultimately deactivating objects for reuse.

[Scene](../Assets/Scenes/SampleScene.unity), [three prefabs](../Assets/ROGue%20Survivor/Prefabs), [five items](../Assets/ROGue%20Survivor/Data), [input](../Assets/ROGue%20Survivor/Player.inputactions).

<a id="combat"></a>

## Movement, pursuit, and projectiles

Player FixedUpdate uses inputVec×speed×fixedDeltaTime with MovePosition; serialized starting speed is 4. EnemyLogic directly follows the normalized player direction without obstacle planning, then clears velocity. Knockback applies an outward impulse of 3, not an elastic-collision solver:

```math
p_{t+1}=p_t+v\,\frac{p_{player}-p_t}{\lVert p_{player}-p_t\rVert}\Delta t
```

The melee weapon defaults to 150°/s; Batch places objects at radius 1.8 using `360f/count` angles. The pistol defaults to one shot per 0.5 s while left is held. Camera.ScreenToWorldPoint determines direction; runtime projection still needs review. There is no hitscan or automatic aim.

Enemy triggers decrement bullet perforation and deactivate at -1: initial 0 permits one hit, and initial -1 orbiting objects do not exhaust. Area exit also deactivates. Enemy death disables collision/physics, awards kills/experience and returns through animation events. Player contact damage now requires the Enemy tag, confirmed on the scene enemy prefab.

<a id="pool-map"></a>

## Pooling and terrain reuse

ObjectManager.Awake allocates empty Lists only. Get linearly searches inactive entries, otherwise instantiates and appends. Worst-case search is O(n), capacity is unbounded, and no zero-allocation guarantee exists. Scanner's per-physics-frame CircleCastAll, card arrays, new Weapon/Gear, and coroutines also need profiling.

The scene has four Ground chunks, not a 3×3 grid. Area exits translate horizontally 56 or vertically 40 based on relative position/input signs. Equal diffX/diffY outside the corner branch produces no movement. This specific rule is not a complete toroidal topology.

An active enemy leaving the area is translated relative to itself:

```math
p'_{enemy}=p_{enemy}+30\,v_{input}+\epsilon,\qquad \epsilon_x,\epsilon_y\sim U(-5,5)
```

This function does not renormalize input or sample a cone around the player; zero input leaves only random displacement.

<a id="progression"></a>

## Progression, stages, and pause

| Scene stage | Spawn interval s | Health | Speed |
| :--- | ---: | ---: | ---: |
| 0–60 s | 0.8 | 10 | 2 |
| 60–120 s | 0.6 | 15 | 2.8 |
| 120–180 s | 0.4 | 25 | 3.3 |
| After 180 s | 0.3 | 37 | 3.6 |

Source uses floor(gameTime/60), clamped to the last entry; levelTime is unused. This is not exponential adaptive difficulty. Enemy death directly grants experience, without pickup/magnet systems. Thresholds are 3/5/10/20/40/60/80/100/120/200 and then capped at 200.

Five items retain the five-level settings, initial baseDamage/baseCount, later base×(1+damages[level]) and accumulated count. Shoes use the Awake-captured baseline (4 in the current scene), computing `BaseSpeed*(1+rate)` without compounding prior upgrades. Gloves still change orbit speed/fire interval.

Eligible items are filtered before selection; heal remains reusable but appears at most once per round. Sample up to three without replacement, showing the available count when fewer remain. There is no duplicate heal fallback or rejection loop. Empty upgrade arrays do not disable consumables, and exhausted stale clicks are rejected. The 300 s enemy-clear/result flow is unchanged.

The direct Jump upgrade has been removed. Both input backends remain enabled; generic gamepad/touch InputActions bindings are not a complete platform-control acceptance test.

<a id="evidence"></a>

## Historical views and static checks

| Upgrade | Victory | Opening |
| :---: | :---: | :---: |
| ![Upgrade](images/demo_levelup_cards.png) | ![Victory](images/demo_victory_survived.png) | ![Opening](images/demo_gameplay_start.png) |

Manifest/lock/InputActions and scene GUID links were inspected. Forty-six non-project GUIDs resolve in local PackageCache; built-ins are separate. Three missing historical image/animation GUIDs occur only in Pal, Player0_*, and Stand1 resources outside the current scene asset graph. This is not a claim that every repository asset is complete.

All 19 application C# scripts compile with the installed Unity 2022.3.17f1c1 Roslyn compiler and existing assembly references, with outputs isolated in task cache. No Editor, Play Mode, platform export or Profiler run occurred. Enemy/terrain contact, all shoe levels and non-divisor orbit counts still need gameplay validation.
