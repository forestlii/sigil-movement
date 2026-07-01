// Copyright (c) 2026 Likeon. Licensed under the MIT License.
// Movement Demo 生成器：程序化产出 InputConfig / InputControlSetup / MovementDefinition 资产 +
// 一个 Player prefab（Resources/）+ 一个可直接 Play 的 MovementDemo.unity 场景（玩家实例 + 第三人称相机 + 地面）。
// prefab 化：结构（组件 + 输入/移动配置资产引用）烘进 prefab；跨边界引用（Mover.viewReference = 相机）在场景级接。
// 菜单：Likeon ▸ GAS ▸ Samples ▸ Build Movement Demo Scene（重跑幂等）。

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

            // 3) 配置资产：InputConfig / InputControlSetup / MovementDefinition
            var config = ScriptableObject.CreateInstance<InputConfig>();
            config.InputActionMappings.Add(new InputActionMapping { InputTag = tMove, Action = moveRef, ValueBinding = true });
            config.InputActionMappings.Add(new InputActionMapping { InputTag = tLook, Action = lookRef, ValueBinding = true });
            CreateAsset(config, dir + "/MovementDemo_InputConfig.asset");

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

            var def = ScriptableObject.CreateInstance<MovementDefinition>();
            var moveSet = new MovementSetSetting { MovementSet = GameplayTag.None };
            moveSet.States.Add(MovementStateSetting.Default(MovementTags.MovementState_Walk, 1.5f));
            moveSet.States.Add(MovementStateSetting.Default(MovementTags.MovementState_Jog, 3.75f));
            moveSet.States.Add(MovementStateSetting.Default(MovementTags.MovementState_Sprint, 6f));
            def.MovementSets.Add(moveSet);
            CreateAsset(def, dir + "/MovementDemo_MovementDef.asset");

            // 4) Player prefab（结构 + 输入/移动配置；viewReference 留场景接）→ 放 Resources/
            string resDir = dir + "/Resources";
            if (!AssetDatabase.IsValidFolder(resDir)) AssetDatabase.CreateFolder(dir, "Resources");
            var playerObj = BuildPlayer(config, setup, def);
            string prefabPath = resDir + "/MovementDemoPlayer.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(playerObj, prefabPath);
            Object.DestroyImmediate(playerObj);

            // 5) 场景：地面 + 光 + 玩家 prefab 实例 + 第三人称相机（相机↔玩家在场景级互引）
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5f, 1f, 5f);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.transform.position = new Vector3(0f, 0.1f, 0f);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            var camSys = camGo.AddComponent<CameraSystemComponent>();
            camSys.Configure(cam, player.transform);
            camGo.transform.position = new Vector3(0f, 3f, -5f);

            // 跨边界：玩家的视角参考 = 场景相机（prefab 内接不了，场景级接）
            WireMoverView(player.GetComponent<CharacterMovementSystemComponent>(), camGo.transform);

            string scenePath = dir + "/MovementDemo.unity";
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MovementDemo] 已生成 prefab + 场景 + 配置资产：{prefabPath} / {scenePath}");
        }

        // 玩家结构：组件 + 可视胶囊子物体 + 输入/移动配置引用（viewReference 留空，场景接）
        private static GameObject BuildPlayer(InputConfig config, InputControlSetup setup, MovementDefinition def)
        {
            var player = new GameObject("Player");
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

            WireInputSystem(ic, config, setup);
            WireMoverDefs(mover, def);
            return player;
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

        private static void WireMoverDefs(CharacterMovementSystemComponent mover, MovementDefinition def)
        {
            var so = new SerializedObject(mover);
            var defs = so.FindProperty("movementDefinitions");
            defs.arraySize = 1;
            defs.GetArrayElementAtIndex(0).objectReferenceValue = def;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireMoverView(CharacterMovementSystemComponent mover, Transform view)
        {
            var so = new SerializedObject(mover);
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
