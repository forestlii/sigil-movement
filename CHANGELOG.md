# Changelog

[English](CHANGELOG.md) | [简体中文](CHANGELOG.zh-CN.md)

All notable changes to Sigil Movement are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Added

- **Movement & look input processors** (`InputProcessor_Move`, `InputProcessor_Look`) — bridge the core GIPS input system (`InputSystemComponent` + `InputProcessor`) to this package's movement and camera, so movement/look input flows through the input system instead of bypassing it. `InputProcessor_Move` turns the 2D move axis into a camera-relative world direction and feeds `MovementSystemComponent.SetInputDirection`; `InputProcessor_Look` drives the third-person camera's `AddLookInput`. Wire them in an `InputControlSetup` on the Move/Look input tags. (Kept here, not in the GAS core, so the core stays movement-agnostic.)
- **Movement Demo sample** — a third-person walk-around demo built on the GAS core + this package, showing input flowing through the full GIPS chain (a shipped `.inputactions` with WASD Move + mouse Look → `InputConfig` → `InputControlSetup` with the Move/Look processors → movement/camera). After importing the sample, run **Likeon ▸ GAS ▸ Samples ▸ Build Movement Demo Scene** to generate the playable scene (player + third-person camera + ground) and its config assets.

### Changed

- Tests moved into the in-package `Tests/` folder (PlayMode + EditMode) so they ship with the package and can be run via `"testables"`.

## [0.1.0] - 2026-06-29

First release as a standalone companion package. Extracted from the Sigil core package
(`com.likeon.gas`) so the GAS core stays UI/movement-agnostic. Namespace unchanged (`Likeon.GAS`).

### Added

- **MovementSystemComponent / CharacterMovementSystemComponent** — GameplayTag-driven movement state machine on a CharacterController, state mirrored to the ASC.
- **Locomotion data layer** — `LocomotionAnimationDriver` + `LocomotionMath` + `LocomotionTypes`: speed / yaw / 4-8 way direction / lean / in-air state / view-relative aim offset / core-state tags → Animator.
- **MovementDefinition / MovementSettings / MovementTags** — data-driven movement configuration.
- **Sample layered Animator Controller generator** — `Likeon ▸ GAS ▸ Samples` (`SampleLocomotionControllerBuilder`).

[Unreleased]: #unreleased
[0.1.0]: #010---2026-06-29
