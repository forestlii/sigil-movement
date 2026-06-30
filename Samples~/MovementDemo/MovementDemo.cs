// Copyright (c) 2026 Likeon. Licensed under the MIT License.
// Movement 包示例：演示"输入经 GIPS 输入系统 → 移动/相机"的完整链路。
// 场景由 MovementDemoBuilder 生成；这里只做小事：锁鼠标 + 走/跑/冲刺切换 + 屏幕提示。
// Move/Look 不在此处理——它们由场景里 InputControlSetup 上的 InputProcessor_Move/Look 驱动。

using UnityEngine;
using UnityEngine.InputSystem;

namespace Likeon.GAS.Sample.MovementDemo
{
    [AddComponentMenu("Likeon/GAS/Samples/Movement Demo")]
    public class MovementDemo : MonoBehaviour
    {
        [Tooltip("被驱动的角色移动组件（builder 接好）")]
        public CharacterMovementSystemComponent Mover;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || Mover == null) return;

            // 1/2/3 切走 / 跑 / 冲刺（演示便利，直接设 desired state）
            if (kb.digit1Key.wasPressedThisFrame) Mover.SetDesiredMovement(MovementTags.MovementState_Walk);
            if (kb.digit2Key.wasPressedThisFrame) Mover.SetDesiredMovement(MovementTags.MovementState_Jog);
            if (kb.digit3Key.wasPressedThisFrame) Mover.SetDesiredMovement(MovementTags.MovementState_Sprint);

            // Esc 释放鼠标
            if (kb.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true };
            string state = Mover != null ? Mover.GetMovementState().TagName : "-";
            float speed = Mover is CharacterMovementSystemComponent c ? c.CurrentSpeed : 0f;

            GUILayout.BeginArea(new Rect(12, 12, 460, 160), GUI.skin.box);
            GUILayout.Label("<b>Sigil Movement Demo</b>  —  输入经 GIPS 输入系统驱动移动/相机", style);
            GUILayout.Label("<b>WASD</b> 移动（经 InputProcessor_Move） · <b>鼠标</b> 转视角（经 InputProcessor_Look）", style);
            GUILayout.Label("<b>1 / 2 / 3</b> 走 / 跑 / 冲刺 · <b>Esc</b> 释放鼠标", style);
            GUILayout.Label($"当前移动状态：<b>{state}</b>　速度：<b>{speed:0.0}</b>", style);
            GUILayout.EndArea();
        }
    }
}
