#if UNITY_EDITOR
using System.IO;
using NepalTrial.Input;
using NepalTrial.Trial;
using NepalTrial.Vehicles.TwoWheeler;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NepalTrial.EditorTools
{
    /// <summary>Static 14-scene component/reference audit; physics tests must run separately.</summary>
    public static class TwoWheelerTrialSceneAudit
    {
        [MenuItem("Nepal Trial/Phase 3/Audit Motorcycle and Scooter Trials")]
        public static void Audit()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string original=SceneManager.GetActiveScene().path;
            int inspected=0,errors=0;
            try
            {
                foreach (var kind in new[]{"motorcycle","scooter"})
                foreach (var course in new[]{"figure-eight","narrow-plank","u-turn",
                    "traffic-light","speed-breaker","ramp","behavior"})
                {
                    string path="Assets/Scenes/BikeTrial_"+kind+"_"+course+".unity";
                    if (!File.Exists(path))
                    {
                        Debug.LogError("Missing "+path+
                            " (first run Nepal Trial > Phase 3 > Create Motorcycle and Scooter Trials)");
                        errors++;
                        continue;
                    }
                    EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                    inspected++;
                    var rider=UnityEngine.Object.FindFirstObjectByType<TwoWheelerRigidbodyController>();
                    var controls=UnityEngine.Object.FindFirstObjectByType<TwoWheelerInput>();
                    var session=UnityEngine.Object.FindFirstObjectByType<TrialGameplayRuntime>();
                    var actions=UnityEngine.Object.FindFirstObjectByType<TwoWheelerSessionActions>();
                    var hud=UnityEngine.Object.FindFirstObjectByType<TrialSessionHud>();
                    if (rider==null || controls==null || session==null || actions==null || hud==null)
                    {
                        errors++;
                        Debug.LogError(path+" missing vehicle/controls/session/actions/HUD");
                        continue;
                    }
                    if (rider.Kind.ToString().ToLowerInvariant()!=kind)
                    {
                        errors++;
                        Debug.LogError(path+" wrong vehicle kind");
                    }
                    if (rider.GetComponentsInChildren<WheelCollider>().Length!=2)
                    {
                        errors++;
                        Debug.LogError(path+" requires two WheelCollider wheels");
                    }
                    var runtime=new SerializedObject(session);
                    var stages=runtime.FindProperty("orderedStages");
                    if (stages==null || stages.arraySize!=1 ||
                        stages.GetArrayElementAtIndex(0).stringValue!=course)
                    {
                        errors++;
                        Debug.LogError(path+" wrong trial stage configuration");
                    }
                    string sensorName=course=="figure-eight"?nameof(TwoWheelerFigureEightSensor):
                        course=="narrow-plank"?nameof(TwoWheelerPlankSensor):
                        course=="u-turn"?nameof(TwoWheelerUTurnSensor):
                        course=="traffic-light"?nameof(TwoWheelerTrafficSignalSensor):
                        course=="speed-breaker"?nameof(TwoWheelerBumpySensor):
                        course=="ramp"?nameof(TwoWheelerRampSensor):
                        nameof(TwoWheelerBehaviorSensor);
                    MonoBehaviour sensor=null;
                    foreach (var script in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                        if (script!=null && script.GetType().Name==sensorName){ sensor=script;break; }
                    if (sensor==null)
                    {
                        errors++;
                        Debug.LogError(path+" missing "+sensorName);
                    }
                    else
                    {
                        var serialized=new SerializedObject(sensor);
                        var reference=serialized.FindProperty("trial");
                        if (reference==null || reference.objectReferenceValue!=session)
                        {
                            errors++;
                            Debug.LogError(path+" missing "+sensorName+".trial");
                        }
                        if (course!="behavior")
                        {
                            var vehicle=serialized.FindProperty("vehicle");
                            if(vehicle==null || vehicle.objectReferenceValue!=rider)
                            {
                                errors++;
                                Debug.LogError(path+" missing "+sensorName+".vehicle");
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
            if (errors==0)
                Debug.Log("Phase 3 scene wiring audit: "+inspected+
                    " scenes inspected, no critical missing references. DOES NOT verify wheel physics.");
            else
                Debug.LogError("Phase 3 scene wiring audit: "+errors+
                    " errors across "+inspected+" inspected scenes.");
        }
    }
}
#endif
