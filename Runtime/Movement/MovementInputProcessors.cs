// Copyright (c) 2026 Likeon. Licensed under the MIT License.
// 移动 / 视角 输入处理器：把 GIPS 输入系统（核心包 InputSystemComponent + InputProcessor）
// 的 2D 轴输入桥接到 movement 包的移动系统与相机——让移动/视角输入也走输入系统（而非旁路）。
//
// 设计：core 不依赖 movement，这两个处理器放 movement 包（依赖 core，可同时引用
// core 的 InputProcessor/CameraSystemComponent 与本包的 MovementSystemComponent）。
// 用法：InputConfig 里把 Move/Look 的 InputTag 配上，InputControlSetup 挂这两个处理器即可。

using UnityEngine;

namespace Likeon.GAS
{
    /// <summary>
    /// 移动输入处理器：读 2D 移动轴 → 按相机偏航转世界方向 → 喂 <see cref="MovementSystemComponent.SetInputDirection"/>。
    /// 默认监听 Triggered/Ongoing（持续移动），松开（Completed/Canceled）置零。
    /// </summary>
    [System.Serializable]
    public sealed class InputProcessor_Move : InputProcessor
    {
        [Tooltip("相机相对移动（WASD 相对相机朝向）。关闭则相对角色自身朝向")]
        public bool CameraRelative = true;

        protected override void HandleInputTriggered(InputSystemComponent ic, InputActionData data, GameplayTag inputTag) => Apply(ic, data);
        protected override void HandleInputOngoing(InputSystemComponent ic, InputActionData data, GameplayTag inputTag) => Apply(ic, data);
        protected override void HandleInputStarted(InputSystemComponent ic, InputActionData data, GameplayTag inputTag) => Apply(ic, data);
        protected override void HandleInputCompleted(InputSystemComponent ic, InputActionData data, GameplayTag inputTag) => Stop(ic);
        protected override void HandleInputCanceled(InputSystemComponent ic, InputActionData data, GameplayTag inputTag) => Stop(ic);

        private void Apply(InputSystemComponent ic, InputActionData data)
        {
            if (ic == null) return;
            var move = ic.GetComponent<MovementSystemComponent>();
            if (move == null) return;

            Vector2 axis = data.Value;
            Vector3 fwd, right;
            var cam = CameraRelative && Camera.main != null ? Camera.main.transform : null;
            if (cam != null) { fwd = Flatten(cam.forward); right = Flatten(cam.right); }
            else { fwd = Flatten(move.transform.forward); right = Flatten(move.transform.right); }

            Vector3 world = right * axis.x + fwd * axis.y;
            move.SetInputDirection(world);
        }

        private static void Stop(InputSystemComponent ic)
            => ic?.GetComponent<MovementSystemComponent>()?.SetInputDirection(Vector3.zero);

        private static Vector3 Flatten(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-6f ? v.normalized : v; }
    }

    /// <summary>
    /// 视角输入处理器：读 2D 视角轴（鼠标/右摇杆）→ 驱动第三人称相机的 <see cref="ThirdPersonCameraBehavior.AddLookInput"/>。
    /// 相机经 ViewDirection 旋转模式再带动角色朝向。
    /// </summary>
    [System.Serializable]
    public sealed class InputProcessor_Look : InputProcessor
    {
        [Tooltip("视角灵敏度")]
        public float Sensitivity = 1f;
        [Tooltip("反转 Y 轴")]
        public bool InvertY = false;

        protected override void HandleInputTriggered(InputSystemComponent ic, InputActionData data, GameplayTag inputTag) => Apply(ic, data);
        protected override void HandleInputOngoing(InputSystemComponent ic, InputActionData data, GameplayTag inputTag) => Apply(ic, data);
        protected override void HandleInputStarted(InputSystemComponent ic, InputActionData data, GameplayTag inputTag) => Apply(ic, data);

        private void Apply(InputSystemComponent ic, InputActionData data)
        {
            if (ic == null) return;
            var tp = ResolveThirdPerson(ic);
            if (tp == null) return;

            Vector2 axis = data.Value;
            float yaw = axis.x * Sensitivity;
            float pitch = (InvertY ? 1f : -1f) * axis.y * Sensitivity;
            tp.AddLookInput(yaw, pitch);
        }

        // 取当前生效的第三人称相机行为：优先相机系统栈顶，其次默认行为。
        private static ThirdPersonCameraBehavior ResolveThirdPerson(InputSystemComponent ic)
        {
            var camSys = ic.GetComponentInChildren<CameraSystemComponent>();
            if (camSys == null) camSys = Object.FindObjectOfType<CameraSystemComponent>();
            if (camSys == null) return null;
            return (camSys.Stack.Top as ThirdPersonCameraBehavior) ?? (camSys.DefaultBehavior as ThirdPersonCameraBehavior);
        }
    }
}
