# Sigil Movement — Locomotion Companion / 移动与运动动画配套包

> English / 中文.

A **companion package** for [Sigil](https://github.com/forestlii/sigil-gas) (`com.likeon.gas`) that provides the
movement and locomotion layer. Kept separate from the GAS core on purpose: movement is a
*consumer* of the GameplayTag state bus, not part of the ability system itself — so you can
use Sigil core with your own movement, or this package with Sigil.

[Sigil](https://github.com/forestlii/sigil-gas)（`com.likeon.gas`）的**移动配套包**。刻意与 GAS 核心分开：移动是
GameplayTag 状态总线的*消费方*，不属于能力系统本身——你可以只用核心配自己的移动，也可以二者搭配。

- **Depends on / 依赖:** `com.likeon.gas` (Sigil core) + `com.unity.inputsystem` (via core)
- **Namespace / 命名空间:** `Likeon.GAS`（与核心一致，不破坏 `using`）
- **Engine / 引擎:** Unity 6 (6000.4)
- **License / 许可:** MIT

## Install / 安装

This package depends on the Sigil core package. Install both:
本包依赖 Sigil 核心包，两者一起装：

1. Add `com.likeon.gas` (Sigil core) first. / 先装核心包。
2. Add `com.likeon.gas.movement` (this package). / 再装本包。

(Package Manager → *Add package from disk…* → each `package.json`.)

### Running tests / 运行测试

The package ships with EditMode + PlayMode tests under `Tests/`. To run them, add the
package to `"testables"` in your project's `Packages/manifest.json`, then open
**Window → General → Test Runner**:

```json
"testables": [ "com.likeon.gas.movement" ]
```

包内 `Tests/` 自带 EditMode + PlayMode 测试。在工程 `Packages/manifest.json` 的
`"testables"` 里加上本包，再打开 **Window → General → Test Runner** 即可运行。

## Features / 功能

- **MovementSystemComponent** — movement-set / state machine, definition stack, rotation modes, input direction. / 移动状态机、定义栈、旋转模式、输入方向。
- **CharacterMovementSystemComponent** — actual movement on a `CharacterController`. / 基于 CharacterController 的实际移动。
- **State bus to the ability system** — movement state mirrors onto the ASC as loose tags (drives ability gating, e.g. sprint → slide). / 移动状态镜像到 ASC（驱动技能门控，如冲刺→滑铲）。
- **Locomotion animation driver** — `LocomotionAnimationDriver` + `LocomotionMath`: speed, local-velocity yaw, 4-/8-way direction (dead-zone hysteresis), lean, in-air state (jump apex / falling time / just-landed / ground prediction), view-relative aim offset, core-state tags → Animator parameters. / 运动动画驱动：速度/偏航/四八向/倾身/空中态/视角 AimOffset/核心状态标签 → Animator。
- **Sample layered Animator Controller generator** — `Likeon ▸ GAS ▸ Samples` builds a controller matching the driver (8-way blend tree + jump/fall + upper-body aim-offset layer). / 示例分层 Controller 生成器（菜单一键生成）。

The driver writes Animator parameters; final animation clips & feel are the host project's.
运动层驱动 Animator 参数，成品动画与手感由宿主提供。

## License / 许可

[MIT](LICENSE.md) — free for any use including commercial, just keep the copyright notice.
/ [MIT 许可证](LICENSE.md) — 免费用于任何用途（含商用），保留版权声明即可。
