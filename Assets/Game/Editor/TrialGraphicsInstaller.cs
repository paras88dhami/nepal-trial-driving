#if UNITY_EDITOR
using System;
using System.IO;
using NepalTrial.Input;
using NepalTrial.Mobile;
using NepalTrial.Vehicles.Car;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NepalTrial.EditorTools
{
    /// <summary>
    /// Optional art integration without rebuilding scenes or replacing sensors.
    /// The user owns/selects their downloaded asset; no account bypass or remote
    /// asset redistribution is performed by this tool.
    /// </summary>
    public static class TrialGraphicsInstaller
    {
        private const string CarScene="Assets/Scenes/NepalTrial_FullCar.unity";
        private static readonly string[] ExamScenes={
            CarScene,
            "Assets/Scenes/NepalTrial_FullMotorcycle.unity",
            "Assets/Scenes/NepalTrial_FullScooter.unity"
        };

        private static bool OpenCarScene()
        {
            if(!File.Exists(CarScene))
            {
                EditorUtility.DisplayDialog("Generate scenes first",
                    "Run Nepal Trial > Connected Exam > Generate All Three Connected Grounds.", "OK");
                return false;
            }
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return false;
            EditorSceneManager.OpenScene(CarScene,OpenSceneMode.Single);
            return true;
        }

        private static void Backup(string assetPath)
        {
            if(!File.Exists(assetPath))return;
            string dir=Path.Combine("Build","GraphicsSceneBackups",
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
            Directory.CreateDirectory(dir);
            File.Copy(assetPath,Path.Combine(dir,Path.GetFileName(assetPath)),true);
            Debug.Log("Graphics backup: "+dir);
        }

        [MenuItem("Nepal Trial/Graphics/Attach Selected Car Model to Full Car")]
        public static void AttachCar()
        {
            var model=Selection.activeObject as GameObject;
            string modelPath=model==null?"":AssetDatabase.GetAssetPath(model);
            if(model==null || string.IsNullOrEmpty(modelPath) ||
                !PrefabUtility.IsPartOfPrefabAsset(model))
            {
                EditorUtility.DisplayDialog("Select a car asset",
                    "Select an imported FBX or car prefab in the Project window first, then rerun this action.",
                    "OK");
                return;
            }
            if(!EditorUtility.DisplayDialog("Install car graphics",
                "Add "+model.name+" to the connected car scene? The existing scene is backed up first. "+
                "Verify scaling, forward direction and wheel meshes after installation.",
                "Install","Cancel"))return;
            if(!OpenCarScene())return;
            var car=UnityEngine.Object.FindFirstObjectByType<CarDriveController>();
            if(car==null){Debug.LogError("No TrialCar found.");return;}
            if(car.transform.Find("CarGraphics")!=null)
            {
                EditorUtility.DisplayDialog("Graphics already present",
                    "A CarGraphics child exists. Rename or remove it manually before installing another car.", "OK");
                return;
            }
            Backup(CarScene);
            var graphics=new GameObject("CarGraphics");
            graphics.transform.SetParent(car.transform,false);
            var instance=PrefabUtility.InstantiatePrefab(model) as GameObject;
            if(instance==null){UnityEngine.Object.DestroyImmediate(graphics);return;}
            instance.name=model.name;
            instance.transform.SetParent(graphics.transform,false);

            // Downloaded prefabs may contain their own physics. The root TrialCar
            // controller remains the only source of physics/collision in the exam.
            foreach(var rb in instance.GetComponentsInChildren<Rigidbody>(true))
                UnityEngine.Object.DestroyImmediate(rb);
            foreach(var collider in instance.GetComponentsInChildren<Collider>(true))
                collider.enabled=false;
            foreach(var old in car.GetComponentsInChildren<Transform>(true))
            {
                if(old==car.transform || old.IsChildOf(graphics.transform))continue;
                if(old.name=="CarVisual" || old.name.StartsWith("WheelVisual_"))
                    old.gameObject.SetActive(false);
            }
            var wheelSync=car.GetComponent<CarWheelVisualSync>();
            if(wheelSync==null)wheelSync=car.gameObject.AddComponent<CarWheelVisualSync>();
            var config=new SerializedObject(wheelSync);
            string[] names={"frontLeft","frontRight","rearLeft","rearRight"};
            foreach(string name in names)
            {
                var wheel=car.transform.Find("Wheel_"+name);
                var prop=config.FindProperty(name+"Collider");
                if(wheel!=null && prop!=null)
                    prop.objectReferenceValue=wheel.GetComponent<WheelCollider>();
            }
            config.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Selection.activeGameObject=graphics;
            Debug.Log("Car model attached: "+modelPath+
                ". IMPORTANT: Adjust model position/scale/rotation and connect four visible wheel meshes in CarWheelVisualSync Inspector.");
        }

        [MenuItem("Nepal Trial/Graphics/Apply Selected Road Texture to Full Grounds")]
        public static void ApplyRoad()
        {
            var tex=Selection.activeObject as Texture2D;
            if(tex==null)
            {
                EditorUtility.DisplayDialog("Select an asphalt image",
                    "Select the imported asphalt_01_diff_2k JPG texture in Unity's Project window.", "OK");
                return;
            }
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            string matDir="Assets/Game/Art/Environment/Roads/Materials";
            Directory.CreateDirectory(matDir);
            string matPath=matDir+"/Trial_Asphalt_URP.mat";
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null){Debug.LogError("URP/Lit shader not found.");return;}
            var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(material==null)
            {
                material=new Material(shader);
                AssetDatabase.CreateAsset(material,matPath);
            }
            material.SetTexture("_BaseMap",tex);
            material.SetTextureScale("_BaseMap",new Vector2(5,8));
            material.SetFloat("_Smoothness",.12f);
            EditorUtility.SetDirty(material);
            int updated=0;
            foreach(string scenePath in ExamScenes)
            {
                if(!File.Exists(scenePath))
                {Debug.LogWarning("Missing scene (generate first): "+scenePath);continue;}
                Backup(scenePath);
                var scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
                foreach(var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(
                    FindObjectsSortMode.None))
                {
                    string n=renderer.name;
                    bool road=n=="ConnectedExam_Pavement" ||
                        n=="LBackEntrance" || n=="LBackCrossLeg" ||
                        n=="LBackParkingBay" || n=="UTurnLane" ||
                        n=="Car_DrivableDownhillRamp" || n=="DrivableRamp" ||
                        n=="TopPlatform" || n=="AscendingRamp" ||
                        n=="RampCrest" || n=="DescendingRamp";
                    if(!road)continue;
                    renderer.sharedMaterial=material;
                    updated++;
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Applied Poly Haven asphalt to "+updated+
                " road renderers. Generated scene rebuilds may overwrite these references.");
        }

        [MenuItem("Nepal Trial/Graphics/Add Analog Steering Wheel to Full Car")]
        public static void AddSteering()
        {
            if(!OpenCarScene())return;
            var input=UnityEngine.Object.FindFirstObjectByType<PrototypeCarInput>();
            var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if(input==null || canvas==null){Debug.LogError("Car input or Canvas missing.");return;}
            if(canvas.transform.Find("AnalogSteeringTouchArea")!=null)
            {Debug.Log("Analog steering wheel already installed.");return;}
            Backup(CarScene);
            Sprite wheel=BuildWheelSprite();
            var area=new GameObject("AnalogSteeringTouchArea",typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image),typeof(MobileAnalogSteeringWheel));
            area.transform.SetParent(canvas.transform,false);
            RectTransform rect=area.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=Vector2.zero;
            rect.pivot=Vector2.zero;
            rect.anchoredPosition=new Vector2(55,65);
            rect.sizeDelta=new Vector2(280,280);
            var touchImage=area.GetComponent<Image>();
            touchImage.color=new Color(0,0,0,.001f);
            touchImage.raycastTarget=true;
            var wheelObj=new GameObject("WheelGraphic",typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            wheelObj.transform.SetParent(area.transform,false);
            var wheelRect=wheelObj.GetComponent<RectTransform>();
            wheelRect.anchorMin=Vector2.zero;wheelRect.anchorMax=Vector2.one;
            wheelRect.offsetMin=wheelRect.offsetMax=Vector2.zero;
            var img=wheelObj.GetComponent<Image>();
            img.sprite=wheel;
            img.preserveAspect=true;
            img.raycastTarget=false;
            area.GetComponent<MobileAnalogSteeringWheel>().Bind(input,wheelRect);
            foreach(Transform t in canvas.GetComponentsInChildren<Transform>(true))
            {
                if(t.name=="Left" || t.name=="Right")
                    t.gameObject.SetActive(false);
            }
            var scene=SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Analog touch steering installed. Drag the left wheel to steer; "+
                "keyboard A/D remains supported.");
        }

        private static Sprite BuildWheelSprite()
        {
            string dir="Assets/Game/Art/UI/DrivingControls";
            Directory.CreateDirectory(dir);
            string path=dir+"/SteeringWheel.png";
            if(!File.Exists(path))
            {
                const int size=256;
                var pixels=new Color32[size*size];
                Color32 clear=new Color32(0,0,0,0);
                Color32 rim=new Color32(211,221,228,255);
                Color32 dark=new Color32(28,37,46,255);
                for(int y=0;y<size;y++)
                for(int x=0;x<size;x++)
                {
                    float px=(x-size*.5f)/(size*.5f);
                    float py=(y-size*.5f)/(size*.5f);
                    float distance=Mathf.Sqrt(px*px+py*py);
                    bool ring=distance>.67f && distance<.85f;
                    bool center=distance<.19f;
                    float angle=Mathf.Atan2(py,px);
                    bool spoke=distance<.72f && distance>.17f &&
                        (Mathf.Abs(Mathf.DeltaAngle(angle*Mathf.Rad2Deg,90))<9 ||
                         Mathf.Abs(Mathf.DeltaAngle(angle*Mathf.Rad2Deg,210))<9 ||
                         Mathf.Abs(Mathf.DeltaAngle(angle*Mathf.Rad2Deg,330))<9);
                    pixels[y*size+x]=ring?rim:(center||spoke?dark:clear);
                }
                var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);
                tex.SetPixels32(pixels);
                tex.Apply();
                File.WriteAllBytes(path,tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                importer.textureType=TextureImporterType.Sprite;
                importer.alphaIsTransparency=true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
#endif
