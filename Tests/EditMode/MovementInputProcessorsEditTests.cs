// EditMode 测试：#3 移动/视角输入处理器（GIPS 输入 → movement 移动 + 相机）。
using NUnit.Framework;
using UnityEngine;
using Likeon.GAS;

namespace Likeon.GAS.Tests
{
    public class MovementInputProcessorsEditTests
    {
        private static GameplayTag T(string s) => GameplayTag.RequestTag(s);

        [Test]
        public void InputProcessor_Move_FeedsMovementInputDirection()
        {
            var go = new GameObject("Mover");
            var ic = go.AddComponent<InputSystemComponent>();
            var move = go.AddComponent<MovementSystemComponent>();

            var proc = new InputProcessor_Move { CameraRelative = false }; // 相对自身朝向，确定性

            // 前向输入（axis.y=1）→ 世界 +Z（新建物体 transform.forward）
            proc.HandleInput(ic, new InputActionData(new Vector2(0f, 1f)), T("InputTag.Move"), InputTriggerEvent.Triggered);
            Assert.Greater(move.GetInputDirection().z, 0.5f, "前向输入应喂成 +Z 世界方向");

            // 松开 → 置零
            proc.HandleInput(ic, InputActionData.Empty, T("InputTag.Move"), InputTriggerEvent.Completed);
            Assert.Less(move.GetInputDirection().magnitude, 0.01f, "松开应清零输入方向");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void InputProcessor_Look_DrivesThirdPersonCamera()
        {
            var go = new GameObject("Player");
            var ic = go.AddComponent<InputSystemComponent>();

            var camGo = new GameObject("CameraRig");
            var camSys = camGo.AddComponent<CameraSystemComponent>();
            var tp = (ThirdPersonCameraBehavior)camSys.DefaultBehavior; // 默认即第三人称
            tp.OnEnter();

            var target = new GameObject("Target").transform;
            tp.Tick(target, 0.1f);
            var before = tp.View.Position;

            // 视角输入：偏航 90°
            var look = new InputProcessor_Look { Sensitivity = 1f };
            look.HandleInput(ic, new InputActionData(new Vector2(90f, 0f)), T("InputTag.Look"), InputTriggerEvent.Triggered);
            tp.Tick(target, 0.1f);
            var after = tp.View.Position;

            Assert.AreNotEqual(before, after, "视角输入后第三人称相机眼位应改变（绕偏航旋转）");

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(camGo);
            Object.DestroyImmediate(target.gameObject);
        }
    }
}
