// Copyright (c) 2026 Likeon. Licensed under the MIT License.
// Movement Demo 场景生成器：程序化产出 InputConfig / InputControlSetup / MovementDefinition 资产
// 与一个可直接 Play 的 MovementDemo.unity 场景（玩家 + 第三人称相机 + 地面）。
// 把 Move/Look 输入处理器接到输入系统、配置接到组件——演示"输入经 GIPS → 移动/相机"。

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Likeon.GAS.Sample.MovementDemo.Editor
{
    public static class MovementDemoBuilder
    {
        private const string Menu = "Likeon/GAS/Samples/Build Movement Demo Scene";

        [MenuItem(Menu)]
        public static void Build()
        {
            // 1) 定位本示例目录（以 .inputactions 资产所在目录为锚）
            var guids = AssetDatabase.FindAssets("MovementDemoControls t:InputActionAsset");
            if (guids.Length == 0) { Debug.LogError("[MovementDemo] 找不到 MovementDemoControls.inputactions"); return; }
            string actionsPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            string dir = System.IO.Path.GetDirectoryName(actionsPath).Replace('\\', '/');

            // 2) 取 Move / Look 的 InputActionReference（.inputactions 导入时自动生成的子资产）
            var refs = AssetDatabase.LoadAllAssetsAtPath(actionsPath).OfType<InputActionReference>().ToList();
            var moveRef = refs.FirstOrDefault(r => r.action != null && r.action.name == "Move");
            var lookRef = refs.FirstOrDefault(r => r.action != null && r.action.name == "Look");
            if (moveRef == null || lookRef == null) { Debug.LogError("[MovementDemo] 找不到 Move/Look InputActionReference"); return; }

            var tMove = GameplayTag.RequestTag("InputTag.Move");
            var tLook = GameplayTag.RequestTag("InputTag.Look");

            // 3) InputConfig：InputTag ↔ InputAction
            var config = ScriptableObject.CreateInstance<InputConfig>();
            config.InputActionMappings.Add(new InputActionMapping { InputTag = tMove, Action = moveRef, ValueBinding = true });
            config.InputActionMappings.Add(new InputActionMapping { InputTag = tLook, Action = lookRef, ValueBinding = true });
            CreateAsset(config, dir + "/MovementDemo_InputConfig.asset");

            // 4) InputControlSetup：挂 Move / Look 处理器
            var setup = ScriptableObject.CreateInstance<InputControlSetup>();
            setup.ExecutionType = EInputProcessorExecutionType.MatchAll;
            var move = new InputProcessor_Move { CameraRelative = true };
            move.InputTags.AddTag(tMove);
            move.TriggerEvents = new List<InputTriggerEvent> { InputTriggerEvent.Started, InputTriggerEvent.Triggered, InputTriggerEvent.Canceled };
            setup.AddProcessor(move);
            var look = new InputProcessor_Look { Sensitivity = 1f };
            look.InputTags.AddTag(tLook);
            look.TriggerEvents = new List<InputTriggerEvent> { InputTriggerEvent.Started, InputTriggerEvent.Triggered };
            setup.AddProcessor(look);
            CreateAsset(setup, dir + "/MovementDemo_InputControlSetup.asset");

            // 5) MovementDefinition：一组 走/跑/冲刺 速度
            var def = ScriptableObject.CreateInstance<MovementDefinition>();
            var moveSet = new MovementSetSetting { MovementSet = GameplayTag.None };
            moveSet.States.Add(MovementStateSetting.Default(MovementTags.MovementState_Walk, 1.5f));
            moveSet.States.Add(MovementStateSetting.Default(MovementTags.MovementState_Jog, 3.75f));
            moveSet.States.Add(MovementStateSetting.Default(MovementTags.MovementState_Sprint, 6f));
            def.MovementSets.Add(moveSet);
            CreateAsset(def, dir + "/MovementDemo_MovementDef.asset");

            // 6) 建场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 地面
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5f, 1f, 5f);

            // 光
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 玩家
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 0.1f, 0f);
            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f; cc.radius = 0.4f; cc.center = new Vector3(0f, 1f, 0f);
            player.AddComponent<AbilitySystemComponent>();
            var mover = player.AddComponent<CharacterMovementSystemComponent>();
            var ic = player.AddComponent<InputSystemComponent>();
            var demo = player.AddComponent<MovementDemo>();
            demo.Mover = mover;

            var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            vis.name = "Visual";
            Object.DestroyImmediate(vis.GetComponent<Collider>());
            vis.transform.SetParent(player.transform, false);
            vis.transform.localPosition = new Vector3(0f, 1f, 0f);

            // 相机（第三人称）
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            var camSys = camGo.AddComponent<CameraSystemComponent>();
            camSys.Configure(cam, player.transform);
            camGo.transform.position = new Vector3(0f, 3f, -5f);

            // 接线（私有 SerializeField 经 SerializedObject）
            WireInputSystem(ic, config, setup);
            WireMover(mover, def, camGo.transform);

            // 保存
            string scenePath = dir + "/MovementDemo.unity";
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MovementDemo] 已生成场景与资产：{scenePath}");
        }

        private static void WireInputSystem(InputSystemComponent ic, InputConfig config, InputControlSetup setup)
        {
            var so = new SerializedObject(ic);
            so.FindProperty("inputConfig").objectReferenceValue = config;
            var setups = so.FindProperty("inputControlSetups");
            setups.arraySize = 1;
            setups.GetArrayElementAtIndex(0).objectReferenceValue = setup;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireMover(CharacterMovementSystemComponent mover, MovementDefinition def, Transform view)
        {
            var so = new SerializedObject(mover);
            var defs = so.FindProperty("movementDefinitions");
            defs.arraySize = 1;
            defs.GetArrayElementAtIndex(0).objectReferenceValue = def;
            var viewProp = so.FindProperty("viewReference");
            if (viewProp != null) viewProp.objectReferenceValue = view;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateAsset(Object asset, string path)
        {
            AssetDatabase.DeleteAsset(path); // 重建幂等
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
