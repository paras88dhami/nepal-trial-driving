#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using NepalTrial.CameraSystem;
using NepalTrial.Input;
using NepalTrial.Trial;
using NepalTrial.Vehicles.TwoWheeler;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NepalTrial.EditorTools
{
    /// <summary>Generates 14 independent motorcycle/scooter practice scenes (7 stages per vehicle).</summary>
    public static class TwoWheelerTrialSceneBuilder
    {
        private const string Folder="Assets/Scenes";
        private const string Materials="Assets/Game/Generated/Materials";
        private static Material ground,road,line,body,red,green,dark;

        [MenuItem("Nepal Trial/Phase 3/Create Motorcycle and Scooter Trials")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            Directory.CreateDirectory(Folder);
            Directory.CreateDirectory(Materials);
            AssetDatabase.Refresh();
            ground=Mat("BikeGround",new Color(.2f,.32f,.2f));
            road=Mat("BikeRoad",new Color(.16f,.2f,.24f));
            line=Mat("BikeMark",new Color(.97f,.85f,.16f));
            body=Mat("BikeBody",new Color(.05f,.65f,.9f));
            red=Mat("BikeRed",new Color(.85f,.12f,.13f));
            green=Mat("BikeGreen",new Color(.12f,.83f,.28f));
            dark=Mat("BikeTire",new Color(.08f,.09f,.10f));
            int created=0;
            try
            {
                foreach (var kind in new[]{TwoWheelerKind.Motorcycle,TwoWheelerKind.Scooter})
                foreach (var course in new[]{"figure-eight","narrow-plank","u-turn",
                    "traffic-light","speed-breaker","ramp","behavior"})
                {
                    string path=Folder+"/BikeTrial_"+kind.ToString().ToLowerInvariant()+"_"+course+".unity";
                    if(File.Exists(path)) { Debug.Log("Preserving "+path); continue; }
                    MakeScene(kind,course,path);
                    created++;
                }
            }
            finally
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                if(!string.IsNullOrEmpty(previous) && File.Exists(previous))
                    EditorSceneManager.OpenScene(previous,OpenSceneMode.Single);
                else EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            }
            Debug.Log("Phase 3: created "+created+" independent scenes. Open BikeTrial_*.unity and test motorcycle/scooter physics. Course measurements and scores are provisional.");
        }

        private static Material Mat(string name,Color color)
        {
            string path=Materials+"/"+name+".mat";
            Material exists=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(exists!=null)return exists;
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null)throw new InvalidOperationException("URP/Standard shader unavailable");
            var material=new Material(shader);
            material.color=color;
            AssetDatabase.CreateAsset(material,path);
            return material;
        }

        private static void MakeScene(TwoWheelerKind kind,string course,string path)
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Block("Ground",new Vector3(0,-.55f,0),new Vector3(100f,1.1f,120f),ground,true);
            Block("Road",new Vector3(0,.015f,0),new Vector3(24f,.03f,60f),road,false);
            Vector3 start=course=="figure-eight"?new Vector3(-10f,1.0f,-7f):new Vector3(0,1.0f,-15f);
            float heading=course=="figure-eight"?90f:0f;
            var vehicle=BuildVehicle(kind,start,heading);
            var input=vehicle.gameObject.AddComponent<TwoWheelerInput>();
            input.SetVehicle(vehicle);
            var session=new GameObject("TwoWheelerTrialSession_"+course);
            var trial=session.AddComponent<TrialGameplayRuntime>();
            ConfigureTrial(trial,kind,course);
            var actions=session.AddComponent<TwoWheelerSessionActions>();
            Ref(actions,"vehicle",vehicle);
            Ref(actions,"input",input);
            Ref(actions,"trial",trial);
            var fall=session.AddComponent<TwoWheelerFallReporter>();
            Ref(fall,"vehicle",vehicle);
            Ref(fall,"trial",trial);
            var stalls=session.AddComponent<TwoWheelerStallReporter>();
            Ref(stalls,"vehicle",vehicle);
            Ref(stalls,"trial",trial);

            var cameraRoot=new GameObject("ChaseCamera");
            cameraRoot.tag="MainCamera";
            cameraRoot.transform.position=start+new Vector3(0,5,-9);
            cameraRoot.AddComponent<Camera>().farClipPlane=200f;
            cameraRoot.AddComponent<AudioListener>();
            cameraRoot.AddComponent<VehicleFollowCamera>().SetTarget(vehicle.transform);
            var sunlight=new GameObject("Sun").AddComponent<Light>();
            sunlight.type=LightType.Directional;
            sunlight.transform.rotation=Quaternion.Euler(45f,-30f,0f);

            Canvas canvas=MakeCanvas();
            AddHud(canvas.transform,trial,vehicle,input,actions);
            switch(course)
            {
                case "figure-eight": FigureEight(trial,vehicle); break;
                case "narrow-plank": Plank(trial,vehicle); break;
                case "u-turn": UTurn(trial,vehicle); break;
                case "traffic-light": Traffic(trial,vehicle); break;
                case "speed-breaker": Bumps(trial,vehicle); break;
                case "ramp": Ramp(trial,vehicle); break;
                case "behavior": Behavior(trial,canvas.transform); break;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if(!EditorSceneManager.SaveScene(scene,path))
                throw new IOException("Unable to save "+path);
        }

        private static void ConfigureTrial(TrialGameplayRuntime trial,TwoWheelerKind kind,string course)
        {
            var so=new SerializedObject(trial);
            so.FindProperty("courseId").stringValue="practice-"+kind.ToString().ToLowerInvariant()+"-"+course;
            var stages=so.FindProperty("orderedStages");
            stages.arraySize=1;
            stages.GetArrayElementAtIndex(0).stringValue=course;
            var penalties=new Dictionary<string,int>
            {
                {"figure-eight-boundary",5},{"plank-foot-down",35},{"plank-off",35},
                {"plank-timeout",35},{"u-turn-boundary",35},{"u-turn-incomplete",35},
                {"u-turn-timeout",35},{"u-turn-no-indicator",10},
                {"signal-red",15},{"signal-yellow",15},
                {"breaker-speed",10},{"breaker-foot-down",5},
                {"ramp-rollback",35},{"ramp-no-stop",35},{"ramp-timeout",35},
                {"ramp-foot-down",35},{"foot-down",5},{"two-wheeler-fall",35},
                {"behavior-incomplete",10},
                {"figure-eight-stall",35},{"ramp-first-stall",0},{"ramp-repeat-stall",35},
                {"breaker-first-stall",5},{"breaker-repeat-stall",35},{"engine-stall",10}
            };
            var rules=so.FindProperty("penaltyRules");
            rules.arraySize=penalties.Count;
            int i=0;
            foreach(var pair in penalties)
            {
                var p=rules.GetArrayElementAtIndex(i++);
                p.FindPropertyRelative("code").stringValue=pair.Key;
                p.FindPropertyRelative("points").intValue=pair.Value;
            }
            so.FindProperty("durationSeconds").floatValue=180f;
            so.FindProperty("startingScore").intValue=100;
            so.FindProperty("passingScore").intValue=70;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TwoWheelerRigidbodyController BuildVehicle(TwoWheelerKind kind,Vector3 position,float heading)
        {
            var root=new GameObject(kind.ToString());
            root.transform.position=position;
            root.transform.rotation=Quaternion.Euler(0,heading,0);
            var bodyCollider=root.AddComponent<BoxCollider>();
            bodyCollider.center=new Vector3(0,.06f,0);
            bodyCollider.size=new Vector3(.65f,.7f,1.8f);
            var rigidbody=root.AddComponent<Rigidbody>();
            rigidbody.interpolation=RigidbodyInterpolation.Interpolate;
            var ride=root.AddComponent<TwoWheelerRigidbodyController>();
            Int(ride,"kind",(int)kind);
            var chassis=Block("Chassis",position+new Vector3(0,.05f,0),
                new Vector3(.48f,.35f,1.28f),body,false);
            chassis.transform.SetParent(root.transform,true);
            var seat=Block("Seat",position+new Vector3(0,.32f,-.2f),
                new Vector3(.35f,.12f,.65f),dark,false);
            seat.transform.SetParent(root.transform,true);

            var offsets=new[]{new Vector3(0,-.4f,.68f),new Vector3(0,-.4f,-.68f)};
            for(int i=0;i<2;i++)
            {
                var wheelObject=new GameObject(i==0?"FrontWheelCollider":"RearWheelCollider");
                wheelObject.transform.SetParent(root.transform,false);
                wheelObject.transform.localPosition=offsets[i];
                var wheel=wheelObject.AddComponent<WheelCollider>();
                wheel.radius=.34f;
                wheel.suspensionDistance=.23f;
                var spring=wheel.suspensionSpring;
                spring.spring=16000f;
                spring.damper=1900f;
                spring.targetPosition=.5f;
                wheel.suspensionSpring=spring;
                var forward=wheel.forwardFriction;
                forward.stiffness=1.25f;
                wheel.forwardFriction=forward;
                var sideways=wheel.sidewaysFriction;
                sideways.stiffness=1.55f;
                wheel.sidewaysFriction=sideways;
                Ref(ride,i==0?"frontWheel":"rearWheel",wheel);

                var pivot=new GameObject(i==0?"FrontWheelVisual":"RearWheelVisual");
                pivot.transform.SetParent(root.transform,false);
                pivot.transform.localPosition=offsets[i];
                var tire=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tire.name="TireMesh";
                tire.transform.SetParent(pivot.transform,false);
                tire.transform.localRotation=Quaternion.Euler(0,0,90f);
                tire.transform.localScale=new Vector3(.34f,.13f,.34f);
                tire.GetComponent<Collider>().enabled=false;
                tire.GetComponent<Renderer>().sharedMaterial=dark;
                Ref(ride,i==0?"frontWheelVisual":"rearWheelVisual",pivot.transform);
            }
            return ride;
        }

        private static Canvas MakeCanvas()
        {
            var root=new GameObject("BikeCanvas",typeof(RectTransform),typeof(Canvas),
                typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=root.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1920,1080);
            new GameObject("EventSystem",typeof(EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            return canvas;
        }

        private static void AddHud(Transform canvas,TrialGameplayRuntime trial,
            TwoWheelerRigidbodyController ride,TwoWheelerInput input,TwoWheelerSessionActions actions)
        {
            Touch(canvas,"Left",new Vector2(100,110),input,TwoWheelerInput.Control.Left);
            Touch(canvas,"Right",new Vector2(265,110),input,TwoWheelerInput.Control.Right);
            Touch(canvas,"Throttle",new Vector2(-155,110),input,TwoWheelerInput.Control.Throttle,true);
            Touch(canvas,"Front brake",new Vector2(-320,110),input,TwoWheelerInput.Control.FrontBrake,true);
            Touch(canvas,"Rear brake",new Vector2(-485,110),input,TwoWheelerInput.Control.RearBrake,true);
            Touch(canvas,"Foot",new Vector2(-650,110),input,TwoWheelerInput.Control.FootDown,true);
            Touch(canvas,"Clutch",new Vector2(100,225),input,TwoWheelerInput.Control.Clutch);
            Touch(canvas,"Gear +",new Vector2(265,225),input,TwoWheelerInput.Control.ShiftUp);
            Touch(canvas,"Gear -",new Vector2(430,225),input,TwoWheelerInput.Control.ShiftDown);
            Touch(canvas,"Ignition",new Vector2(595,225),input,TwoWheelerInput.Control.Ignition);
            Touch(canvas,"Signal L",new Vector2(100,335),input,TwoWheelerInput.Control.LeftIndicator);
            Touch(canvas,"Signal R",new Vector2(265,335),input,TwoWheelerInput.Control.RightIndicator);
            Text stage=Label(canvas,"Stage",new Vector2(0,-40),580,28);
            Text score=Label(canvas,"Score",new Vector2(0,-90),580,28);
            Text timer=Label(canvas,"Time",new Vector2(0,-140),580,28);
            Text status=Label(canvas,"State",new Vector2(0,-190),580,28);
            Text violations=Label(canvas,"Violations",new Vector2(0,-240),580,28);
            Text dash=Label(canvas,"Dashboard",new Vector2(0,-300),950,26);
            var hud=new GameObject("TrialHud").AddComponent<TrialSessionHud>();
            Ref(hud,"trial",trial);
            Ref(hud,"stageLabel",stage);
            Ref(hud,"scoreLabel",score);
            Ref(hud,"timeLabel",timer);
            Ref(hud,"stateLabel",status);
            Ref(hud,"violationsLabel",violations);
            var dashboard=new GameObject("BikeDashboard").AddComponent<TwoWheelerDashboardHud>();
            Ref(dashboard,"vehicle",ride);
            Ref(dashboard,"dashboardText",dash);

            Button pause=Button(canvas,"Pause",new Vector2(-145,-55),true);
            UnityEventTools.AddPersistentListener(pause.onClick,actions.TogglePause);
            Button restart=Button(canvas,"Restart",new Vector2(-305,-55),true);
            UnityEventTools.AddPersistentListener(restart.onClick,actions.Restart);

            GameObject results=new GameObject("ResultPanel",typeof(RectTransform),typeof(Image));
            results.transform.SetParent(canvas,false);
            var rect=results.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.sizeDelta=new Vector2(600,340);
            results.GetComponent<Image>().color=new Color(.07f,.12f,.19f,.92f);
            Text result=Label(results.transform,"Result",new Vector2(0,-60),550,40);
            result.GetComponent<RectTransform>().anchorMin=
                result.GetComponent<RectTransform>().anchorMax=new Vector2(.5f,1f);
            var retry=Button(results.transform,"Retry",new Vector2(0,-240),false);
            UnityEventTools.AddPersistentListener(retry.onClick,actions.Restart);
            Ref(hud,"resultsPanel",results);
            var view=new GameObject("ResultView").AddComponent<CarTrialResultsView>();
            Ref(view,"trial",trial);
            Ref(view,"resultsLabel",result);
            results.SetActive(false);
        }

        private static Text Label(Transform parent,string text,Vector2 offset,float width,int size)
        {
            var root=new GameObject("Label_"+text,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Text));
            root.transform.SetParent(parent,false);
            var rect=root.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,1f);
            rect.anchoredPosition=offset;
            rect.sizeDelta=new Vector2(width,55);
            var label=root.GetComponent<Text>();
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text=text;
            label.fontSize=size;
            label.color=Color.white;
            label.alignment=TextAnchor.MiddleCenter;
            label.raycastTarget=false;
            return label;
        }

        private static Button Button(Transform canvas,string caption,Vector2 offset,bool topRight)
        {
            var root=new GameObject(caption,typeof(RectTransform),typeof(CanvasRenderer),
                typeof(Image),typeof(Button));
            root.transform.SetParent(canvas,false);
            RectTransform rect=root.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=topRight?Vector2.one:new Vector2(.5f,1f);
            rect.pivot=topRight?Vector2.one:new Vector2(.5f,1f);
            rect.sizeDelta=new Vector2(140,62);
            rect.anchoredPosition=offset;
            root.GetComponent<Image>().color=new Color(.08f,.17f,.25f,.95f);
            Text text=Label(root.transform,caption,Vector2.zero,140,23);
            RectTransform labelRect=text.GetComponent<RectTransform>();
            labelRect.anchorMin=Vector2.zero;labelRect.anchorMax=Vector2.one;
            labelRect.offsetMin=labelRect.offsetMax=Vector2.zero;
            return root.GetComponent<Button>();
        }

        private static void Touch(Transform canvas,string caption,Vector2 offset,
            TwoWheelerInput input,TwoWheelerInput.Control action,bool right=false)
        {
            var root=new GameObject(caption,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            root.transform.SetParent(canvas,false);
            var rect=root.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=right?new Vector2(1f,0f):new Vector2(0f,0f);
            rect.pivot=right?new Vector2(1f,0f):new Vector2(0f,0f);
            rect.anchoredPosition=offset;
            rect.sizeDelta=new Vector2(150,85);
            root.GetComponent<Image>().color=new Color(.08f,.2f,.29f,.88f);
            var touch=root.AddComponent<MobileTwoWheelerTouchControl>();
            Ref(touch,"input",input);
            Enum(touch,"control",(int)action);
            Text text=Label(root.transform,caption,Vector2.zero,150,21);
            RectTransform label=text.GetComponent<RectTransform>();
            label.anchorMin=Vector2.zero;label.anchorMax=Vector2.one;
            label.offsetMin=label.offsetMax=Vector2.zero;
        }

        private static void FigureEight(TrialGameplayRuntime trial,TwoWheelerRigidbodyController ride)
        {
            foreach (float center in new[]{-7f,7f})
                foreach (float r in new[]{5.2f,8.8f})Ring(center,r);
            var gates=new List<Transform>();
            for(int i=0;i<5;i++)
            {
                float a=-Mathf.PI*.5f + i*2f*Mathf.PI/4f;
                Vector3 p=new Vector3(-7f+7f*Mathf.Cos(a),.06f,7f*Mathf.Sin(a));
                Vector3 direction=new Vector3(-Mathf.Sin(a),0,Mathf.Cos(a));
                gates.Add(Gate("Left_"+i,p,direction));
            }
            for(int i=0;i<5;i++)
            {
                float a=Mathf.PI*.5f-i*2f*Mathf.PI/4f;
                Vector3 p=new Vector3(7f+7f*Mathf.Cos(a),.06f,7f*Mathf.Sin(a));
                Vector3 direction=new Vector3(Mathf.Sin(a),0,-Mathf.Cos(a));
                gates.Add(Gate("Right_"+i,p,direction));
            }
            var sensor=new GameObject("BikeFigureEightSensor").AddComponent<TwoWheelerFigureEightSensor>();
            Ref(sensor,"trial",trial);
            Ref(sensor,"vehicle",ride);
            Ref(sensor,"frontWheel",ride.transform.Find("FrontWheelCollider").GetComponent<WheelCollider>());
            Ref(sensor,"rearWheel",ride.transform.Find("RearWheelCollider").GetComponent<WheelCollider>());
            Array(sensor,"checkpoints",gates.ToArray());
            var dashboard=UnityEngine.Object.FindFirstObjectByType<TwoWheelerDashboardHud>();
            if(dashboard!=null)Ref(dashboard,"figureEight",sensor);
        }

        private static void Ring(float centerX,float radius)
        {
            const int segments=64;
            for(int i=0;i<segments;i++)
            {
                float a=2f*Mathf.PI*i/segments;
                float b=2f*Mathf.PI*(i+1)/segments;
                Vector3 p=new Vector3(centerX+radius*Mathf.Cos(a),.065f,radius*Mathf.Sin(a));
                Vector3 q=new Vector3(centerX+radius*Mathf.Cos(b),.065f,radius*Mathf.Sin(b));
                var seg=Block("Mark", (p+q)*.5f,new Vector3(.08f,.04f,Vector3.Distance(p,q)),line,false);
                seg.transform.rotation=Quaternion.LookRotation(q-p);
            }
        }
        private static Transform Gate(string name,Vector3 position,Vector3 direction)
        {
            var root=new GameObject("Gate_"+name);
            root.transform.position=position;
            root.transform.rotation=Quaternion.LookRotation(direction);
            var marker=Block("GateStripe",position,new Vector3(3.2f,.04f,.12f),green,false);
            marker.transform.SetParent(root.transform,true);
            return root.transform;
        }

        private static void Plank(TrialGameplayRuntime trial,TwoWheelerRigidbodyController ride)
        {
            Block("RaisedPlank",new Vector3(0,.08f,1.5f),new Vector3(.9f,.15f,12f),road,true);
            var root=new GameObject("PlankContactArea");
            root.transform.position=new Vector3(0,1f,1.5f);
            var box=root.AddComponent<BoxCollider>();
            box.isTrigger=true;
            box.size=new Vector3(.88f,3f,12f);
            var sensor=root.AddComponent<TwoWheelerPlankSensor>();
            Ref(sensor,"trial",trial);Ref(sensor,"vehicle",ride);Ref(sensor,"plankRegion",box);
            Ref(sensor,"frontWheel",ride.transform.Find("FrontWheelCollider").GetComponent<WheelCollider>());
            Ref(sensor,"rearWheel",ride.transform.Find("RearWheelCollider").GetComponent<WheelCollider>());
            LineAt(-4.5f);LineAt(7.5f);
        }

        private static void UTurn(TrialGameplayRuntime trial,TwoWheelerRigidbodyController ride)
        {
            Block("UTurnLane",new Vector3(0,.04f,4),new Vector3(15f,.07f,31f),road,false);
            var region=new GameObject("UTurnRideArea");
            region.transform.position=new Vector3(0,1.1f,4f);
            var area=region.AddComponent<BoxCollider>();
            area.isTrigger=true;
            area.size=new Vector3(15f,4f,31f);
            var sensor=new GameObject("UTurnSensor").AddComponent<TwoWheelerUTurnSensor>();
            Ref(sensor,"trial",trial);Ref(sensor,"vehicle",ride);Ref(sensor,"ridingRegion",area);
            Maneuver("BeginUTurn",new Vector3(0,1,-8),0,trial,ride,sensor,null);
            Maneuver("ExitUTurn",new Vector3(0,1,-10),1,trial,ride,sensor,null);
            Block("LeftBoundary",new Vector3(-7.5f,.09f,4f),new Vector3(.14f,.05f,31f),line,false);
            Block("RightBoundary",new Vector3(7.5f,.09f,4f),new Vector3(.14f,.05f,31f),line,false);
        }

        private static void Traffic(TrialGameplayRuntime trial,TwoWheelerRigidbodyController ride)
        {
            var lineObject=new GameObject("TrafficStopLine");
            lineObject.transform.position=new Vector3(0,1.1f,2f);
            BoxCollider box=lineObject.AddComponent<BoxCollider>();
            box.isTrigger=true;
            box.size=new Vector3(12f,3f,.9f);
            var sensor=lineObject.AddComponent<TwoWheelerTrafficSignalSensor>();
            Ref(sensor,"trial",trial);Ref(sensor,"vehicle",ride);
            LineAt(2f);
            Block("SignalPost",new Vector3(6f,1.8f,2f),new Vector3(.3f,3.6f,.3f),dark,true);
            var lamp=Block("SignalLamp",new Vector3(6f,3.5f,2f),new Vector3(.8f,.8f,.6f),red,false);
            var cycle=new GameObject("SignalCycle").AddComponent<TwoWheelerTrafficCycle>();
            Ref(cycle,"stopLine",sensor);Ref(cycle,"lamp",lamp.GetComponent<Renderer>());
        }

        private static void Bumps(TrialGameplayRuntime trial,TwoWheelerRigidbodyController ride)
        {
            var sensorObject=new GameObject("BumpyZone");
            sensorObject.transform.position=new Vector3(0,1.2f,2.5f);
            BoxCollider zone=sensorObject.AddComponent<BoxCollider>();
            zone.isTrigger=true;
            zone.size=new Vector3(10f,3.2f,7f);
            var sensor=sensorObject.AddComponent<TwoWheelerBumpySensor>();
            Ref(sensor,"trial",trial);Ref(sensor,"vehicle",ride);
            for(int i=0;i<5;i++)
                Block("Bump_"+i,new Vector3(0,.05f,i*1.25f),
                    new Vector3(9f,.12f,.3f),line,true);
        }

        private static void Ramp(TrialGameplayRuntime trial,TwoWheelerRigidbodyController ride)
        {
            // Connected ascent, flat crest, and descent; test colliders and joins in PlayMode.
            var ascent=Block("AscendingRamp",new Vector3(0,1f,-3f),
                new Vector3(7f,.28f,12f),road,true);
            ascent.transform.rotation=Quaternion.Euler(-9f,0,0);
            Block("RampCrest",new Vector3(0,1.95f,5f),
                new Vector3(7f,.28f,4f),road,true);
            var descent=Block("DescendingRamp",new Vector3(0,1f,13f),
                new Vector3(7f,.28f,12f),road,true);
            descent.transform.rotation=Quaternion.Euler(9f,0,0);
            var stopObj=new GameObject("AscendingStopZone");
            stopObj.transform.position=new Vector3(0,1.5f,-2f);
            BoxCollider stop=stopObj.AddComponent<BoxCollider>();
            stop.isTrigger=true;
            stop.size=new Vector3(7f,4f,2f);
            var descentMarker=new GameObject("DescentStart");
            descentMarker.transform.position=new Vector3(0,1.9f,7f);
            var observer=new GameObject("RampObserver").AddComponent<TwoWheelerRampSensor>();
            observer.transform.forward=Vector3.forward;
            Ref(observer,"trial",trial);Ref(observer,"vehicle",ride);
            Ref(observer,"uphillDirection",observer.transform);Ref(observer,"stopZone",stop);
            Ref(observer,"descentStart",descentMarker.transform);
            Maneuver("BeginRamp",new Vector3(0,1,-8f),2,trial,ride,null,observer);
            Maneuver("ExitRamp",new Vector3(0,1,20f),3,trial,ride,null,observer);
            LineAt(-2f);
        }

        private static void Behavior(TrialGameplayRuntime trial,Transform canvas)
        {
            var sensor=new GameObject("PreRideChecklist").AddComponent<TwoWheelerBehaviorSensor>();
            Ref(sensor,"trial",trial);
            Text info=Label(canvas,"Check safety items",new Vector2(0,-380),750,28);
            Ref(sensor,"checklistLabel",info);
            string[] names={"Helmet","Mirrors","Brakes","Tires","Submit"};
            for(int i=0;i<names.Length;i++)
            {
                var button=Button(canvas,names[i],new Vector2(-205f-(i*165f),-155f),true);
                switch(i)
                {
                    case 0:UnityEventTools.AddPersistentListener(button.onClick,sensor.CheckHelmet);break;
                    case 1:UnityEventTools.AddPersistentListener(button.onClick,sensor.CheckMirrors);break;
                    case 2:UnityEventTools.AddPersistentListener(button.onClick,sensor.CheckBrakes);break;
                    case 3:UnityEventTools.AddPersistentListener(button.onClick,sensor.CheckTires);break;
                    default:UnityEventTools.AddPersistentListener(button.onClick,sensor.Submit);break;
                }
            }
        }

        private static void Maneuver(string name,Vector3 position,int action,
            TrialGameplayRuntime trial,TwoWheelerRigidbodyController ride,
            TwoWheelerUTurnSensor uTurn,TwoWheelerRampSensor ramp)
        {
            var root=new GameObject(name);
            root.transform.position=position;
            var box=root.AddComponent<BoxCollider>();
            box.isTrigger=true;
            box.size=new Vector3(10f,4f,.8f);
            var gate=root.AddComponent<TwoWheelerManeuverGate>();
            Ref(gate,"trial",trial);Ref(gate,"vehicle",ride);
            if(uTurn!=null)Ref(gate,"uTurn",uTurn);
            if(ramp!=null)Ref(gate,"ramp",ramp);
            Enum(gate,"action",action);
            LineAt(position.z);
        }

        private static void LineAt(float z)
        {
            Block("Line",new Vector3(0,.07f,z),new Vector3(10f,.04f,.18f),line,false);
        }

        private static GameObject Block(string name,Vector3 position,Vector3 scale,Material material,bool physical)
        {
            GameObject item=GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name=name;
            item.transform.position=position;
            item.transform.localScale=scale;
            if(!physical)item.GetComponent<BoxCollider>().enabled=false;
            item.GetComponent<Renderer>().sharedMaterial=material;
            return item;
        }

        private static void Ref(Component component,string property,UnityEngine.Object value)
        {
            var so=new SerializedObject(component);
            var field=so.FindProperty(property);
            if(field==null)throw new InvalidOperationException(component.GetType().Name+"."+property+" missing");
            field.objectReferenceValue=value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Enum(Component component,string property,int index)
        {
            var so=new SerializedObject(component);
            var field=so.FindProperty(property);
            if(field==null)throw new InvalidOperationException(component.GetType().Name+"."+property+" missing");
            field.enumValueIndex=index;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Array<T>(Component component,string property,T[] values)
            where T:UnityEngine.Object
        {
            var so=new SerializedObject(component);
            var field=so.FindProperty(property);
            if(field==null)throw new InvalidOperationException(component.GetType().Name+"."+property+" missing");
            field.arraySize=values.Length;
            for(int i=0;i<values.Length;i++)field.GetArrayElementAtIndex(i).objectReferenceValue=values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
