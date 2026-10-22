#if UNITY_EDITOR
using NepalTrial.CameraSystem;
using NepalTrial.Input;
using NepalTrial.Mobile;
using NepalTrial.Trial;
using NepalTrial.Vehicles.Car;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NepalTrial.EditorTools
{
    /// <summary>Editor menu creates a disposable prototype scene using primitive meshes.</summary>
    public static class NepalTrialPrototypeBuilder
    {
        [MenuItem("Nepal Trial/Create Prototype Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            MakeCube("Ground", new Vector3(0,-.5f,0), new Vector3(75,1,65), Color.gray);
            var car = new GameObject("PrototypeCar");
            car.transform.position = new Vector3(-7,1.2f,-12);
            car.AddComponent<Rigidbody>();
            var carBody = MakeCube("CarBody", car.transform.position, new Vector3(1.75f,.7f,3.7f), Color.cyan);
            carBody.transform.SetParent(car.transform,true);
            var bodyCollider = carBody.GetComponent<BoxCollider>();
            bodyCollider.enabled = false;
            car.AddComponent<BoxCollider>().size = new Vector3(1.75f,.7f,3.7f);
            var controller = car.AddComponent<CarDriveController>();
            foreach (var pair in new[] { new {name="FL",pos=new Vector3(-.85f,-.35f,1.2f)},new{name="FR",pos=new Vector3(.85f,-.35f,1.2f)},new{name="RL",pos=new Vector3(-.85f,-.35f,-1.2f)},new{name="RR",pos=new Vector3(.85f,-.35f,-1.2f)} })
            {
                var w = new GameObject("Wheel_"+pair.name);
                w.transform.SetParent(car.transform,false);
                w.transform.localPosition=pair.pos;
                var wheel=w.AddComponent<WheelCollider>();
                wheel.radius=.34f;
                wheel.suspensionDistance=.25f;
                var field=typeof(CarDriveController).GetField(pair.name=="FL"?"frontLeft":pair.name=="FR"?"frontRight":pair.name=="RL"?"rearLeft":"rearRight",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                field.SetValue(controller,wheel);
            }
            var input=car.AddComponent<PrototypeCarInput>();
            typeof(PrototypeCarInput).GetField("car",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(input,controller);
            var camera=new GameObject("ChaseCamera");
            camera.tag="MainCamera";
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.transform.position=new Vector3(-7,5,-20);
            camera.AddComponent<VehicleFollowCamera>().SetTarget(car.transform);
            var sunlight=new GameObject("Sun").AddComponent<Light>();
            sunlight.type=LightType.Directional;
            sunlight.transform.rotation=Quaternion.Euler(40,-30,0);
            var track=new GameObject("FigureEightGuide").AddComponent<ProceduralFigureEight>();
            track.Generate();
            var canvas=new GameObject("TouchCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvas.GetComponent<CanvasScaler>().referenceResolution=new Vector2(1920,1080);
            var eventSystem=new GameObject("EventSystem",typeof(EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            CreateButton(canvas.transform,"Left",new Vector2(125,120),new Vector2(160,130),input,MobileCarTouchControl.Control.SteerLeft);
            CreateButton(canvas.transform,"Right",new Vector2(320,120),new Vector2(160,130),input,MobileCarTouchControl.Control.SteerRight);
            CreateButton(canvas.transform,"Brake",new Vector2(-380,120),new Vector2(160,130),input,MobileCarTouchControl.Control.Brake,true);
            CreateButton(canvas.transform,"Gas",new Vector2(-170,120),new Vector2(160,130),input,MobileCarTouchControl.Control.Throttle,true);
            EditorSceneManager.MarkSceneDirty(scene);
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/PrototypeCar.unity");
            Selection.activeGameObject=car;
            Debug.Log("Prototype scene generated. Open Assets/Scenes/PrototypeCar.unity. Test physics in Play Mode; not an official trial course.");
        }

        private static GameObject MakeCube(string name,Vector3 position,Vector3 scale,Color color)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name=name;
            go.transform.position=position;
            go.transform.localScale=scale;
            var renderer=go.GetComponent<Renderer>();
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color=color;
            renderer.sharedMaterial=material;
            return go;
        }

        private static void CreateButton(Transform parent,string text,Vector2 offset,Vector2 size,PrototypeCarInput input,MobileCarTouchControl.Control control,bool right=false)
        {
            var root=new GameObject(text,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            root.transform.SetParent(parent,false);
            var rect=root.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=right?new Vector2(1,0):new Vector2(0,0);
            rect.pivot=right?new Vector2(1,0):new Vector2(0,0);
            rect.anchoredPosition=offset;rect.sizeDelta=size;
            root.GetComponent<Image>().color=new Color(.1f,.15f,.2f,.75f);
            var controlScript=root.AddComponent<MobileCarTouchControl>();
            var so=new SerializedObject(controlScript);
            so.FindProperty("target").objectReferenceValue=input;
            so.FindProperty("control").enumValueIndex=(int)control;
            so.ApplyModifiedPropertiesWithoutUndo();
            var label=new GameObject("Label",typeof(RectTransform),typeof(Text));
            label.transform.SetParent(root.transform,false);
            var r=label.GetComponent<RectTransform>();
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            var t=label.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=text;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.fontSize=32;
        }
    }
}
#endif
