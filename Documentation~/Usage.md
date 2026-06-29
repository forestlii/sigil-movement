# Sigil Movement — Usage Guide

[English](Usage.md) | [简体中文](Usage.zh-CN.md)

> The movement & locomotion **companion package** `com.likeon.gas.movement`. Depends on the Sigil
> core package `com.likeon.gas` (GitHub: [sigil-gas](https://github.com/forestlii/sigil-gas)). Namespace
> is unchanged (`Likeon.GAS`).
>
> Design stance: movement is a **consumer** of the GameplayTag state bus, not part of the ability
> system itself — so it ships as its own package. You can use the core with your own movement, or
> install this package for ready-made movement + a locomotion animation data layer.

## Contents

1. [Install](#1-install)
2. [Movement system](#2-movement-system)
3. [Locomotion animation driver](#3-locomotion-animation-driver)
4. [Relationship to GAS (the state bus)](#4-relationship-to-gas-the-state-bus)

---

## 1. Install

This package depends on the Sigil core package; install both:

1. Add `com.likeon.gas` (Sigil core) first.
2. Add `com.likeon.gas.movement` (this package).

(Package Manager → *Add package from disk…* → select each `package.json`.)

Assemblies: `Likeon.GAS.Movement` (Runtime, references `Likeon.GAS.Runtime`) + `Likeon.GAS.Movement.Editor`. The namespace is `Likeon.GAS` throughout, so `using Likeon.GAS;` is all you need.

---

## 2. Movement system

`CharacterMovementSystemComponent` (requires a `CharacterController`). Data-driven, wired to GAS through the state bus.

```csharp
using Likeon.GAS;

var move = gameObject.AddComponent<CharacterMovementSystemComponent>();
// If the same GameObject has an ASC, Awake automatically sets the ASC as the tag provider —
// movement state then mirrors onto the ASC (optional integration).

move.PushAvailableMovementDefinition(movementDef);          // a MovementDefinition asset
move.SetMovementSet(GameplayTag.RequestTag("Movement.Set.Default"));
move.SetDesiredMovement(MovementTags.MovementState_Jog);    // Walk / Jog / Sprint
move.SetDesiredRotationMode(MovementTags.RotationMode_VelocityDirection); // face movement direction

// Feed an input direction every frame (world space)
move.SetInputDirection(camRelativeDir);
```

`MovementDefinition` (*Create → Likeon → GAS → Movement Definition*): holds several MovementSets; each set has Walk/Jog/Sprint `MovementStateSetting`s (Speed / Acceleration / BrakingDeceleration / RotationInterpolationSpeed …). It can be swapped on a stack (e.g. push a set when equipping a greatsword, pop it when unequipping).

`MovementTags`: the built-in movement-related GameplayTags (`MovementState_Walk/Jog/Sprint`, `RotationMode_*`, `LocomotionMode_*`, etc.).

---

## 3. Locomotion animation driver

`LocomotionAnimationDriver` (attach to the character; it auto-finds the CharacterController / Animator / MovementSystemComponent) computes drive values from movement data every frame — readable directly, and also written into Animator parameters:

```csharp
var driver = character.AddComponent<LocomotionAnimationDriver>();
// Read directly: driver.State.Speed / .CardinalDirection / .Lean / .Jumping / .ViewYawAngle …
// Or it writes Animator parameters (Speed / Direction / VelocityYaw / LeanForward / LeanRight /
//   Grounded / Jumping / Falling / ViewYaw / ViewPitch / PitchAmount) to feed a blend tree.
```

The output covers: horizontal speed, view-relative velocity yaw, 4-/8-way direction (with dead-zone hysteresis to avoid boundary flicker), lean, in-air state (jump-apex time / falling duration / just-landed / ground prediction), view-relative aim offset, and core-state tags (LocomotionMode / RotationMode / MovementState / MovementSet).

`LocomotionMath` is a set of pure functions (speed / yaw / direction / lean …) that can be called standalone and are easy to test.

**Sample controller**: the menu *Likeon ▸ GAS ▸ Samples ▸ Create Locomotion Animator Controller* generates, in one click, a layered Animator Controller aligned to the parameters above (a base 8-way 2D blend tree + Jump/Fall states + an upper-body AimOffset additive layer + an AvatarMask placeholder), with the animation-clip slots left empty for you to fill.

> Note: the traditional node-graph animation approach has no direct equivalent in Unity, so this package replaces it with an "Animator data-driven layer + sample layered Controller"; the final animation clips and feel are the host project's responsibility.

---

## 4. Relationship to GAS (the state bus)

**State bus**: after you call `SetDesiredMovement(Sprint)`, `Movement.State.Sprint` is mirrored automatically onto the ASC's loose tags — so the core package's [input dispatch layer](https://github.com/forestlii/sigil-gas) **state-driven key polymorphism** (e.g. "slide on one key while sprinting, otherwise crouch") is wired to the real movement state.

The ASC mirror is **optional**: the movement system runs fine without an ASC (just don't call `SetGameplayTagsProvider`); it simply won't push state up to the ability layer.

> This package depends only on the core's `GameplayTag` vocabulary and (optionally) `AbilitySystemComponent`, with no reverse coupling — the core stays usable on its own.
