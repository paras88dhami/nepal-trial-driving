#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NepalTrial.Flow;
using NepalTrial.Input;
using NepalTrial.Trial;
using NepalTrial.Vehicles.Car;
using NepalTrial.Vehicles.TwoWheeler;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NepalTrial.EditorTools
{
    /// <summary>
    /// Assembles the existing practice prefabs/scenes into one continuous training
    /// ground per category. The connector roads are provisional: this is an
    /// integration milestone, NOT a verified official Nepal trial-ground CAD plan.
    /// Each exam has ONE vehicle, ONE camera, ONE timer and ONE trial coordinator.
    /// </summary>
    public static class ConnectedTrialSceneBuilder
    {
        private const string Folder="Assets/Scenes";
        private const float Spacing=70f;

        [MenuItem("Nepal Trial/Connected Exam/Generate All Three Connected Grounds")]
        public static void GenerateAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previous=SceneManager.GetActiveScene().path;
            Directory.CreateDirectory(Folder);
            try
            {
                foreach(string vehicle in TrialCatalog.Vehicles)
                    Generate(vehicle);
                RegisterBuildScenes();
            }
            finally
            {
                if (!string.IsNullOrEmpty(previous) && File.Exists(previous))
                    EditorSceneManager.OpenScene(previous,OpenSceneMode.Single);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
                AssetDatabase.Refresh();
            }
        }

        private static void Generate(string vehicle)
        {
            string name=TrialCatalog.ConnectedExamSceneFor(vehicle);
            string output=Folder+"/"+name+".unity";
            if(File.Exists(output))
            {
                Debug.LogWarning("Preserving existing connected exam scene: "+output);
                return;
            }

            string[] stages=TrialCatalog.ConnectedExamStagesFor(vehicle);
            bool twoWheeler=TrialCatalog.IsTwoWheeler(vehicle);
            int physicalStages=stages.Length-1;
            for(int i=0;i<physicalStages;i++)
            {
                string source=Folder+"/"+TrialCatalog.SceneFor(vehicle,stages[i])+".unity";
                if (!File.Exists(source))
                    throw new FileNotFoundException("Missing practice scene; run Phase 4 scene setup first: "+source);
            }

            Scene target=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            TrialGameplayRuntime sharedTrial=null;
            CarDriveController sharedCar=null;
            TwoWheelerRigidbodyController sharedBike=null;
            PrototypeCarInput sharedCarInput=null;
            TwoWheelerInput sharedBikeInput=null;

            for(int stage=0;stage<physicalStages;stage++)
            {
                string source=Folder+"/"+TrialCatalog.SceneFor(vehicle,stages[stage])+".unity";
                var loaded=EditorSceneManager.OpenScene(source,OpenSceneMode.Additive);
                SceneManager.SetActiveScene(target);
                GameObject stageRoot=new GameObject("Stage_"+(stage+1)+"_"+stages[stage]);
                var roots=loaded.GetRootGameObjects();
                foreach(GameObject root in roots)
                {
                    bool isVehicle=root.GetComponent<CarDriveController>()!=null ||
                                   root.GetComponent<TwoWheelerRigidbodyController>()!=null;
                    bool isTrial=root.GetComponent<TrialGameplayRuntime>()!=null ||
                                 root.GetComponentInChildren<TrialGameplayRuntime>(true)!=null;
                    bool isInterface=root.GetComponent<Canvas>()!=null ||
                                     root.GetComponent<UnityEngine.EventSystems.EventSystem>()!=null ||
                                     root.GetComponent<Camera>()!=null ||
                                     root.GetComponent<Light>()!=null ||
                                     root.GetComponent<TrialSessionHud>()!=null ||
                                     root.GetComponent<CarTrialResultsView>()!=null ||
                                     root.GetComponent<TwoWheelerDashboardHud>()!=null;
                    bool isBaseFloor=root.name=="Ground" || root.name=="PracticeRoad" ||
                                     root.name=="Road";
                    bool isBehavior=root.GetComponent<TwoWheelerBehaviorSensor>()!=null;
                    if (isBaseFloor || isBehavior ||
                        (stage>0 && (isVehicle || isTrial || isInterface)))
                    {
                        UnityEngine.Object.DestroyImmediate(root);
                        continue;
                    }
                    SceneManager.MoveGameObjectToScene(root,target);
                    if (stage>0 || (!isVehicle && !isTrial && !isInterface))
                        root.transform.SetParent(stageRoot.transform,true);
                }
                EditorSceneManager.CloseScene(loaded,true);

                if(stage==0)
                {
                    sharedTrial=UnityEngine.Object.FindFirstObjectByType<TrialGameplayRuntime>();
                    sharedCar=UnityEngine.Object.FindFirstObjectByType<CarDriveController>();
                    sharedBike=UnityEngine.Object.FindFirstObjectByType<TwoWheelerRigidbodyController>();
                    sharedCarInput=UnityEngine.Object.FindFirstObjectByType<PrototypeCarInput>();
                    sharedBikeInput=UnityEngine.Object.FindFirstObjectByType<TwoWheelerInput>();
                    if (sharedTrial==null || (twoWheeler ? sharedBike==null : sharedCar==null))
                        throw new InvalidOperationException("Base stage is missing trial runtime or correct vehicle.");
                }
                else
                {
                    // Remap every imported maneuver sensor/trigger to the ONE shared
                    // vehicle and exam runtime, instead of destroyed practice objects.
                    foreach(MonoBehaviour behaviour in stageRoot.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if(behaviour==null) continue;
                        var serialized=new SerializedObject(behaviour);
                        if (behaviour is CarGarageParkingSensor)
                        {
                            serialized.FindProperty("requireReverseApproach").boolValue=true;
                            serialized.ApplyModifiedPropertiesWithoutUndo();
                        }
                        if (behaviour is CarHillStartSensor)
                        {
                            serialized.FindProperty("maxRollbackMeters").floatValue=.1524f;
                            serialized.ApplyModifiedPropertiesWithoutUndo();
                        }
                        if (behaviour is TwoWheelerRampSensor)
                        {
                            serialized.FindProperty("maxRollbackMeters").floatValue=.1524f;
                            serialized.ApplyModifiedPropertiesWithoutUndo();
                        }
                        var field=serialized.GetIterator();
                        bool changed=false;
                        if(field.NextVisible(true))
                            do
                            {
                                if(field.propertyType != SerializedPropertyType.ObjectReference) continue;
                                // Stage roots are moved after the old per-stage car/trial
                                // are deleted. Their serialized references can therefore
                                // be missing; use the known field contracts, not the old type.
                                var original=field.objectReferenceValue;
                                UnityEngine.Object replacement=null;
                                switch (field.name)
                                {
                                    case "trial": replacement=sharedTrial; break;
                                    case "car": replacement=sharedCar; break;
                                    case "vehicle": replacement=sharedBike; break;
                                    case "input":
                                        replacement=twoWheeler ? (UnityEngine.Object)sharedBikeInput : sharedCarInput;
                                        break;
                                    case "vehicleBody":
                                        if (!twoWheeler && sharedCar!=null)
                                            replacement=sharedCar.GetComponent<BoxCollider>();
                                        break;
                                }
                                if(replacement==null || replacement==original)continue;
                                field.objectReferenceValue=replacement;
                                changed=true;
                            }
                            while(field.NextVisible(false));
                        if(changed)serialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                }

                stageRoot.transform.position=new Vector3(0,0,stage*Spacing);
                PlaceStageSign(stageRoot.transform,stages[stage],stage);
            }

            SetupSurface(physicalStages);
            SetOverallRuntime(sharedTrial,vehicle,stages);

            var behavior=new GameObject("Stage_7_BehavioralAssessment")
                .AddComponent<TrialBehaviorChecklist>();
            var so=new SerializedObject(behavior);
            so.FindProperty("trial").objectReferenceValue=sharedTrial;
            so.FindProperty("twoWheeler").boolValue=twoWheeler;
            so.ApplyModifiedPropertiesWithoutUndo();
            PlaceStageSign(behavior.transform,"behavior",physicalStages);

            EditorSceneManager.MarkSceneDirty(target);
            if(!EditorSceneManager.SaveScene(target,output))
                throw new IOException("Unity failed to save "+output);
            Debug.Log("Generated "+output+" with one vehicle and seven ordered trial stages. "+
                "Road links and trigger placement MUST be checked in PlayMode.");
        }

        private static void SetOverallRuntime(TrialGameplayRuntime trial,string vehicle,string[] stages)
        {
            var serialized=new SerializedObject(trial);
            serialized.FindProperty("courseId").stringValue="full-exam-"+vehicle;
            var ordered=serialized.FindProperty("orderedStages");
            ordered.arraySize=stages.Length;
            for(int i=0;i<stages.Length;i++)
                ordered.GetArrayElementAtIndex(i).stringValue=stages[i];
            var rules=NepalTrialScoringCatalog.GetRules(TrialCatalog.IsTwoWheeler(vehicle));
            var penalties=serialized.FindProperty("penaltyRules");
            penalties.arraySize=rules.Count;
            for(int i=0;i<rules.Count;i++)
            {
                var item=penalties.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("code").stringValue=rules[i].Code;
                item.FindPropertyRelative("points").intValue=rules[i].Points;
                item.FindPropertyRelative("failAtOccurrence").intValue=rules[i].FailOnOccurrence;
            }
            serialized.FindProperty("startingScore").intValue=100;
            serialized.FindProperty("passingScore").intValue=70;
            serialized.FindProperty("durationSeconds").floatValue=1200f; // prototype overall limit
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetupSurface(int physicalStages)
        {
            float length=(physicalStages+1)*Spacing+80f;
            float midpoint=(physicalStages-1)*Spacing*.5f;
            GameObject ground=GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name="ConnectedExam_Ground";
            ground.transform.position=new Vector3(0,-.7f,midpoint);
            ground.transform.localScale=new Vector3(85f,1.2f,length);
            ground.GetComponent<Renderer>().sharedMaterial=CreateMaterial(
                "ConnectedGround",new Color(.18f,.28f,.16f));

            GameObject pavement=GameObject.CreatePrimitive(PrimitiveType.Cube);
            pavement.name="ConnectedExam_Pavement";
            pavement.transform.position=new Vector3(0,-.04f,midpoint);
            pavement.transform.localScale=new Vector3(46f,.12f,length);
            pavement.GetComponent<Renderer>().sharedMaterial=CreateMaterial(
                "ConnectedRoad",new Color(.19f,.21f,.24f));
        }

        private static Material CreateMaterial(string name,Color color)
        {
            string folder="Assets/Game/Generated/Materials";
            Directory.CreateDirectory(folder);
            string path=folder+"/"+name+".mat";
            Material existing=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(existing!=null)return existing;
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null)throw new InvalidOperationException("Missing URP/Standard shader.");
            var material=new Material(shader){color=color};
            AssetDatabase.CreateAsset(material,path);
            return material;
        }

        private static void PlaceStageSign(Transform stageRoot,string name,int stage)
        {
            var sign=GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name="Guide_"+(stage+1)+"_"+name;
            sign.transform.position=new Vector3(23f,.5f,stage*Spacing-16f);
            sign.transform.localScale=new Vector3(.5f,1f,3f);
            sign.GetComponent<Renderer>().sharedMaterial=CreateMaterial(
                "ConnectedCheckpoint",new Color(.95f,.82f,.10f));
            sign.transform.SetParent(stageRoot,true);
        }

        private static void RegisterBuildScenes()
        {
            var list=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach(string vehicle in TrialCatalog.Vehicles)
            {
                string path=Folder+"/"+TrialCatalog.ConnectedExamSceneFor(vehicle)+".unity";
                if(!File.Exists(path))continue;
                bool found=false;
                foreach(var scene in list)
                    if(scene.path==path){scene.enabled=true;found=true;break;}
                if(!found)list.Add(new EditorBuildSettingsScene(path,true));
            }
            EditorBuildSettings.scenes=list.ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
