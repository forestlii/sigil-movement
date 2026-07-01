# Sigil Movement · 移动 Demo

[English](README.md) | [简体中文](README.zh-CN.md)

一个最小移动 demo：第三人称角色，用 **WASD 移动 + 鼠标看**，输入经 GAS 输入系统
（`InputProcessor_Move` / `InputProcessor_Look` → `CharacterMovementSystemComponent` + 第三人称相机）。

## 怎么跑

本示例是**生成器型**——只随包发脚本 + 一个 `.inputactions` 资产，场景按需生成，
**跑生成器之前没有现成的场景文件**：

1. 菜单 **Likeon ▸ GAS ▸ Samples ▸ Build Movement Demo Scene**。
2. 它会在**本文件夹里**生成 **`MovementDemo.unity`** 和三个资产——`MovementDemo_InputConfig`、
   `MovementDemo_InputControlSetup`、`MovementDemo_MovementDef`（Console 会打印确切路径：
   `[MovementDemo] 已生成场景与资产：…`）。
3. 打开生成的 **`MovementDemo.unity`** 按 **Play**。
4. 操作：**WASD** 移动 · **鼠标** 看。（走 / 跑 / 冲刺速度在生成的 `MovementDef` 里。）

## 它演示什么

生成器把整条输入链接好，生成出来的资产就是一份活样板：

- `MovementDemoControls.inputactions` —— Unity 输入系统动作（**Move** / **Look**）。
- `MovementDemo_InputConfig` —— `InputActionMapping` 把 `InputTag.Move` / `InputTag.Look` 绑到上面那些动作；
  `InputSystemComponent` 启用时自动订阅（`.inputactions` → `InputConfig` 那条路）。
- `MovementDemo_InputControlSetup` —— 一个 `InputProcessor_Move`（相机相对）+ 一个 `InputProcessor_Look`。
- 玩家 = `CharacterController` + `AbilitySystemComponent` + `CharacterMovementSystemComponent` +
  `InputSystemComponent`，由第三人称 `CameraSystemComponent` 跟随。

> 依赖核心包 `com.likeon.gas`。程序员美术白模（地面 + 胶囊体）；真实手感需你自己接动画。
