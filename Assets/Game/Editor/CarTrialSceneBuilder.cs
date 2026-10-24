#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using NepalTrial.CameraSystem;
using NepalTrial.Input;
using NepalTrial.Mobile;
using NepalTrial.Trial;
using NepalTrial.Vehicles.Car;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NepalTrial.EditorTools
{
    /// <summary>Generates six INDEPENDENT provisional car-trial practice scenes.</summary>
    public static class CarTrialSceneBuilder
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string MaterialFolder = "Assets/Game/Generated/Materials";
        private static Material road, ground, cyan, yellow, red, green, dark;

        [MenuItem("Nepal Trial/Phase 2/Create Car Trial Scenes")]
        public static void CreateAll()
        {
            Directory.CreateDirectory(SceneFolder);
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.Refresh();
            PrepareMaterials();
            int created = 0;
            foreach (string id in new[] { "figure-eight", "u-turn", "hill-start",
                "traffic-light", "speed-breaker", "garage" })
            {
                string scenePath = SceneFolder + "/CarTrial_" + id + ".unity";
                if (File.Exists(scenePath))
                {
                    Debug.LogWarning("Scene already exists, preserving your edits: " + scenePath);
                    continue;
                }
                CreateCourse(id, scenePath);
                created++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Phase 2: generated " + created + " car trial scenes. Open a CarTrial_*.unity scene and press Play. Trial dimensions/penalties are provisional.");
        }

        private static void PrepareMaterials()
        {
            road = MakeMaterial("CourseRoad", new Color(.2f,.23f,.25f));
            ground = MakeMaterial("CourseGround", new Color(.22f,.36f,.21f));
            cyan = MakeMaterial("CourseCar", new Color(.07f,.8f,.85f));
            yellow = MakeMaterial("CourseYellow", new Color(.98f,.82f,.12f));
            red = MakeMaterial("CourseRed", new Color(.8f,.1f,.12f));
            green = MakeMaterial("CourseGreen", new Color(.12f,.8f,.27f));
            dark = MakeMaterial("CourseDark", new Color(.12f,.15f,.18f));
        }

        private static Material MakeMaterial(string name, Color color)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("No suitable material shader installed.");
            var material = new Material(shader);
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void CreateCourse(string id, string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Block("Ground", Vector3.down * .55f, new Vector3(130f,1f,135f), ground);
            Block("PracticeRoad", new Vector3(0,-.01f,0), new Vector3(35f,.06f,56f), road, false);
            Vector3 start = id == "figure-eight" ? new Vector3(0,1.15f,-5f) :
                new Vector3(0,1.15f,-18f);
            CarDriveController car = BuildCar(start);
            PrototypeCarInput input = car.GetComponent<PrototypeCarInput>();

            GameObject runtimeRoot = new GameObject("TrialSession_" + id);
            TrialGameplayRuntime trial = runtimeRoot.AddComponent<TrialGameplayRuntime>();
            ConfigureTrial(trial, id);

            var reporter = car.gameObject.AddComponent<CarCollisionReporter>();
            Ref(reporter, "trial", trial);

            CameraFollow(car.transform);
            CreateSun();
            Canvas canvas = CreateCanvas();
            TrialCarSessionActions actions = runtimeRoot.AddComponent<TrialCarSessionActions>();
            Ref(actions, "trial", trial);
            Ref(actions, "car", car);
            Ref(actions, "input", input);
            AddControls(canvas.transform, input, actions, trial);

            switch (id)
            {
                case "figure-eight": CreateFigureEight(trial, car); break;
                case "u-turn": CreateUTurn(trial, car); break;
                case "hill-start": CreateHillStart(trial, car); break;
                case "traffic-light": CreateTrafficLight(trial, car); break;
                case "speed-breaker": CreateSpeedBreaker(trial, car); break;
                case "garage": CreateGarage(trial, car); break;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
                throw new IOException("Unable to save scene " + scenePath);
        }

        private static void ConfigureTrial(TrialGameplayRuntime trial, string course)
        {
            var so = new SerializedObject(trial);
            so.FindProperty("courseId").stringValue = "practice-car-" + course;
            var stages = so.FindProperty("orderedStages");
            stages.arraySize = 1;
            stages.GetArrayElementAtIndex(0).stringValue = course;
            var penalties = new Dictionary<string,int>
            {
                { "figure-eight-boundary", 10 },
                { "u-turn-boundary", 35 }, { "u-turn-timeout", 35 }, { "u-turn-incomplete", 35 },
                { "hill-rollback", 35 }, { "hill-timeout", 35 },
                { "signal-red", 35 }, { "signal-yellow", 35 },
                { "breaker-speed", 35 },
                { "garage-obstruction", 35 },
                { "obstacle-hit", 35 }
            };
            var rules = so.FindProperty("penaltyRules");
            rules.arraySize = penalties.Count;
            int i = 0;
            foreach (var pair in penalties)
            {
                var element = rules.GetArrayElementAtIndex(i++);
                element.FindPropertyRelative("code").stringValue = pair.Key;
                element.FindPropertyRelative("points").intValue = pair.Value;
            }
            so.FindProperty("durationSeconds").floatValue = 180f;
            so.FindProperty("startingScore").intValue = 100;
            so.FindProperty("passingScore").intValue = 70;
            so.FindProperty("startAutomatically").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CarDriveController BuildCar(Vector3 start)
        {
            GameObject root = new GameObject("TrialCar");
            root.transform.position = start;
            Rigidbody rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.75f,.8f,3.7f);
            Block("CarVisual", start, new Vector3(1.75f,.8f,3.7f), cyan, false)
                .transform.SetParent(root.transform,true);
            CarDriveController controller = root.AddComponent<CarDriveController>();
            var wheels = new[]
            {
                new Vector3(-.84f,-.32f,1.2f),new Vector3(.84f,-.32f,1.2f),
                new Vector3(-.84f,-.32f,-1.2f),new Vector3(.84f,-.32f,-1.2f)
            };
            var names = new[] { "frontLeft", "frontRight", "rearLeft", "rearRight" };
            for (int i=0; i<4; i++)
            {
                var wheelObject = new GameObject("Wheel_" + names[i]);
                wheelObject.transform.SetParent(root.transform,false);
                wheelObject.transform.localPosition = wheels[i];
                WheelCollider wheel = wheelObject.AddComponent<WheelCollider>();
                wheel.radius = .34f;
                wheel.suspensionDistance = .28f;
                Ref(controller,names[i],wheel);
                Block("WheelVisual_" + i, root.transform.TransformPoint(wheels[i]), new Vector3(.3f,.64f,.64f), dark, false)
                    .transform.SetParent(root.transform,true);
            }
            var input = root.AddComponent<PrototypeCarInput>();
            Ref(input,"car",controller);
            return controller;
        }

        private static void CameraFollow(Transform target)
        {
            var root = new GameObject("ChaseCamera");
            root.tag = "MainCamera";
            Camera camera = root.AddComponent<Camera>();
            camera.farClipPlane = 250f;
            root.AddComponent<AudioListener>();
            root.transform.position = target.position + new Vector3(0,5,-11);
            root.AddComponent<VehicleFollowCamera>().SetTarget(target);
        }

        private static void CreateSun()
        {
            Light light = new GameObject("Sun").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f,-35f,0f);
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("TrialCanvas",typeof(RectTransform),typeof(Canvas),
                typeof(CanvasScaler),typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080);
            new GameObject("EventSystem",typeof(EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            return canvas;
        }

        private static void AddControls(Transform canvas, PrototypeCarInput input,
            TrialCarSessionActions actions, TrialGameplayRuntime trial)
        {
            Touch(canvas,"Left",new Vector2(120,115),input,MobileCarTouchControl.Control.SteerLeft);
            Touch(canvas,"Right",new Vector2(310,115),input,MobileCarTouchControl.Control.SteerRight);
            Touch(canvas,"Gas",new Vector2(-170,115),input,MobileCarTouchControl.Control.Throttle,true);
            Touch(canvas,"Brake",new Vector2(-360,115),input,MobileCarTouchControl.Control.Brake,true);
            Touch(canvas,"Reverse",new Vector2(-555,115),input,MobileCarTouchControl.Control.Reverse,true);
            Touch(canvas,"Handbrake",new Vector2(-770,115),input,MobileCarTouchControl.Control.Handbrake,true);
            Touch(canvas,"Drive",new Vector2(-170,260),input,MobileCarTouchControl.Control.Drive,true);
            Touch(canvas,"Neutral",new Vector2(-360,260),input,MobileCarTouchControl.Control.Neutral,true);
            Touch(canvas,"Park",new Vector2(-555,260),input,MobileCarTouchControl.Control.Park,true);
            var pause = UiButton(canvas,"Pause",new Vector2(-165,-80),new Vector2(170,65),true,true);
            UnityEventTools.AddPersistentListener(pause.onClick,actions.TogglePause);
            var reset = UiButton(canvas,"Restart",new Vector2(-355,-80),new Vector2(170,65),true,true);
            UnityEventTools.AddPersistentListener(reset.onClick,actions.Restart);

            Text stage = TextLabel(canvas,"Stage",new Vector2(-200,-40),new Vector2(580,58),30);
            Text score = TextLabel(canvas,"Score",new Vector2(-200,-100),new Vector2(580,58),30);
            Text time = TextLabel(canvas,"Time",new Vector2(-200,-160),new Vector2(580,58),30);
            Text status = TextLabel(canvas,"State",new Vector2(-200,-220),new Vector2(580,58),30);
            Text violations = TextLabel(canvas,"Violations",new Vector2(-200,-280),new Vector2(580,58),30);

            GameObject panel = new GameObject("ResultsPanel",typeof(RectTransform),typeof(Image));
            panel.transform.SetParent(canvas,false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f);
            rect.sizeDelta = new Vector2(540,300);
            panel.GetComponent<Image>().color = new Color(.08f,.13f,.2f,.92f);
            Text resultText = TextLabel(panel.transform,"Result",new Vector2(0,-55),new Vector2(500,165),38,false);
            Button retry = UiButton(panel.transform,"Retry",new Vector2(0,-215),
                new Vector2(220,65),false,false);
            UnityEventTools.AddPersistentListener(retry.onClick,actions.Restart);

            var hud = new GameObject("TrialSessionHud").AddComponent<TrialSessionHud>();
            Ref(hud,"trial",trial);
            Ref(hud,"stageLabel",stage);
            Ref(hud,"scoreLabel",score);
            Ref(hud,"timeLabel",time);
            Ref(hud,"stateLabel",status);
            Ref(hud,"violationsLabel",violations);
            Ref(hud,"resultsPanel",panel);
            var view = new GameObject("TrialResultText").AddComponent<CarTrialResultsView>();
            Ref(view,"trial",trial);
            Ref(view,"resultsLabel",resultText);
            panel.SetActive(false);
        }

        private static Text TextLabel(Transform parent,string initial,Vector2 offset,Vector2 size,int fontSize,bool top=true)
        {
            var go = new GameObject("Text_" + initial,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
            go.transform.SetParent(parent,false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = top ? new Vector2(.5f,1f) : new Vector2(.5f,1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = initial;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        private static Button UiButton(Transform parent,string label,Vector2 offset,Vector2 size,bool top,bool right)
        {
            var go = new GameObject(label,typeof(RectTransform),typeof(CanvasRenderer),
                typeof(Image),typeof(Button));
            go.transform.SetParent(parent,false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = top ? (right ? new Vector2(1f,1f) : new Vector2(0f,1f))
                : new Vector2(.5f,1f);
            rect.pivot = top ? (right ? new Vector2(1f,1f) : new Vector2(0f,1f))
                : new Vector2(.5f,1f);
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(.08f,.16f,.24f,.9f);
            Text text = TextLabel(go.transform,label,Vector2.zero,size,26);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin=Vector2.zero; textRect.anchorMax=Vector2.one;
            textRect.offsetMin=textRect.offsetMax=Vector2.zero;
            text.raycastTarget = false;
            return go.GetComponent<Button>();
        }

        private static void Touch(Transform canvas,string label,Vector2 offset,
            PrototypeCarInput target,MobileCarTouchControl.Control control,bool right=false)
        {
            var go = new GameObject(label,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            go.transform.SetParent(canvas,false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = right ? new Vector2(1,0) : new Vector2(0,0);
            rect.pivot = right ? new Vector2(1,0) : new Vector2(0,0);
            rect.anchoredPosition=offset;
            rect.sizeDelta=new Vector2(165,90);
            go.GetComponent<Image>().color=new Color(.06f,.16f,.25f,.85f);
            MobileCarTouchControl handler = go.AddComponent<MobileCarTouchControl>();
            Ref(handler,"target",target);
            SetInt(handler,"control",(int)control);
            Text text=TextLabel(go.transform,label,Vector2.zero,rect.sizeDelta,24);
            RectTransform t=text.GetComponent<RectTransform>();
            t.anchorMin=Vector2.zero;t.anchorMax=Vector2.one;t.offsetMin=t.offsetMax=Vector2.zero;
            text.raycastTarget=false;
        }

        private static void CreateFigureEight(TrialGameplayRuntime trial,CarDriveController car)
        {
            var transforms = new List<Transform>();
            Ring("Left outer",-7f,9.8f,yellow);Ring("Left inner",-7f,4.2f,yellow);
            Ring("Right outer",7f,9.8f,yellow);Ring("Right inner",7f,4.2f,yellow);
            float[] left = { Mathf.PI/4f,3f*Mathf.PI/4f,5f*Mathf.PI/4f,7f*Mathf.PI/4f,2f*Mathf.PI };
            float[] right = {3f*Mathf.PI/4f,Mathf.PI/4f,-Mathf.PI/4f,-3f*Mathf.PI/4f,-Mathf.PI };
            for(int i=0;i<left.Length;i++)
            {
                float angle=left[i];
                Vector3 point = new Vector3(-7f+7f*Mathf.Cos(angle),.06f,7f*Mathf.Sin(angle));
                Vector3 tangent = new Vector3(-Mathf.Sin(angle),0,Mathf.Cos(angle));
                transforms.Add(Marker("LeftGate_"+i,point,tangent).transform);
            }
            for(int i=0;i<right.Length;i++)
            {
                float angle=right[i];
                Vector3 point = new Vector3(7f+7f*Mathf.Cos(angle),.06f,7f*Mathf.Sin(angle));
                Vector3 tangent = new Vector3(Mathf.Sin(angle),0,-Mathf.Cos(angle));
                transforms.Add(Marker("RightGate_"+i,point,tangent).transform);
            }
            var sensor = new GameObject("FigureEightSensor").AddComponent<CarFigureEightSensor>();
            Ref(sensor,"trial",trial);
            Ref(sensor,"car",car);
            var wheels=car.GetComponentsInChildren<WheelCollider>();
            Array(sensor,"wheels",wheels);
            Array(sensor,"orderedGates",transforms.ToArray());
        }

        private static void Ring(string name,float centerX,float radius,Material material)
        {
            const int segments=64;
            for(int i=0;i<segments;i++)
            {
                float a = i*2f*Mathf.PI/segments;
                float b = (i+1)*2f*Mathf.PI/segments;
                Vector3 p=new Vector3(centerX+Mathf.Cos(a)*radius,.055f,Mathf.Sin(a)*radius);
                Vector3 q=new Vector3(centerX+Mathf.Cos(b)*radius,.055f,Mathf.Sin(b)*radius);
                var block=Block(name+"_"+i,(p+q)*.5f,
                    new Vector3(.1f,.035f,Vector3.Distance(p,q)),material,false);
                block.transform.rotation = Quaternion.LookRotation(q-p);
            }
        }

        private static GameObject Marker(string name,Vector3 position,Vector3 heading)
        {
            var root = new GameObject(name);
            root.transform.position=position;
            root.transform.rotation=Quaternion.LookRotation(heading);
            var line=Block(name+"_Line",position,new Vector3(4f,.04f,.1f),green,false);
            line.transform.SetParent(root.transform,true);
            return root;
        }

        private static void CreateUTurn(TrialGameplayRuntime trial,CarDriveController car)
        {
            Block("UTurnArea",new Vector3(0,.035f,4),new Vector3(18f,.07f,34f),road,false);
            Block("LeftMark",new Vector3(-9f,.08f,4),new Vector3(.17f,.06f,34f),yellow,false);
            Block("RightMark",new Vector3(9f,.08f,4),new Vector3(.17f,.06f,34f),yellow,false);
            var regionObject = new GameObject("UTurnDrivingRegion");
            regionObject.transform.position = new Vector3(0,1.4f,4f);
            var region = regionObject.AddComponent<BoxCollider>();
            region.isTrigger = true;
            region.size = new Vector3(18f,4f,34f);
            var sensor=new GameObject("UTurnObserver").AddComponent<CarUTurnSensor>();
            Ref(sensor,"trial",trial); Ref(sensor,"car",car);
            var boundary = new GameObject("UTurnBoundaryObserver").AddComponent<CarUTurnBoundarySensor>();
            Ref(boundary,"trial",trial); Ref(boundary,"maneuver",sensor);
            Ref(boundary,"drivingRegion",region);
            Ref(boundary,"vehicleBody",car.GetComponent<BoxCollider>());
            ManeuverGate("BeginUTurn",new Vector3(0,1,-8),0,car,sensor,null);
            ManeuverGate("ExitUTurn",new Vector3(0,1,-10),1,car,sensor,null);
        }

        private static void CreateHillStart(TrialGameplayRuntime trial,CarDriveController car)
        {
            var ramp=Block("DrivableRamp",new Vector3(0,.65f,0),new Vector3(8f,.35f,12f),road,true);
            ramp.transform.rotation=Quaternion.Euler(-6f,0,0);
            var observer=new GameObject("HillObserver").AddComponent<CarHillStartSensor>();
            Ref(observer,"trial",trial);Ref(observer,"car",car);
            ManeuverGate("BeginHill",new Vector3(0,1.4f,-3),2,car,null,observer);
            ManeuverGate("ExitHill",new Vector3(0,2f,5),3,car,null,observer);
            Block("TopPlatform",new Vector3(0,1.26f,9.5f),new Vector3(8f,.3f,8f),road,true);
        }

        private static void ManeuverGate(string name,Vector3 position,int action,
            CarDriveController car,CarUTurnSensor uTurn,CarHillStartSensor hill)
        {
            var root=new GameObject(name);
            root.transform.position=position;
            BoxCollider box=root.AddComponent<BoxCollider>();
            box.isTrigger=true;
            box.size=new Vector3(12f,4f,1f);
            CarManeuverGate gate=root.AddComponent<CarManeuverGate>();
            Ref(gate,"car",car);
            if(uTurn!=null)Ref(gate,"uTurn",uTurn);
            if(hill!=null)Ref(gate,"hill",hill);
            SetInt(gate,"action",action);
            Block(name+"_Stripe",new Vector3(position.x,.06f,position.z),
                new Vector3(12f,.03f,.22f),green,false);
        }

        private static void CreateTrafficLight(TrialGameplayRuntime trial,CarDriveController car)
        {
            var root=new GameObject("TrafficLightStopLine");
            root.transform.position=new Vector3(0,1.2f,2f);
            var collider=root.AddComponent<BoxCollider>();
            collider.size=new Vector3(13f,3.5f,1f);
            collider.isTrigger=true;
            var sensor=root.AddComponent<CarTrafficSignalSensor>();
            Ref(sensor,"trial",trial); Ref(sensor,"car",car);
            Block("StopStripe",new Vector3(0,.065f,2f),new Vector3(13f,.03f,.3f),yellow,false);
            Block("SignalPost",new Vector3(7f,1.65f,2f),new Vector3(.3f,3.3f,.3f),dark,true);
            var lamp=Block("SignalLamp",new Vector3(7f,3.3f,2f),new Vector3(.75f,.85f,.55f),red,false);
            var cycle=new GameObject("PracticeSignalCycle").AddComponent<PracticeTrafficSignalCycle>();
            Ref(cycle,"stopLine",sensor); Ref(cycle,"lamp",lamp.GetComponent<Renderer>());
        }

        private static void CreateSpeedBreaker(TrialGameplayRuntime trial,CarDriveController car)
        {
            var root=new GameObject("SpeedBreakerTrigger");
            root.transform.position=new Vector3(0,1.2f,1);
            var collider=root.AddComponent<BoxCollider>();
            collider.isTrigger=true;
            collider.size=new Vector3(12f,3.5f,1.5f);
            var sensor=root.AddComponent<CarSpeedBreakerSensor>();
            Ref(sensor,"trial",trial); Ref(sensor,"car",car);
            Block("PhysicalBump",new Vector3(0,.07f,1f),new Vector3(12f,.12f,.65f),yellow,true);
        }

        private static void CreateGarage(TrialGameplayRuntime trial,CarDriveController car)
        {
            var root=new GameObject("ParkingVolume");
            root.transform.position=new Vector3(0,1.25f,2f);
            var area=root.AddComponent<BoxCollider>();
            area.isTrigger=true;
            area.size=new Vector3(5.4f,3.5f,7.5f);
            var sensor=root.AddComponent<CarGarageParkingSensor>();
            Ref(sensor,"trial",trial); Ref(sensor,"car",car);
            Ref(sensor,"vehicleBody",car.GetComponent<BoxCollider>());
            Block("ParkingFloor",new Vector3(0,.04f,2f),new Vector3(5.3f,.06f,7.4f),green,false);
            Block("GarageLeftWall",new Vector3(-3f,.8f,2f),new Vector3(.35f,1.6f,8f),red,true)
                .AddComponent<TrialObstacle>();
            Block("GarageRightWall",new Vector3(3f,.8f,2f),new Vector3(.35f,1.6f,8f),red,true)
                .AddComponent<TrialObstacle>();
            Block("GarageBackWall",new Vector3(0,.8f,6.3f),new Vector3(6f,1.6f,.35f),red,true)
                .AddComponent<TrialObstacle>();
        }

        private static GameObject Block(string name,Vector3 position,Vector3 scale,
            Material material,bool collider=true)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name=name;
            go.transform.position=position;
            go.transform.localScale=scale;
            var box=go.GetComponent<BoxCollider>();
            if (!collider) box.enabled=false;
            go.GetComponent<Renderer>().sharedMaterial=material;
            return go;
        }

        private static void Ref(Component component,string field,UnityEngine.Object target)
        {
            var so=new SerializedObject(component);
            var property=so.FindProperty(field);
            if (property==null) throw new InvalidOperationException(component.GetType().Name+" missing field "+field);
            property.objectReferenceValue=target;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Component component,string field,int value)
        {
            var so=new SerializedObject(component);
            var property=so.FindProperty(field);
            if (property==null) throw new InvalidOperationException(component.GetType().Name+" missing field "+field);
            property.enumValueIndex=value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Array<T>(Component component,string field,T[] items) where T:UnityEngine.Object
        {
            var so=new SerializedObject(component);
            var property=so.FindProperty(field);
            if (property==null) throw new InvalidOperationException(component.GetType().Name+" missing field "+field);
            property.arraySize=items.Length;
            for (int i=0;i<items.Length;i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue=items[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
