// EditMode 测试：LocomotionMath 纯数学（偏航角 / 四向 / 八向 / 死区滞回 / 倾身）。
using NUnit.Framework;
using UnityEngine;
using Likeon.GAS;

namespace Likeon.GAS.Tests
{
    public class LocomotionEditTests
    {
        private static readonly Quaternion FaceForward = Quaternion.identity; // 前=+Z

        // ===== 偏航角 =====
        [Test]
        public void VelocityYawAngle_CardinalDirections()
        {
            Assert.AreEqual(0f, LocomotionMath.VelocityYawAngle(Vector3.forward, FaceForward), 0.01f, "正前=0");
            Assert.AreEqual(90f, LocomotionMath.VelocityYawAngle(Vector3.right, FaceForward), 0.01f, "正右=+90");
            Assert.AreEqual(-90f, LocomotionMath.VelocityYawAngle(Vector3.left, FaceForward), 0.01f, "正左=-90");
            Assert.AreEqual(180f, Mathf.Abs(LocomotionMath.VelocityYawAngle(Vector3.back, FaceForward)), 0.01f, "正后=±180");
            Assert.AreEqual(0f, LocomotionMath.VelocityYawAngle(Vector3.zero, FaceForward), 0.01f, "零速=0");
        }

        // ===== 四向基本 =====
        [Test]
        public void SelectCardinal_Basic()
        {
            Assert.AreEqual(EMovementDirection.Forward, LocomotionMath.SelectCardinal(0f, 10f, EMovementDirection.Forward, false));
            Assert.AreEqual(EMovementDirection.Right, LocomotionMath.SelectCardinal(90f, 10f, EMovementDirection.Forward, false));
            Assert.AreEqual(EMovementDirection.Left, LocomotionMath.SelectCardinal(-90f, 10f, EMovementDirection.Forward, false));
            Assert.AreEqual(EMovementDirection.Backward, LocomotionMath.SelectCardinal(180f, 10f, EMovementDirection.Forward, false));
        }

        // ===== 四向死区滞回（同一角度，按当前方向粘住）=====
        [Test]
        public void SelectCardinal_DeadZoneHysteresis()
        {
            // 60°：在前后死区翻倍范围内/外的临界
            // 当前=Forward → 死区翻倍(45+20=65)，60≤65 → 仍 Forward（粘住）
            Assert.AreEqual(EMovementDirection.Forward,
                LocomotionMath.SelectCardinal(60f, 10f, EMovementDirection.Forward, true), "Forward 应粘住");
            // 当前=Right → 不翻倍(45+10=55)，60>55 → 落到 Right
            Assert.AreEqual(EMovementDirection.Right,
                LocomotionMath.SelectCardinal(60f, 10f, EMovementDirection.Right, true), "Right 应落到 Right");
            // 不启用滞回时，60° 一律按 55 边界 → Right
            Assert.AreEqual(EMovementDirection.Right,
                LocomotionMath.SelectCardinal(60f, 10f, EMovementDirection.Forward, false), "无滞回按固定边界");
        }

        // ===== 八向 =====
        [Test]
        public void SelectOctagonal_Sectors()
        {
            Assert.AreEqual(EMovementDirection8.Forward, LocomotionMath.SelectOctagonal(0f, 10f, EMovementDirection8.Forward, false));
            Assert.AreEqual(EMovementDirection8.ForwardRight, LocomotionMath.SelectOctagonal(45f, 10f, EMovementDirection8.Forward, false));
            Assert.AreEqual(EMovementDirection8.Right, LocomotionMath.SelectOctagonal(90f, 10f, EMovementDirection8.Forward, false));
            Assert.AreEqual(EMovementDirection8.BackwardRight, LocomotionMath.SelectOctagonal(135f, 10f, EMovementDirection8.Forward, false));
            Assert.AreEqual(EMovementDirection8.Backward, LocomotionMath.SelectOctagonal(180f, 10f, EMovementDirection8.Forward, false));
            Assert.AreEqual(EMovementDirection8.ForwardLeft, LocomotionMath.SelectOctagonal(-45f, 10f, EMovementDirection8.Forward, false));
            Assert.AreEqual(EMovementDirection8.Left, LocomotionMath.SelectOctagonal(-90f, 10f, EMovementDirection8.Forward, false));
            Assert.AreEqual(EMovementDirection8.BackwardLeft, LocomotionMath.SelectOctagonal(-135f, 10f, EMovementDirection8.Forward, false));
        }

        // ===== 倾身 =====
        [Test]
        public void RelativeAccelerationAmount_ForwardSidewaysBraking()
        {
            // 前向加速 → 前倾>0
            var fwd = LocomotionMath.RelativeAccelerationAmount(new Vector3(0, 0, 10), new Vector3(0, 0, 5), FaceForward, 12f, 20f);
            Assert.Greater(fwd.x, 0f, "前向加速应前倾");
            Assert.AreEqual(0f, fwd.y, 0.01f);

            // 侧向加速 → 右倾
            var side = LocomotionMath.RelativeAccelerationAmount(new Vector3(10, 0, 0), new Vector3(0, 0, 5), FaceForward, 12f, 20f);
            Assert.Greater(side.y, 0f, "右向加速应右倾");

            // 减速（加速度与速度反向）→ 后倾，且用 maxBraking 归一化
            var brake = LocomotionMath.RelativeAccelerationAmount(new Vector3(0, 0, -10), new Vector3(0, 0, 5), FaceForward, 12f, 20f);
            Assert.Less(brake.x, 0f, "减速应后倾");
            Assert.AreEqual(-0.5f, brake.x, 0.01f, "应按 maxBraking=20 归一化：-10/20=-0.5");
        }

        // ===== View / AimOffset =====
        [Test]
        public void ViewYawAngle_RelativeToFacing()
        {
            Assert.AreEqual(0f, LocomotionMath.ViewYawAngle(Quaternion.identity, FaceForward), 0.01f, "同向=0");
            Assert.AreEqual(90f, LocomotionMath.ViewYawAngle(Quaternion.Euler(0, 90, 0), FaceForward), 0.01f, "右看=+90");
            Assert.AreEqual(-90f, LocomotionMath.ViewYawAngle(Quaternion.Euler(0, -90, 0), FaceForward), 0.01f, "左看=-90");
        }

        [Test]
        public void ViewPitchAngle_UpDownLevel()
        {
            // 容差 0.1°：Euler→Quaternion→asin 的 float 精度链累积约 0.02°，不写死过紧（防 false RED）。
            Assert.AreEqual(0f, LocomotionMath.ViewPitchAngle(Quaternion.identity), 0.1f, "平视=0");
            Assert.AreEqual(90f, LocomotionMath.ViewPitchAngle(Quaternion.Euler(-90, 0, 0)), 0.1f, "上看=+90");
            Assert.AreEqual(-90f, LocomotionMath.ViewPitchAngle(Quaternion.Euler(90, 0, 0)), 0.1f, "下看=-90");
        }

        [Test]
        public void PitchToAmount_Mapping()
        {
            Assert.AreEqual(0.5f, LocomotionMath.PitchToAmount(0f), 0.001f, "平视→0.5");
            Assert.AreEqual(0f, LocomotionMath.PitchToAmount(90f), 0.001f, "上看→0");
            Assert.AreEqual(1f, LocomotionMath.PitchToAmount(-90f), 0.001f, "下看→1");
        }

        // ===== ToLocalPlanar / Opposite =====
        [Test]
        public void ToLocalPlanar_StripsVerticalIdentity()
        {
            var local = LocomotionMath.ToLocalPlanar(new Vector3(3, 7, 4), FaceForward);
            Assert.AreEqual(3f, local.x, 0.01f);
            Assert.AreEqual(0f, local.y, 0.01f, "竖直分量清零");
            Assert.AreEqual(4f, local.z, 0.01f);
        }

        [Test]
        public void Opposite_AllDirections()
        {
            Assert.AreEqual(EMovementDirection.Backward, LocomotionMath.Opposite(EMovementDirection.Forward));
            Assert.AreEqual(EMovementDirection.Forward, LocomotionMath.Opposite(EMovementDirection.Backward));
            Assert.AreEqual(EMovementDirection.Right, LocomotionMath.Opposite(EMovementDirection.Left));
            Assert.AreEqual(EMovementDirection.Left, LocomotionMath.Opposite(EMovementDirection.Right));
        }

        // ===== 空中倾身 =====
        [Test]
        public void InAirLeanAmount_DirectionAndReference()
        {
            // 前向速度 → 前倾 x>0；refSpeed=5、mult=1 → 10/5=2
            var fwd = LocomotionMath.InAirLeanAmount(new Vector3(0, 0, 10), FaceForward, 5f, 1f);
            Assert.AreEqual(2f, fwd.x, 0.01f, "前后量=vz/ref");
            Assert.AreEqual(0f, fwd.y, 0.01f);
            // 侧向速度 → 右倾 y>0
            var side = LocomotionMath.InAirLeanAmount(new Vector3(10, 0, 0), FaceForward, 5f, 1f);
            Assert.AreEqual(2f, side.y, 0.01f);
            // 竖直乘子取负 → 反向（上升↔下落平滑反向）
            var inv = LocomotionMath.InAirLeanAmount(new Vector3(0, 0, 10), FaceForward, 5f, -1f);
            Assert.AreEqual(-2f, inv.x, 0.01f, "mult<0 反向");
            // refSpeed<=0 → zero（除零保护）
            Assert.AreEqual(Vector2.zero, LocomotionMath.InAirLeanAmount(new Vector3(0, 0, 10), FaceForward, 0f, 1f));
        }

        // ===== 着地预测 sweep =====
        [Test]
        public void GroundPredictionSweep_ThresholdAndScaling()
        {
            // 慢于阈值（vs>-2）→ 不预测
            Assert.AreEqual(Vector3.zero, LocomotionMath.GroundPredictionSweep(new Vector3(0, -1, 0), -1f, 1f), "慢于阈值不预测");
            // 下落 → 非零、方向向下
            var s = LocomotionMath.GroundPredictionSweep(new Vector3(0, -10, 0), -10f, 1f);
            Assert.Greater(s.magnitude, 0f, "下落应有 sweep");
            Assert.Less(s.y, 0f, "方向向下");
            // 越快下落越远
            var slow = LocomotionMath.GroundPredictionSweep(new Vector3(0, -5, 0), -5f, 1f);
            var fast = LocomotionMath.GroundPredictionSweep(new Vector3(0, -40, 0), -40f, 1f);
            Assert.Greater(fast.magnitude, slow.magnitude, "越快越远");
            // 缩放：scale=2 长度约翻倍
            var s2 = LocomotionMath.GroundPredictionSweep(new Vector3(0, -10, 0), -10f, 2f);
            Assert.AreEqual(s.magnitude * 2f, s2.magnitude, 0.01f, "长度随 scale 线性");
        }
    }
}
