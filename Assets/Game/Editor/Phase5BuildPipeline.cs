#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using NepalTrial.Flow;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NepalTrial.EditorTools
{
    /// <summary>
    /// Strict Unity editor release checks. These MUST be run in a real Unity Editor.
    /// Static CI alone does not prove the generated scenes can be played or built.
    /// </summary>
    public static class Phase5BuildPipeline
    {
        [MenuItem("Nepal Trial/Phase 5/Validate Build Prerequisites")]
        public static void ValidatePrerequisites()
        {
            var scenes=EditorBuildSettings.scenes;
            string menu="Assets/Scenes/"+TrialCatalog.MenuScene+".unity";
            if(scenes==null || scenes.Length==0 || scenes[0].path!=menu || !scenes[0].enabled)
                throw new InvalidOperationException("Main Menu must be enabled at Build Settings scene index zero.");

            foreach(string vehicle in TrialCatalog.Vehicles)
            foreach(string course in TrialCatalog.CoursesFor(vehicle))
            {
                string expected="Assets/Scenes/"+TrialCatalog.SceneFor(vehicle,course)+".unity";
                if (!File.Exists(expected))
                    throw new FileNotFoundException("Generate the missing trial scene with the Phase 4 menu: "+expected);
                if (!scenes.Any(scene => scene.path==expected && scene.enabled))
                    throw new InvalidOperationException("Build Settings scene missing or disabled: "+expected);
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(expected)))
                    throw new InvalidOperationException("Unity has not imported scene: "+expected);
            }
            if (!File.Exists(menu) || string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(menu)))
                throw new InvalidOperationException("Missing/unimported main menu scene: "+menu);
            if(EditorApplication.isCompiling || EditorApplication.isUpdating)
                throw new InvalidOperationException("Unity is importing or compiling assets. Wait and retry.");

            Debug.Log("Phase 5: 21 required scenes present, imported and registered. " +
                "This does NOT prove game physics or Android build success.");
        }

        [MenuItem("Nepal Trial/Phase 5/Build Development Android APK")]
        public static void BuildDevelopmentAndroid()
        {
            ValidatePrerequisites();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new InvalidOperationException(
                    "Activate Android in Unity Build Profiles or launch batchmode with -buildTarget Android.");
            string folder="Build/Android";
            Directory.CreateDirectory(folder);
            string output=Path.GetFullPath(Path.Combine(folder,"NepalDrivingTrial-dev.apk"));
            var options=new BuildPlayerOptions
            {
                scenes=EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>x.path).ToArray(),
                locationPathName=output,
                target=BuildTarget.Android,
                options=BuildOptions.Development | BuildOptions.AllowDebugging
            };
            var report=BuildPipeline.BuildPlayer(options);
            if (report==null || report.summary.result!=BuildResult.Succeeded)
                throw new InvalidOperationException("Android build failed. Inspect the Unity Editor log.");
            Debug.Log("Phase 5: Development Android APK built at "+output+
                ". Real Android device testing is still required.");
        }
    }
}
#endif
