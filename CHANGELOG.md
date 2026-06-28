# Changelog

All notable changes to Sigil Movement are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

本文件记录 Sigil Movement 的所有重要变更。格式遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [0.1.0] - 2026-06-29

First release as a standalone companion package. Extracted from the Sigil core package
(`com.likeon.gas`) so the GAS core stays UI/movement-agnostic. Namespace unchanged (`Likeon.GAS`).

作为独立配套包的首个版本。从 Sigil 核心包（`com.likeon.gas`）拆出，让 GAS 核心保持与移动解耦。
命名空间不变（`Likeon.GAS`）。

### Added / 新增

- **MovementSystemComponent / CharacterMovementSystemComponent** — GameplayTag-driven movement state machine on a CharacterController, state mirrored to the ASC. / 标签驱动的移动状态机，状态镜像到 ASC。
- **Locomotion data layer** — `LocomotionAnimationDriver` + `LocomotionMath` + `LocomotionTypes`: speed / yaw / 4-8 way direction / lean / in-air state / view-relative aim offset / core-state tags → Animator. / 运动动画数据层。
- **MovementDefinition / MovementSettings / MovementTags** — data-driven movement configuration. / 数据驱动的移动配置。
- **Sample layered Animator Controller generator** — `Likeon ▸ GAS ▸ Samples` (`SampleLocomotionControllerBuilder`). / 示例分层 Controller 生成器。

[0.1.0]: #010---2026-06-29
