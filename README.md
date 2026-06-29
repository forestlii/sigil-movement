# Sigil Movement — Locomotion Companion

[English](README.md) | [简体中文](README.zh-CN.md)

A **companion package** for [Sigil](https://github.com/forestlii/sigil-gas) (`com.likeon.gas`) that provides the
movement and locomotion layer. Kept separate from the GAS core on purpose: movement is a
*consumer* of the GameplayTag state bus, not part of the ability system itself — so you can
use Sigil core with your own movement, or this package with Sigil.

- **Depends on:** `com.likeon.gas` (Sigil core) + `com.unity.inputsystem` (via core)
- **Namespace:** `Likeon.GAS` (same as the core, so it doesn't break your `using`)
- **Engine:** Unity 6 (6000.4)
- **License:** MIT

## Install

This package depends on the Sigil core package. Install both:

1. Add `com.likeon.gas` (Sigil core) first.
2. Add `com.likeon.gas.movement` (this package).

(Package Manager → *Add package from disk…* → each `package.json`.)

### Running tests

The package ships with EditMode + PlayMode tests under `Tests/`. To run them, add the
package to `"testables"` in your project's `Packages/manifest.json`, then open
**Window → General → Test Runner**:

```json
"testables": [ "com.likeon.gas.movement" ]
```

## Features

- **MovementSystemComponent** — movement-set / state machine, definition stack, rotation modes, input direction.
- **CharacterMovementSystemComponent** — actual movement on a `CharacterController`.
- **State bus to the ability system** — movement state mirrors onto the ASC as loose tags (drives ability gating, e.g. sprint → slide).
- **Locomotion animation driver** — `LocomotionAnimationDriver` + `LocomotionMath`: speed, local-velocity yaw, 4-/8-way direction (dead-zone hysteresis), lean, in-air state (jump apex / falling time / just-landed / ground prediction), view-relative aim offset, core-state tags → Animator parameters.
- **Sample layered Animator Controller generator** — `Likeon ▸ GAS ▸ Samples` builds a controller matching the driver (8-way blend tree + jump/fall + upper-body aim-offset layer).

The driver writes Animator parameters; final animation clips & feel are the host project's.

## License

[MIT](LICENSE.md) — free for any use including commercial, just keep the copyright notice.
