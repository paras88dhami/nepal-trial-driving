#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NepalTrial.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NepalTrial.EditorTools
{
    /// <summary>Generates menu and registers all 20 individual car/two-wheeler course scenes for builds.</summary>
    public static class Phase4GameSetup
    {
        [MenuItem("Nepal Trial/Phase 4/Prepare Playable Game")]
        public static void Prepare()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            // These editors only create scenes that do not already exist.
            CarTrialSceneBuilder.CreateAll();
            TwoWheelerTrialSceneBuilder.Create();
            CreateMenuAndRegister();
        }

        [MenuItem("Nepal Trial/Phase 4/Create Menu and Register Build Scenes")]
        public static void CreateMenuAndRegister()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string original = SceneManager.GetActiveScene().path;
            string menuPath = "Assets/Scenes/" + TrialCatalog.MenuScene + ".unity";
            Directory.CreateDirectory("Assets/Scenes");
            try
            {
                if (!File.Exists(menuPath))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    var cameraGo = new GameObject("MenuCamera");
                    cameraGo.tag = "MainCamera";
                    var camera = cameraGo.AddComponent<Camera>();
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(.025f,.045f,.075f);
                    camera.orthographic = true;
                    cameraGo.AddComponent<AudioListener>();
                    new GameObject("NepalTrialMainMenu").AddComponent<TrialMainMenuController>();
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene,menuPath))
                        throw new IOException("Unable to save menu scene: " + menuPath);
                }
                else Debug.Log("Preserving existing menu scene: " + menuPath);

                // Enforce menu as scene zero, then known trial scenes. Preserve other scenes.
                var ordered = new List<EditorBuildSettingsScene>();
                var seen = new HashSet<string>();
                Add(menuPath,ordered,seen);
                foreach (string course in TrialCatalog.CarCourses)
                    Add("Assets/Scenes/CarTrial_" + course + ".unity",ordered,seen);
                foreach (string kind in new[] { "motorcycle","scooter" })
                    foreach (string course in TrialCatalog.TwoWheelerCourses)
                        Add("Assets/Scenes/BikeTrial_" + kind + "_" + course + ".unity",ordered,seen);
                foreach (var entry in EditorBuildSettings.scenes)
                    if (entry != null && !seen.Contains(entry.path) && File.Exists(entry.path))
                    {
                        ordered.Add(entry);
                        seen.Add(entry.path);
                    }
                EditorBuildSettings.scenes = ordered.ToArray();
                AssetDatabase.SaveAssets();
                Debug.Log("Phase 4 Build Settings: " + ordered.Count +
                    " scenes; menu is first. Generate missing courses before testing navigation.");
            }
            finally
            {
                if (!string.IsNullOrEmpty(original) && File.Exists(original))
                    EditorSceneManager.OpenScene(original,OpenSceneMode.Single);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
                AssetDatabase.Refresh();
            }
        }

        private static void Add(string path,List<EditorBuildSettingsScene> ordered,HashSet<string> seen)
        {
            if (seen.Contains(path)) return;
            if (!File.Exists(path))
            {
                Debug.LogWarning("Phase 4 scene missing: "+path);
                return;
            }
            ordered.Add(new EditorBuildSettingsScene(path,true));
            seen.Add(path);
        }
    }
}
#endif
