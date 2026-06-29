# Sigil Movement — 移动与运动动画配套包

[English](README.md) | [简体中文](README.zh-CN.md)

[Sigil](https://github.com/forestlii/sigil-gas)（`com.likeon.gas`）的**移动配套包**，提供移动与运动动画层。
刻意与 GAS 核心分开：移动是 GameplayTag 状态总线的*消费方*，不属于能力系统本身——你可以只用核心配
自己的移动，也可以二者搭配。

- **依赖：** `com.likeon.gas`（Sigil 核心）+ `com.unity.inputsystem`（经核心）
- **命名空间：** `Likeon.GAS`（与核心一致，不破坏 `using`）
- **引擎：** Unity 6（6000.4）
- **许可：** MIT

## 安装

本包依赖 Sigil 核心包，两者一起装：

1. 先装 `com.likeon.gas`（Sigil 核心）。
2. 再装 `com.likeon.gas.movement`（本包）。

（Package Manager → *Add package from disk…* → 各自的 `package.json`。）

### 运行测试

包内 `Tests/` 自带 EditMode + PlayMode 测试。在工程 `Packages/manifest.json` 的
`"testables"` 里加上本包，再打开 **Window → General → Test Runner** 即可运行：

```json
"testables": [ "com.likeon.gas.movement" ]
```

## 功能

- **MovementSystemComponent** — 移动集 / 状态机、定义栈、旋转模式、输入方向。
- **CharacterMovementSystemComponent** — 基于 `CharacterController` 的实际移动。
- **状态总线接入能力系统** — 移动状态以松散标签镜像到 ASC（驱动技能门控，如 冲刺 → 滑铲）。
- **运动动画驱动** — `LocomotionAnimationDriver` + `LocomotionMath`：速度、局部速度偏航、四/八向（死区滞回）、倾身、空中状态（跳跃顶点 / 下落时间 / 刚着地 / 着地预测）、视角相对 aim offset、核心状态标签 → Animator 参数。
- **示例分层 Animator Controller 生成器** — 菜单 `Likeon ▸ GAS ▸ Samples` 一键生成与驱动匹配的 Controller（八向 blend tree + 跳跃/下落 + 上半身 aim-offset 层）。

运动层驱动 Animator 参数，成品动画与手感由宿主工程提供。

## 许可

[MIT](LICENSE.md) — 免费用于任何用途（含商用），保留版权声明即可。
