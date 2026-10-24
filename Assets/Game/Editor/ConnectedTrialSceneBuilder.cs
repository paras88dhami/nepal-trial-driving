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
        public static void GenerateAll() => GenerateBatch(false);

        [MenuItem("Nepal Trial/Connected Exam/Rebuild Connected Grounds (Back Up Existing)")]
        public static void RebuildAll() => GenerateBatch(true);

        private static void GenerateBatch(bool overwrite)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previous=SceneManager.GetActiveScene().path;
            Directory.CreateDirectory(Folder);
            try
            {
                foreach(string vehicle in TrialCatalog.Vehicles)
                    Generate(vehicle,overwrite);
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

        [MenuItem("Nepal Trial/Connected Exam/Audit All Three Connected Grounds")]
        public static void AuditAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previous=SceneManager.GetActiveScene().path;
            int errors=0;
            try
            {
                foreach(string vehicle in TrialCatalog.Vehicles)
                {
                    string path=Folder+"/"+TrialCatalog.ConnectedExamSceneFor(vehicle)+".unity";
                    if(!File.Exists(path))
                    {
                        Debug.LogError("Missing connected exam: "+path); errors++; continue;
                    }
                    EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                    TrialGameplayRuntime trial=UnityEngine.Object.FindFirstObjectByType<TrialGameplayRuntime>();
                    var car=UnityEngine.Object.FindObjectsByType<CarDriveController>(FindObjectsSortMode.None);
                    var bikes=UnityEngine.Object.FindObjectsByType<TwoWheelerRigidbodyController>(FindObjectsSortMode.None);
                    int total=car.Length+bikes.Length;
                    if(trial==null || total!=1 ||
                        (vehicle==TrialCatalog.StarterCar ? car.Length!=1 : bikes.Length!=1))
                    {
                        Debug.LogError(path+": expected exactly one trial runtime and one correctly typed vehicle.");
                        errors++;
                        continue;
                    }
                    var serialized=new SerializedObject(trial);
                    var stages=serialized.FindProperty("orderedStages");
                    var expected=TrialCatalog.ConnectedExamStagesFor(vehicle);
                    if(stages==null || stages.arraySize!=expected.Length)
                    {
                        Debug.LogError(path+": seven-stage trial order is missing."); errors++; continue;
                    }
                    for(int i=0;i<expected.Length;i++)
                        if(stages.GetArrayElementAtIndex(i).stringValue!=expected[i])
                        {
                            Debug.LogError(path+": incorrect stage "+i); errors++;
                        }
                    if(UnityEngine.Object.FindFirstObjectByType<TrialBehaviorChecklist>()==null)
                    {
                        Debug.LogError(path+": behavioral assessment UI missing."); errors++;
                    }
                    foreach(MonoBehaviour script in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                        FindObjectsSortMode.None))
                    {
                        if(script==null)continue;
                        SerializedProperty p=new SerializedObject(script).FindProperty("trial");
                        if(p==null)continue;
                        if(p.objectReferenceValue != trial)
                        {
                            Debug.LogError(path+": "+script.GetType().Name+" has missing/wrong exam link.");
                            errors++;
                        }
                    }
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(previous) && File.Exists(previous))
                    EditorSceneManager.OpenScene(previous,OpenSceneMode.Single);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            }
            if(errors==0)Debug.Log("Connected exam wiring audit passed for three scenes. "+
                "Road driving, gate directions and marks STILL need PlayMode checks.");
            else Debug.LogError("Connected exam audit found "+errors+" issue(s).");
        }

        private static void Generate(string vehicle,bool overwrite)
        {
            string name=TrialCatalog.ConnectedExamSceneFor(vehicle);
            string output=Folder+"/"+name+".unity";
            if(File.Exists(output) && !overwrite)
            {
                Debug.LogWarning("Preserving existing connected exam scene: "+output);
                return;
            }
            if(overwrite && File.Exists(output))
            {
                string backup=Path.Combine("Build","ConnectedSceneBackups",
                    DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"),name+".unity");
                Directory.CreateDirectory(Path.GetDirectoryName(backup));
                File.Copy(output,backup,true);
                Debug.Log("Saved previous connected scene backup: "+backup);
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
                    if (!twoWheeler && sharedCar.GetComponent<CarTurnIndicators>()==null)
                        sharedCar.gameObject.AddComponent<CarTurnIndicators>();
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
                        if (behaviour is CarSpeedBreakerSensor ||
                            behaviour is TwoWheelerBumpySensor)
                        {
                            serialized.FindProperty("enforceTrainingSpeedLimit").boolValue=false;
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

                if(stage==0)
                {
                    RebuildOfficialEight(stageRoot.transform,twoWheeler);
                    Vector3 spawn=twoWheeler
                        ? new Vector3(-2.25f,1.05f,-2.5f)
                        : new Vector3(-4.5f,1.15f,-4.5f);
                    var root=twoWheeler ? sharedBike.transform : sharedCar.transform;
                    root.SetPositionAndRotation(spawn,Quaternion.Euler(0,90,0));
                }
                if(!twoWheeler && stages[stage]=="garage")
                    RebuildCarLBack(stageRoot.transform,sharedTrial,sharedCar);
                if(!twoWheeler && stages[stage]=="hill-start")
                    RebuildCarRamp(stageRoot.transform);
                if(twoWheeler && stages[stage]=="ramp")
                    RebuildBikeRamp(stageRoot.transform);
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

        private static void RebuildBikeRamp(Transform stageRoot)
        {
            TwoWheelerRampSensor ramp=stageRoot.GetComponentInChildren<TwoWheelerRampSensor>(true);
            if(ramp==null)throw new InvalidOperationException("Bike ramp sensor missing.");
            GameObject zone=new GameObject("Ramp_BikeDownhillStopArea");
            zone.transform.SetParent(stageRoot,false);
            zone.transform.position=new Vector3(0,1.6f,16.8f);
            var collider=zone.AddComponent<BoxCollider>();
            collider.isTrigger=true;
            collider.size=new Vector3(7f,4f,2.5f);
            var serialized=new SerializedObject(ramp);
            serialized.FindProperty("downhillStopZone").objectReferenceValue=collider;
            serialized.FindProperty("maxRollbackMeters").floatValue=.1524f;
            serialized.FindProperty("timeoutSeconds").floatValue=90f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            DrawStripe("Ramp_BikeDownhillStopStripe",new Vector3(0,.10f,16.8f),
                new Vector3(7f,.04f,.15f),CreateMaterial(
                    "ConnectedFigureEightLines",new Color(.96f,.8f,.10f)),stageRoot);
        }

        private static void RebuildCarRamp(Transform stageRoot)
        {
            CarHillStartSensor hill=stageRoot.GetComponentInChildren<CarHillStartSensor>(true);
            if(hill==null)throw new InvalidOperationException("Hill-start sensor missing.");
            var upStop=new GameObject("Ramp_UphillStopArea");
            upStop.transform.SetParent(stageRoot,false);
            upStop.transform.position=new Vector3(0,1.8f,.5f);
            BoxCollider uphill=upStop.AddComponent<BoxCollider>();
            uphill.isTrigger=true;
            uphill.size=new Vector3(8f,4f,2.5f);

            var downStop=new GameObject("Ramp_DownhillStopArea");
            downStop.transform.SetParent(stageRoot,false);
            downStop.transform.position=new Vector3(0,1.5f,23f);
            BoxCollider downhill=downStop.AddComponent<BoxCollider>();
            downhill.isTrigger=true;
            downhill.size=new Vector3(8f,4f,2.5f);

            var crest=new GameObject("Ramp_DescentStart");
            crest.transform.SetParent(stageRoot,false);
            crest.transform.position=new Vector3(0,1.3f,13.5f);
            crest.transform.forward=Vector3.forward;

            var axis=new GameObject("Ramp_TravelAxis");
            axis.transform.SetParent(stageRoot,false);
            axis.transform.position=Vector3.zero;
            axis.transform.forward=Vector3.forward;

            var s=new SerializedObject(hill);
            s.FindProperty("uphillStopZone").objectReferenceValue=uphill;
            s.FindProperty("downhillStopZone").objectReferenceValue=downhill;
            s.FindProperty("descentStart").objectReferenceValue=crest.transform;
            s.FindProperty("uphillAxis").objectReferenceValue=axis.transform;
            s.FindProperty("maxRollbackMeters").floatValue=.1524f;
            s.FindProperty("maximumSeconds").floatValue=90f;
            s.ApplyModifiedPropertiesWithoutUndo();

            Transform exit=stageRoot.Find("ExitHill");
            if(exit==null)throw new InvalidOperationException("Expected ExitHill trigger missing.");
            exit.position=new Vector3(0,1.5f,27f);
            Transform stripe=stageRoot.Find("ExitHill_Stripe");
            if(stripe!=null)stripe.position=new Vector3(0,.12f,27f);

            GameObject descending=GameObject.CreatePrimitive(PrimitiveType.Cube);
            descending.name="Car_DrivableDownhillRamp";
            descending.transform.SetParent(stageRoot,false);
            descending.transform.position=new Vector3(0,.65f,20f);
            descending.transform.localScale=new Vector3(8f,.35f,12f);
            descending.transform.rotation=Quaternion.Euler(6f,0,0);
            descending.GetComponent<Renderer>().sharedMaterial=CreateMaterial(
                "ConnectedRoad",new Color(.19f,.21f,.24f));
            DrawStripe("RampDownhillStopStripe",new Vector3(0,.10f,23f),
                new Vector3(8f,.04f,.15f),
                CreateMaterial("ConnectedFigureEightLines",new Color(.96f,.80f,.10f)),stageRoot);
        }

        private static void RebuildCarLBack(Transform stageRoot,
            TrialGameplayRuntime trial,CarDriveController car)
        {
            // L-shaped driving surface: longitudinal entrance, perpendicular
            // reverse leg, final bay. Geometry is a prototype and must be
            // measured and driven in the Unity Editor before release.
            var sensor=stageRoot.GetComponentInChildren<CarGarageParkingSensor>(true);
            if(sensor==null)throw new InvalidOperationException("Garage parking sensor missing.");
            foreach(Transform t in stageRoot.GetComponentsInChildren<Transform>(true))
            {
                if(t==null || t==stageRoot)continue;
                if(t.name=="GarageLeftWall" || t.name=="GarageRightWall" ||
                   t.name=="GarageBackWall" || t.name=="ParkingFloor")
                    UnityEngine.Object.DestroyImmediate(t.gameObject);
            }
            var parking=sensor.GetComponent<BoxCollider>();
            sensor.transform.position=new Vector3(-12f,1.25f,10f);
            parking.size=new Vector3(6f,3.5f,7.5f);
            var approachObj=new GameObject("LBack_ApproachLeg");
            approachObj.transform.SetParent(stageRoot,false);
            approachObj.transform.position=new Vector3(0,1f,-7f);
            var approach=approachObj.AddComponent<BoxCollider>();
            approach.isTrigger=true;
            approach.size=new Vector3(8f,3f,38f);
            var reverseObj=new GameObject("LBack_ReverseLeg");
            reverseObj.transform.SetParent(stageRoot,false);
            reverseObj.transform.position=new Vector3(-8f,1f,10f);
            var reverse=reverseObj.AddComponent<BoxCollider>();
            reverse.isTrigger=true;
            reverse.size=new Vector3(23f,3f,8f);
            var path=stageRoot.gameObject.AddComponent<CarLBackPathSensor>();
            var ps=new SerializedObject(path);
            ps.FindProperty("trial").objectReferenceValue=trial;
            ps.FindProperty("car").objectReferenceValue=car;
            ps.FindProperty("approachLeg").objectReferenceValue=approach;
            ps.FindProperty("reverseLeg").objectReferenceValue=reverse;
            var wheels=ps.FindProperty("wheels");
            var allWheels=car.GetComponentsInChildren<WheelCollider>();
            wheels.arraySize=allWheels.Length;
            for(int i=0;i<allWheels.Length;i++)
                wheels.GetArrayElementAtIndex(i).objectReferenceValue=allWheels[i];
            ps.ApplyModifiedPropertiesWithoutUndo();
            var garage=new SerializedObject(sensor);
            garage.FindProperty("pathSensor").objectReferenceValue=path;
            garage.FindProperty("requireReverseApproach").boolValue=false;
            garage.ApplyModifiedPropertiesWithoutUndo();
            Material asphalt=CreateMaterial("LBackRoad",new Color(.22f,.24f,.26f));
            Material marking=CreateMaterial("LBackBoundary",new Color(.94f,.88f,.12f));
            DrawRectangle("LBackEntrance",new Vector3(0,.055f,-7),
                new Vector2(8,38),asphalt,stageRoot);
            DrawRectangle("LBackCrossLeg",new Vector3(-8,.06f,10),
                new Vector2(23,8),asphalt,stageRoot);
            DrawRectangle("LBackParkingBay",new Vector3(-12,.075f,10),
                new Vector2(6,7.5f),CreateMaterial("LBackBay",new Color(.14f,.42f,.30f)),stageRoot);
            DrawStripe("ApproachLeft",new Vector3(-4,.11f,-7),new Vector3(.13f,.04f,38),marking,stageRoot);
            DrawStripe("ApproachRight",new Vector3(4,.11f,-7),new Vector3(.13f,.04f,38),marking,stageRoot);
            DrawStripe("CrossLegBottom",new Vector3(-8,.11f,6),new Vector3(23,.04f,.13f),marking,stageRoot);
            DrawStripe("CrossLegTop",new Vector3(-8,.11f,14),new Vector3(23,.04f,.13f),marking,stageRoot);
        }

        private static void DrawRectangle(string name,Vector3 position,Vector2 size,
            Material material,Transform parent)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);
            o.name=name;
            o.transform.position=position;
            o.transform.localScale=new Vector3(size.x,.06f,size.y);
            o.GetComponent<Collider>().enabled=false;
            o.GetComponent<Renderer>().sharedMaterial=material;
            o.transform.SetParent(parent,true);
        }

        private static void DrawStripe(string name,Vector3 position,Vector3 size,
            Material material,Transform parent)
        {
            var o=GameObject.CreatePrimitive(PrimitiveType.Cube);
            o.name=name;
            o.transform.position=position;
            o.transform.localScale=size;
            o.GetComponent<Collider>().enabled=false;
            o.GetComponent<Renderer>().sharedMaterial=material;
            o.transform.SetParent(parent,true);
        }

        private static void RebuildOfficialEight(Transform stageRoot,bool bike)
        {
            // Bagmati published track measurements:
            // Category B: outer diameter 12 m, inner diameter 6 m, center spacing 9 m.
            // A/K: outer radius 3.5 m, inner radius 1.5 m, center spacing 4.5 m.
            // Drawings are top-down reference geometry, not finished road mesh.
            float halfCenter=bike?2.25f:4.5f;
            float inner=bike?1.5f:3f;
            float outer=bike?3.5f:6f;
            float middle=(inner+outer)*.5f;
            float halfWidth=(outer-inner)*.5f;
            foreach(Transform child in stageRoot.GetComponentsInChildren<Transform>(true))
            {
                if(child==null || child==stageRoot)continue;
                string n=child.name;
                bool remove=bike ? n=="Mark" || n.StartsWith("Gate_") :
                    n.StartsWith("Left outer_") || n.StartsWith("Left inner_") ||
                    n.StartsWith("Right outer_") || n.StartsWith("Right inner_") ||
                    n.StartsWith("LeftGate_") || n.StartsWith("RightGate_");
                if(remove)UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            Material line=CreateMaterial("ConnectedFigureEightLines",new Color(.96f,.80f,.10f));
            for(int side=-1;side<=1;side+=2)
            for(int ring=0;ring<2;ring++)
            {
                float radius=ring==0?inner:outer;
                for(int k=0;k<64;k++)
                {
                    float a=2f*Mathf.PI*k/64f;
                    float b=2f*Mathf.PI*(k+1)/64f;
                    Vector3 p=new Vector3(side*halfCenter+radius*Mathf.Cos(a),.105f,
                        radius*Mathf.Sin(a));
                    Vector3 q=new Vector3(side*halfCenter+radius*Mathf.Cos(b),.105f,
                        radius*Mathf.Sin(b));
                    GameObject stripe=GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stripe.name="OfficialFigureEight_Line";
                    stripe.transform.position=(p+q)*.5f;
                    stripe.transform.rotation=Quaternion.LookRotation(q-p);
                    stripe.transform.localScale=new Vector3(.15f,.04f,Vector3.Distance(p,q));
                    stripe.GetComponent<BoxCollider>().enabled=false;
                    stripe.GetComponent<Renderer>().sharedMaterial=line;
                    stripe.transform.SetParent(stageRoot,true);
                }
            }

            var gateTransforms=new List<Transform>();
            for(int loop=0;loop<2;loop++)
            for(int i=0;i<5;i++)
            {
                float a=loop==0
                    ? Mathf.PI/4f+i*Mathf.PI/2f
                    : 3f*Mathf.PI/4f-i*Mathf.PI/2f;
                float centerX=loop==0?-halfCenter:halfCenter;
                Vector3 point=new Vector3(centerX+middle*Mathf.Cos(a),.10f,middle*Mathf.Sin(a));
                Vector3 forward=loop==0
                    ? new Vector3(-Mathf.Sin(a),0,Mathf.Cos(a))
                    : new Vector3(Mathf.Sin(a),0,-Mathf.Cos(a));
                GameObject marker=new GameObject("OfficialCheckpoint_"+loop+"_"+i);
                marker.transform.SetParent(stageRoot,false);
                marker.transform.position=point;
                marker.transform.rotation=Quaternion.LookRotation(forward);
                gateTransforms.Add(marker.transform);
                GameObject band=GameObject.CreatePrimitive(PrimitiveType.Cube);
                band.name="CheckpointMarker";
                band.transform.SetParent(marker.transform,false);
                band.transform.localPosition=Vector3.zero;
                band.transform.localScale=new Vector3(halfWidth*1.85f,.03f,.14f);
                band.GetComponent<BoxCollider>().enabled=false;
                band.GetComponent<Renderer>().sharedMaterial=CreateMaterial(
                    "ConnectedGateGreen",new Color(.10f,.78f,.32f));
            }

            MonoBehaviour sensor=bike
                ? (MonoBehaviour)stageRoot.GetComponentInChildren<TwoWheelerFigureEightSensor>()
                : stageRoot.GetComponentInChildren<CarFigureEightSensor>();
            if(sensor==null)throw new InvalidOperationException("Missing Figure-8 sensor in base course.");
            var serialized=new SerializedObject(sensor);
            serialized.FindProperty("leftCenter").vector2Value=new Vector2(-halfCenter,0f);
            serialized.FindProperty("rightCenter").vector2Value=new Vector2(halfCenter,0f);
            serialized.FindProperty(bike?"radius":"ringRadius").floatValue=middle;
            serialized.FindProperty(bike?"halfWidth":"trackHalfWidth").floatValue=halfWidth;
            serialized.FindProperty("gateHalfWidth").floatValue=halfWidth;
            var gates=serialized.FindProperty(bike?"checkpoints":"orderedGates");
            gates.arraySize=gateTransforms.Count;
            for(int i=0;i<gateTransforms.Count;i++)
                gates.GetArrayElementAtIndex(i).objectReferenceValue=gateTransforms[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
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
