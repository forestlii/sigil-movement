// PlayMode 冒烟测试：验证烘出的 MovementDemoPlayer prefab 实例化后组件齐、输入/移动配置接好。
// （prefab 化后 demo 随包发一个可玩场景 + prefab；本测试确认 prefab 接线正确。）
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Likeon.GAS;

namespace Likeon.GAS.Sample.MovementDemo.Tests
{
    public class MovementDemoSmokeTest
    {
        [UnityTest]
        public IEnumerator PlayerPrefab_Instantiates_WithInputAndMovementWired()
        {
            var prefab = Resources.Load<GameObject>("MovementDemoPlayer");
            Assert.IsNotNull(prefab, "应能从 Resources 加载 MovementDemoPlayer prefab（先运行 Likeon ▸ GAS ▸ Samples ▸ Build Movement Demo Scene 生成）");

            var player = Object.Instantiate(prefab);
            yield return null; // 等 Awake

            Assert.IsNotNull(player.GetComponent<CharacterController>(), "应含 CharacterController");
            Assert.IsNotNull(player.GetComponent<AbilitySystemComponent>(), "应含 ASC");

            var mover = player.GetComponent<CharacterMovementSystemComponent>();
            Assert.IsNotNull(mover, "应含角色移动组件");

            var input = player.GetComponent<InputSystemComponent>();
            Assert.IsNotNull(input, "应含输入组件");
            Assert.IsNotNull(input.Config, "prefab 应烘好 InputConfig（.inputactions → InputConfig 自动绑定链）");

            var demo = player.GetComponent<Likeon.GAS.Sample.MovementDemo.MovementDemo>();
            Assert.IsNotNull(demo, "应含 MovementDemo");
            Assert.AreSame(mover, demo.Mover, "MovementDemo.Mover 应接到同 prefab 的移动组件");

            Object.Destroy(player);
            yield return null;
        }
    }
}
