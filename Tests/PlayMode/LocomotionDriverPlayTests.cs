// PlayMode 测试：LocomotionAnimationDriver 的状态机（spec §3）。
// driver 是 MonoBehaviour，其 Update() 每帧会用零速度自动调 UpdateState 覆盖手动值——
// 故测试 enabled=false 禁 Update，只手动调 public 方法（UpdateState/UpdateView/UpdateCoreState）验证。
//  A) 上升 → Jumping + TimeToJumpApex>0；B) 下落 → Falling + FallingTime 累加；
//  C) 落地帧 JustLanded 仅一帧；D) UpdateView yaw/pitchAmount；
//  E) UpdateCoreState changed 标志（首帧不算变化/改一个/不变帧）；
//  F) 着地预测默认不启用 → false/-1；G) 地面前向加速 → 前倾 Lean.x>0。
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Likeon.GAS;

namespace Likeon.GAS.PlayTests
{
    public class LocomotionDriverPlayTests
    {
        private static GameplayTag Tag(string s) => GameplayTag.RequestTag(s);
        private readonly List<GameObject> _spawned = new List<GameObject>();

        [TearDown]
        public void Cleanup()
        {
            foreach (var go in _spawned) if (go != null) Object.Destroy(go);
            _spawned.Clear();
        }

        private LocomotionAnimationDriver NewDriver()
        {
            var go = new GameObject("Loco"); _spawned.Add(go);
            var d = go.AddComponent<LocomotionAnimationDriver>();
            d.enabled = false; // 禁 Update 自动驱动，测试只手动调 public 方法
            return d;
        }

        // ============ A) 上升 ============
        [UnityTest]
        public IEnumerator A_Ascending_SetsJumping()
        {
            var d = NewDriver();
            d.UpdateState(new Vector3(0, 5f, 0), Quaternion.identity, grounded: false, hasInput: false, Vector3.zero, 0.1f);
            Assert.IsTrue(d.State.Jumping, "上升应 Jumping");
            Assert.IsFalse(d.State.Falling);
            Assert.Greater(d.State.TimeToJumpApex, 0f, "上升中到顶点剩余时间>0");
            Assert.AreEqual(0f, d.State.FallingTime, "上升中 FallingTime=0");
            yield return null;
        }

        // ============ B) 下落 + FallingTime 累加 ============
        [UnityTest]
        public IEnumerator B_Falling_AccumulatesTime()
        {
            var d = NewDriver();
            d.UpdateState(new Vector3(0, -5f, 0), Quaternion.identity, false, false, Vector3.zero, 0.1f);
            Assert.IsTrue(d.State.Falling, "下落应 Falling");
            Assert.IsFalse(d.State.Jumping);
            float t1 = d.State.FallingTime;
            d.UpdateState(new Vector3(0, -5f, 0), Quaternion.identity, false, false, Vector3.zero, 0.1f);
            Assert.Greater(d.State.FallingTime, t1, "FallingTime 应逐帧累加");
            yield return null;
        }

        // ============ C) 落地帧 JustLanded 仅一帧 ============
        [UnityTest]
        public IEnumerator C_JustLanded_OneFrameOnly()
        {
            var d = NewDriver();
            d.UpdateState(new Vector3(0, -5f, 0), Quaternion.identity, grounded: false, false, Vector3.zero, 0.1f); // 空中
            d.UpdateState(Vector3.zero, Quaternion.identity, grounded: true, false, Vector3.zero, 0.1f);             // 落地帧
            Assert.IsTrue(d.State.JustLanded, "落地帧 JustLanded=true");
            d.UpdateState(Vector3.zero, Quaternion.identity, grounded: true, false, Vector3.zero, 0.1f);             // 下一帧
            Assert.IsFalse(d.State.JustLanded, "JustLanded 仅维持一帧");
            yield return null;
        }

        // ============ D) UpdateView ============
        [UnityTest]
        public IEnumerator D_UpdateView_YawAndPitchAmount()
        {
            var d = NewDriver();
            d.UpdateView(Quaternion.Euler(0, 90f, 0), 0.1f); // 视角朝右
            Assert.Greater(d.State.ViewYawAngle, 0f, "右看 ViewYaw>0");
            Assert.GreaterOrEqual(d.State.ViewPitchAmount, 0f);
            Assert.LessOrEqual(d.State.ViewPitchAmount, 1f, "PitchAmount∈[0,1]");
            yield return null;
        }

        // ============ E) UpdateCoreState changed 标志 ============
        [UnityTest]
        public IEnumerator E_UpdateCoreState_ChangedFlags()
        {
            var d = NewDriver();
            var loco = Tag("Movement.Locomotion.Grounded");
            var rot = Tag("Movement.Rotation.VelocityDirection");
            var set = Tag("Movement.Set.Default");
            var jog = Tag("Movement.State.Jog");
            var sprint = Tag("Movement.State.Sprint");

            d.UpdateCoreState(loco, rot, jog, set); // 首帧
            Assert.IsFalse(d.CoreState.HasAnyChange, "首帧不算变化");

            d.UpdateCoreState(loco, rot, sprint, set); // 仅改 movementState
            Assert.IsTrue(d.CoreState.MovementStateChanged, "改 MovementState 应 changed");
            Assert.IsTrue(d.CoreState.HasAnyChange);
            Assert.IsFalse(d.CoreState.LocomotionModeChanged, "未改的不应 changed");

            d.UpdateCoreState(loco, rot, sprint, set); // 不变帧
            Assert.IsFalse(d.CoreState.HasAnyChange, "不变帧全 false");
            yield return null;
        }

        // ============ F) 着地预测默认不启用 ============
        [UnityTest]
        public IEnumerator F_GroundPrediction_DisabledByDefault()
        {
            var d = NewDriver();
            d.UpdateState(new Vector3(0, -10f, 0), Quaternion.identity, false, false, Vector3.zero, 0.1f);
            Assert.IsFalse(d.State.HasPredictedGround, "默认不启用预测");
            Assert.AreEqual(-1f, d.State.GroundDistance, "未预测 GroundDistance=-1");
            yield return null;
        }

        // ============ G) 地面前向加速 → 前倾 ============
        [UnityTest]
        public IEnumerator G_GroundLean_FollowsForwardAcceleration()
        {
            var d = NewDriver();
            // 第一帧建立 prevVelocity，之后持续前向加速（速度递增）多帧驱动平滑倾身
            d.UpdateState(new Vector3(0, 0, 1f), Quaternion.identity, grounded: true, hasInput: true, Vector3.zero, 0.1f);
            for (int i = 2; i <= 12; i++)
                d.UpdateState(new Vector3(0, 0, i), Quaternion.identity, grounded: true, hasInput: true, Vector3.zero, 0.1f);
            Assert.Greater(d.State.Lean.x, 0f, "持续前向加速应前倾 Lean.x>0");
            yield return null;
        }
    }
}
