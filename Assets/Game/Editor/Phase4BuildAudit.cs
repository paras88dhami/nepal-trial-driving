#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using NepalTrial.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NepalTrial.EditorTools
{
    /// <summary>Static Phase 4 scene/build settings audit, not a runtime or physics QA result.</summary>
    public static class Phase4BuildAudit
    {
        [MenuItem("Nepal Trial/Phase 4/Audit Menu and Build Settings")]
        public static void Audit()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string original=SceneManager.GetActiveScene().path;
            int errors=0,checkedTrials=0;
            try
            {
                EditorBuildSettingsScene[] builds=EditorBuildSettings.scenes;
                if (builds==null || builds.Length==0 ||
                    builds[0].path!="Assets/Scenes/"+TrialCatalog.MenuScene+".unity" ||
                    !builds[0].enabled)
                {
                    Debug.LogError("Phase4: Main Menu must be scene 0 in Build Settings.");
                    errors++;
                }
                var enabled=new HashSet<string>();
                if (builds!=null)
                    foreach (var scene in builds)
                        if (scene.enabled && File.Exists(scene.path)) enabled.Add(scene.path);
                string menu="Assets/Scenes/"+TrialCatalog.MenuScene+".unity";
                if (!File.Exists(menu))
                {
                    Debug.LogError("Phase4: missing main menu scene "+menu);
                    errors++;
                }
                else
                {
                    EditorSceneManager.OpenScene(menu,OpenSceneMode.Single);
                    if (UnityEngine.Object.FindFirstObjectByType<TrialMainMenuController>()==null)
                    {
                        Debug.LogError("Phase4: menu is missing TrialMainMenuController.");
                        errors++;
                    }
                }
                foreach (string vehicle in TrialCatalog.Vehicles)
                foreach (string course in TrialCatalog.CoursesFor(vehicle))
                {
                    string name=TrialCatalog.SceneFor(vehicle,course);
                    string path="Assets/Scenes/"+name+".unity";
                    if (!enabled.Contains(path))
                    {
                        Debug.LogError("Phase4: scene missing or disabled in Build Settings: "+path);
                        errors++;
                    }
                    else checkedTrials++;
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(original) && File.Exists(original))
                    EditorSceneManager.OpenScene(original,OpenSceneMode.Single);
                else EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            }
            if (errors == 0)
                Debug.Log("Phase4 static audit: Menu and "+checkedTrials+
                    " trial scenes registered, no missing scene/build references. NOT a PlayMode test.");
            else
                Debug.LogError("Phase4 static audit: "+errors+" errors, "+checkedTrials+
                    " registered trial scenes. Run Phase4 Prepare Playable Game.");
        }
    }
}
#endif
