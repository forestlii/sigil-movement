# 更新日志

[English](CHANGELOG.md) | [简体中文](CHANGELOG.zh-CN.md)

本文件记录 Sigil Movement 的所有重要变更。格式遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [未发布]

### 变更

- 测试迁入包内 `Tests/` 目录（PlayMode + EditMode），随包发布，用户加 `"testables"` 即可运行。

## [0.1.0] - 2026-06-29

作为独立配套包的首个版本。从 Sigil 核心包（`com.likeon.gas`）拆出，让 GAS 核心保持与移动解耦。
命名空间不变（`Likeon.GAS`）。

### 新增

- **MovementSystemComponent / CharacterMovementSystemComponent** — 基于 CharacterController 的标签驱动移动状态机，状态镜像到 ASC。
- **运动数据层** — `LocomotionAnimationDriver` + `LocomotionMath` + `LocomotionTypes`：速度 / 偏航 / 四八向 / 倾身 / 空中状态 / 视角相对 aim offset / 核心状态标签 → Animator。
- **MovementDefinition / MovementSettings / MovementTags** — 数据驱动的移动配置。
- **示例分层 Animator Controller 生成器** — `Likeon ▸ GAS ▸ Samples`（`SampleLocomotionControllerBuilder`）。

[未发布]: #未发布
[0.1.0]: #010---2026-06-29
