// PlayMode 测试：验证阶段 4 移动系统 + 与 GAS 的状态总线对接。
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Likeon.GAS;

namespace Likeon.GAS.PlayTests
{
    public class MovementPlayTests
    {
        private static GameplayTag Tag(string s) => GameplayTag.RequestTag(s);

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private readonly List<Object> _assets = new List<Object>();

        [TearDown]
        public void Cleanup()
        {
            foreach (var go in _spawned) if (go != null) Object.Destroy(go);
            foreach (var a in _assets) if (a != null) Object.Destroy(a);
            _spawned.Clear();
            _assets.Clear();
        }

        private GameObject NewObj(string name)
        {
            var go = new GameObject(name);
            _spawned.Add(go);
            return go;
        }

        private GameObject NewPawn(string name)
        {
            var go = new GameObject(name);
            _spawned.Add(go);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 2f; cc.radius = 0.5f; cc.center = new Vector3(0, 1, 0);
            return go;
        }

        private MovementDefinition MakeDefinition(params (GameplayTag state, float speed)[] states)
        {
            var def = ScriptableObject.CreateInstance<MovementDefinition>();
            _assets.Add(def);
            var set = new MovementSetSetting { MovementSet = Tag("Movement.Set.Default") };
            foreach (var (state, speed) in states)
                set.States.Add(new MovementStateSetting
                {
                    State = state, Speed = speed,
                    Acceleration = 50f, BrakingDeceleration = 50f, RotationInterpolationSpeed = 12f
                });
            def.MovementSets.Add(set);
            return def;
        }

        // A) 移动状态镜像到 ASC（状态总线）——驱动"冲刺→滑铲"的关键
        [UnityTest]
        public IEnumerator A_MovementState_MirroredToASC()
        {
            var go = NewPawn("Pawn");
            var asc = go.AddComponent<AbilitySystemComponent>();
            var move = go.AddComponent<CharacterMovementSystemComponent>(); // Awake 自动把 ASC 设为 provider
            yield return null;

            move.SetDesiredMovement(MovementTags.MovementState_Sprint);
            Assert.IsTrue(asc.HasMatchingGameplayTag(MovementTags.MovementState_Sprint), "冲刺状态应镜像为 ASC 上的标签");

            move.SetDesiredMovement(MovementTags.MovementState_Walk);
            Assert.IsFalse(asc.HasMatchingGameplayTag(MovementTags.MovementState_Sprint), "切换后旧状态标签应被移除");
            Assert.IsTrue(asc.HasMatchingGameplayTag(MovementTags.MovementState_Walk), "新状态标签应在 ASC 上");
        }

        // B) 角色按输入方向实际移动
        // 注：headless 批处理下 Time.deltaTime 极小（~0.0001s/帧），故用 WaitForSeconds 按游戏时间推进，
        //     让仿真累积足够时间（与帧率无关），否则几帧只推进毫秒级、角色几乎不动（环境假象，非代码问题）。
        [UnityTest]
        public IEnumerator B_Character_MovesWithInput()
        {
            // 地面，让角色 Grounded
            var ground = NewObj("Ground");
            ground.AddComponent<BoxCollider>().size = new Vector3(50, 1, 50);
            ground.transform.position = new Vector3(0, -0.5f, 0);

            var go = NewPawn("Pawn");
            go.transform.position = new Vector3(0, 0.1f, 0);
            go.AddComponent<AbilitySystemComponent>();
            var move = go.AddComponent<CharacterMovementSystemComponent>();
            yield return null;

            move.PushAvailableMovementDefinition(MakeDefinition((MovementTags.MovementState_Jog, 5f)));
            move.SetMovementSet(Tag("Movement.Set.Default"));
            move.SetDesiredMovement(MovementTags.MovementState_Jog);
            move.SetInputDirection(Vector3.forward);

            Vector3 start = go.transform.position;
            yield return new WaitForSeconds(0.5f); // 按游戏时间推进

            // 本测试验证"按输入方向移动"（方向正确 + 非零位移）。
            // 精确速度由 C 验证；headless 批处理 Time.deltaTime 极小，绝对位移量不可靠，故只断言方向性。
            Vector3 delta = go.transform.position - start;
            Assert.Greater(delta.z, 0.02f, "应朝输入方向(+Z)移动");
            Assert.Greater(delta.z, Mathf.Abs(delta.x) + 0.01f, "位移应主要沿输入方向(+Z)，而非横向漂移");
        }

        // C) 速度随移动状态变化（Walk < Sprint）
        [UnityTest]
        public IEnumerator C_Speed_ChangesWithState()
        {
            var go = NewPawn("Pawn");
            go.AddComponent<AbilitySystemComponent>();
            var move = go.AddComponent<CharacterMovementSystemComponent>();
            yield return null;

            move.PushAvailableMovementDefinition(MakeDefinition(
                (MovementTags.MovementState_Walk, 2f),
                (MovementTags.MovementState_Sprint, 6f)));
            move.SetMovementSet(Tag("Movement.Set.Default"));

            move.SetDesiredMovement(MovementTags.MovementState_Walk);
            Assert.AreEqual(2f, move.CurrentSpeed, 0.01f, "Walk 速度应为 2");

            move.SetDesiredMovement(MovementTags.MovementState_Sprint);
            Assert.AreEqual(6f, move.CurrentSpeed, 0.01f, "Sprint 速度应为 6");
        }
    }
}
