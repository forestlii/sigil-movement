# Sigil Movement · 移动 Demo

[English](README.md) | [简体中文](README.zh-CN.md)

一个最小移动 demo：第三人称角色，用 **WASD 移动 + 鼠标看**，输入经 GAS 输入系统
（`InputProcessor_Move` / `InputProcessor_Look` → `CharacterMovementSystemComponent` + 第三人称相机）。

## 怎么跑

本示例随包发**烘好的场景 + 玩家 prefab**——直接：

1. 打开**本文件夹里的 `MovementDemo.unity`**。
2. 按 **Play**。
3. 操作：**WASD** 移动 · **鼠标** 看 · **1 / 2 / 3** 走 / 跑 / 冲刺 · **Esc** 释放鼠标。

### 重新烘（可选）

场景、`Resources/` 下的 `MovementDemoPlayer` prefab、以及三个配置资产（`MovementDemo_InputConfig`、
`MovementDemo_InputControlSetup`、`MovementDemo_MovementDef`）由编辑器脚本生成。想重新生成（比如你改了
`.inputactions`），跑菜单 **Likeon ▸ GAS ▸ Samples ▸ Build Movement Demo Scene**（幂等；Console 会打印输出路径）。

## 它演示什么

生成器把整条输入链接好，生成出来的资产就是一份活样板：

- `MovementDemoControls.inputactions` —— Unity 输入系统动作（**Move** / **Look**）。
- `MovementDemo_InputConfig` —— `InputActionMapping` 把 `InputTag.Move` / `InputTag.Look` 绑到上面那些动作；
  `InputSystemComponent` 启用时自动订阅（`.inputactions` → `InputConfig` 那条路）。
- `MovementDemo_InputControlSetup` —— 一个 `InputProcessor_Move`（相机相对）+ 一个 `InputProcessor_Look`。
- 玩家 = `CharacterController` + `AbilitySystemComponent` + `CharacterMovementSystemComponent` +
  `InputSystemComponent`，由第三人称 `CameraSystemComponent` 跟随。

> 依赖核心包 `com.likeon.gas`。程序员美术白模（地面 + 胶囊体）；真实手感需你自己接动画。
