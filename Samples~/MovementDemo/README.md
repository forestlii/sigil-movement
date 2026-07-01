# Sigil Movement · Movement Demo

[English](README.md) | [简体中文](README.zh-CN.md)

A minimal locomotion demo: a third-person character you walk around with **WASD + mouse look**,
with input flowing through the GAS input system (`InputProcessor_Move` / `InputProcessor_Look` →
`CharacterMovementSystemComponent` + a third-person camera).

## Run it

This sample is **generator-based** — it ships as scripts + an `.inputactions` asset and builds its
playable scene on demand, so **there is no prebaked scene file until you run the builder**:

1. Menu **Likeon ▸ GAS ▸ Samples ▸ Build Movement Demo Scene**.
2. It generates **`MovementDemo.unity`** plus three assets — `MovementDemo_InputConfig`,
   `MovementDemo_InputControlSetup`, `MovementDemo_MovementDef` — **into this folder**
   (the Console prints the exact path: `[MovementDemo] 已生成场景与资产：…`).
3. Open the generated **`MovementDemo.unity`** and press **Play**.
4. Controls: **WASD** move · **mouse** look. (Walk / jog / sprint speeds live in the generated `MovementDef`.)

## What it shows

The generator wires the whole input path, so the generated assets are a working reference:

- `MovementDemoControls.inputactions` — Unity Input System actions (**Move** / **Look**).
- `MovementDemo_InputConfig` — `InputActionMapping`s binding `InputTag.Move` / `InputTag.Look` to those
  actions; `InputSystemComponent` auto-subscribes them on enable (the `.inputactions` → `InputConfig` path).
- `MovementDemo_InputControlSetup` — an `InputProcessor_Move` (camera-relative) + `InputProcessor_Look`.
- Player = `CharacterController` + `AbilitySystemComponent` + `CharacterMovementSystemComponent` +
  `InputSystemComponent`, followed by a third-person `CameraSystemComponent`.

> Requires the core package `com.likeon.gas`. Placeholder programmer art (plane + capsule); real
> movement feel needs your own animation setup.
