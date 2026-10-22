#if UNITY_EDITOR
using NepalTrial.CameraSystem;
using NepalTrial.Input;
using NepalTrial.Trial;
using NepalTrial.Vehicles.Car;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NepalTrial.EditorTools
{
    /// <summary>Generates a functional placeholder scene; not an official trial layout.</summary>
    public static class TrialPrototypeBuilder
    {
        [MenuItem("Nepal Trial/Build Car Practice Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Practice Asphalt (placeholder)";
            floor.transform.position = new Vector3(0f, -0.25f, 0f);
            floor.transform.localScale = new Vector3(90f, 0.5f, 90f);

            var car = new GameObject("Prototype Car");
            car.transform.position = new Vector3(0f, 0.9f, -17f);
            car.transform.rotation = Quaternion.identity;
            var rb = car.AddComponent<Rigidbody>();
            rb.mass = 1050f;
            rb.centerOfMass = new Vector3(0f, -0.25f, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body Placeholder";
            body.transform.SetParent(car.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.8f, 0.7f, 3.8f);

            var drive = car.AddComponent<CarDriveController>();
            var wheels = new WheelCollider[4];
            for (int i = 0; i < 4; i++)
            {
                var wheel = new GameObject(new[] {"Front Left Wheel","Front Right Wheel","Rear Left Wheel","Rear Right Wheel"}[i]);
                wheel.transform.SetParent(car.transform, false);
                wheel.transform.localPosition = new Vector3(i % 2 == 0 ? -0.77f : 0.77f, -0.34f, i < 2 ? 1.15f : -1.15f);
                var wc = wheel.AddComponent<WheelCollider>();
                wc.radius = 0.35f;
                wc.suspensionDistance = 0.2f;
                wc.mass = 20f;
                wheels[i] = wc;
            }

            var driveSO = new SerializedObject(drive);
            driveSO.FindProperty("frontLeft").objectReferenceValue = wheels[0];
            driveSO.FindProperty("frontRight").objectReferenceValue = wheels[1];
            driveSO.FindProperty("rearLeft").objectReferenceValue = wheels[2];
            driveSO.FindProperty("rearRight").objectReferenceValue = wheels[3];
            driveSO.ApplyModifiedPropertiesWithoutUndo();

            var controls = car.AddComponent<PrototypeCarInput>();
            var controlsSO = new SerializedObject(controls);
            controlsSO.FindProperty("car").objectReferenceValue = drive;
            controlsSO.ApplyModifiedPropertiesWithoutUndo();

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0, 5, -24);
            var follow = cameraObject.AddComponent<VehicleFollowCamera>();
            follow.SetTarget(car.transform);

            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(45f, -35f, 0);

            var track = new GameObject("Approximate Figure-8 Guide ONLY");
            var eight = track.AddComponent<ProceduralFigureEight>();
            eight.Generate();

            const string scenePath = "Assets/Scenes/CarPractice.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Assets/Scenes/CarPractice.unity. Prototype only; geometry is NOT validated against Nepal official trial rules.");
        }
    }
}
#endif
