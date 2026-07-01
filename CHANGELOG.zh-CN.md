# 更新日志

[English](CHANGELOG.md) | [简体中文](CHANGELOG.zh-CN.md)

本文件记录 Sigil Movement 的所有重要变更。格式遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [未发布]

## [0.2.0] - 2026-07-01

### 新增

- **移动 / 视角输入处理器**（`InputProcessor_Move`、`InputProcessor_Look`）——把核心 GIPS 输入系统（`InputSystemComponent` + `InputProcessor`）桥接到本包的移动与相机，让移动/视角输入也走输入系统（而非旁路）。`InputProcessor_Move` 把 2D 移动轴转成相机相对的世界方向喂 `MovementSystemComponent.SetInputDirection`；`InputProcessor_Look` 驱动第三人称相机的 `AddLookInput`。在 `InputControlSetup` 里把它们配到 Move/Look 输入标签上。（放本包而非 GAS 核心，让核心保持与移动无关。）
- **Movement Demo 示例**——建在 GAS 核心 + 本包之上的第三人称走动 demo，演示输入走完整 GIPS 链（随包带 `.inputactions`：WASD Move + 鼠标 Look → `InputConfig` → 挂 Move/Look 处理器的 `InputControlSetup` → 移动/相机）。**以烘好的场景 + 玩家 prefab 交付**：导入示例、打开 `MovementDemo.unity` 按 Play（WASD 移动 · 鼠标看 · 1/2/3 走/跑/冲刺）。场景、`Resources/` 下的 `MovementDemoPlayer` prefab 和三个配置资产由编辑器脚本生成——想重烘跑 **Likeon ▸ GAS ▸ Samples ▸ Build Movement Demo Scene**。含 README 和一个 PlayMode 冒烟测试。

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
