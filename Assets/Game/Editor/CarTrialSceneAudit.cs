#if UNITY_EDITOR
using System;
using System.IO;
using NepalTrial.Trial;
using NepalTrial.Vehicles.Car;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NepalTrial.EditorTools
{
    /// <summary>Static scene wiring audit; does not replace PlayMode or Android testing.</summary>
    public static class CarTrialSceneAudit
    {
        [MenuItem("Nepal Trial/Phase 2/Audit Car Trial Scenes")]
        public static void Audit()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string original = SceneManager.GetActiveScene().path;
            int errors = 0;
            int inspected = 0;
            try
            {
                foreach (string course in new[] { "figure-eight","u-turn","hill-start",
                    "traffic-light","speed-breaker","garage" })
                {
                    string path = "Assets/Scenes/CarTrial_" + course + ".unity";
                    if (!File.Exists(path))
                    {
                        Debug.LogError("Missing " + path + ". Run Nepal Trial > Phase 2 > Create Car Trial Scenes.");
                        errors++;
                        continue;
                    }
                    EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                    inspected++;
                    CarDriveController car = UnityEngine.Object.FindFirstObjectByType<CarDriveController>();
                    TrialGameplayRuntime trial = UnityEngine.Object.FindFirstObjectByType<TrialGameplayRuntime>();
                    TrialCarSessionActions actions = UnityEngine.Object.FindFirstObjectByType<TrialCarSessionActions>();
                    TrialSessionHud hud = UnityEngine.Object.FindFirstObjectByType<TrialSessionHud>();
                    if (car==null || trial==null || actions==null || hud==null)
                    {
                        Debug.LogError(path + ": car, runtime, session actions or HUD missing");
                        errors++;
                        continue;
                    }
                    var settings = new SerializedObject(trial);
                    var stageIds = settings.FindProperty("orderedStages");
                    if (stageIds==null || stageIds.arraySize!=1 ||
                        stageIds.GetArrayElementAtIndex(0).stringValue!=course)
                    {
                        Debug.LogError(path + ": incorrect ordered stage");
                        errors++;
                    }
                    string component = course == "figure-eight" ? nameof(CarFigureEightSensor) :
                        course == "u-turn" ? nameof(CarUTurnSensor) :
                        course == "hill-start" ? nameof(CarHillStartSensor) :
                        course == "traffic-light" ? nameof(CarTrafficSignalSensor) :
                        course == "speed-breaker" ? nameof(CarSpeedBreakerSensor) :
                        nameof(CarGarageParkingSensor);
                    MonoBehaviour sensor = null;
                    foreach (var behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                        if (behaviour != null && behaviour.GetType().Name == component) { sensor = behaviour; break; }
                    if (sensor == null)
                    {
                        Debug.LogError(path + ": " + component + " missing");
                        errors++;
                    }
                    else
                    {
                        var so = new SerializedObject(sensor);
                        foreach (var key in new[] { "car", "trial" })
                        {
                            var field = so.FindProperty(key);
                            if (field == null || field.objectReferenceValue == null)
                            {
                                Debug.LogError(path + ": missing " + component + "." + key);
                                errors++;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(original) && File.Exists(original))
                    EditorSceneManager.OpenScene(original,OpenSceneMode.Single);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            }
            if(errors==0)
                Debug.Log("Phase 2 SCENE WIRING AUDIT: " + inspected + " scenes inspected, no missing critical references. Physics and exam outcomes NOT validated.");
            else
                Debug.LogError("Phase 2 SCENE WIRING AUDIT: " + errors + " errors across " + inspected + " scenes.");
        }
    }
}
#endif
