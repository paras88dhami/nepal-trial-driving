using System;
using NepalTrial.Progress;
using NepalTrial.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NepalTrial.Flow
{
    /// <summary>
    /// Minimal playable menu/garage UI. Generated entirely from standard Unity UI components,
    /// avoiding fragile prefab references while the art direction is being developed.
    /// </summary>
    public sealed class TrialMainMenuController : MonoBehaviour
    {
        private GameFlowState flow;
        private LocalProgressData progress;
        private LocalGameSettings settings;
        private Canvas canvas;
        private GameObject content;
        private Text errorLabel;
        private bool settingsOpen;
        private string error;

        private void Start()
        {
            progress = TrialProgressBootstrap.LoadReady();
            settings = new LocalSettingsStore().Load();
            ApplySettings();
            flow = new GameFlowState();
            flow.SetAvailableVehicles(progress.unlockedVehicleIds);
            Build();
        }

        private void ApplySettings()
        {
            GameSettingsRules.Normalize(settings);
            AudioListener.volume = settings.soundVolume;
            int count = QualitySettings.names.Length;
            if (count > 0)
                QualitySettings.SetQualityLevel(Mathf.Clamp(settings.graphicsQuality,0,count-1),true);
        }

        private void SaveSettings()
        {
            new LocalSettingsStore().Save(settings);
            ApplySettings();
            Build();
        }

        private void EnsureCanvas()
        {
            if (canvas != null) return;
            var root = new GameObject("MenuCanvas",typeof(RectTransform),typeof(Canvas),
                typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080);
            if (EventSystem.current == null)
                new GameObject("EventSystem",typeof(EventSystem),
                    typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        private void Build()
        {
            EnsureCanvas();
            if (content != null) Destroy(content);
            content = new GameObject("MenuContent",typeof(RectTransform),
                typeof(Image),typeof(VerticalLayoutGroup));
            content.transform.SetParent(canvas.transform,false);
            var rect = content.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f);
            rect.sizeDelta = new Vector2(810,940);
            content.GetComponent<Image>().color = new Color(.045f,.085f,.13f,.93f);
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(45,45,22,22);
            layout.spacing = 9f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            if (settingsOpen) BuildSettings();
            else switch (flow.Screen)
            {
                case GameScreen.Home: BuildHome(); break;
                case GameScreen.VehicleSelect: BuildGarage(); break;
                case GameScreen.ModeSelect: BuildModes(); break;
                case GameScreen.CourseSelect: BuildCourses(); break;
                default: flow.ReturnHome(); BuildHome(); break;
            }
            errorLabel = AddText(string.IsNullOrEmpty(error) ? "" : error,22,new Color(1f,.58f,.36f),60);
        }

        private void BuildHome()
        {
            AddText("NEPAL DRIVING TRIAL 3D",39,Color.white,66);
            AddText("Offline car, motorcycle and scooter practice",22,Color.white,44);
            Button("Start driving", () =>
            {
                flow.SetAvailableVehicles(progress.unlockedVehicleIds);
                if (flow.OpenVehicleSelect()) { error=null;Build(); }
            });
            var previous = TrialSessionService.Current;
            if (previous != null && !previous.ended)
                Button("Resume " + TrialCatalog.DisplayVehicle(previous.vehicleId) +
                    " (" + (previous.stageIndex+1) + "/" + previous.courses.Length + ")",() =>
                {
                    if (!TrialSessionService.Resume(out error)) Build();
                });
            if (previous != null && previous.ended)
                AddText("Last run: " + (previous.passed ? "Passed" : "Failed") +
                    " | Score " + previous.finalScore,24,Color.white,42);
            int attempts = 0, completions = 0;
            if (progress.records != null)
                foreach (var record in progress.records)
                {
                    if (record == null) continue;
                    attempts += record.attempts;
                    completions += record.completions;
                }
            AddText("Saved attempts: " + attempts + " | passes: " + completions,23,Color.white,45);
            Button("Settings",() => {settingsOpen=true;error=null;Build();});
            Button("Reset unfinished session",() =>
            {
                TrialSessionService.Abandon();
                error="Unfinished session cleared. Saved trial records are preserved.";
                Build();
            });
        }

        private void BuildGarage()
        {
            AddText("CHOOSE YOUR VEHICLE",35,Color.white,75);
            AddText("Starter vehicles are unlocked. Art is placeholder.",23,Color.white,54);
            foreach (string vehicle in TrialCatalog.Vehicles)
            {
                string id=vehicle;
                bool unlocked=progress.unlockedVehicleIds != null &&
                    progress.unlockedVehicleIds.Contains(id);
                Button(TrialCatalog.DisplayVehicle(id)+(unlocked?"":" (Locked)"),() =>
                {
                    if (!unlocked || !flow.SelectVehicle(id))
                    {
                        error="Vehicle locked or unavailable.";
                        Build(); return;
                    }
                    progress.selectedVehicleId=id;
                    new LocalProgressStore().Save(progress);
                    error=null;
                    Build();
                },unlocked);
            }
            Button("Back",BackToHome);
        }

        private void BuildModes()
        {
            AddText(TrialCatalog.DisplayVehicle(flow.VehicleId),35,Color.white,74);
            AddText("SELECT GAME MODE",28,Color.white,52);
            Button("Practice (single trial)",()=>ChooseMode(DriveMode.Practice));
            Button("Exam (all courses in order)",()=>ChooseMode(DriveMode.Exam));
            Button("Free Drive (no score)",()=>ChooseMode(DriveMode.FreeDrive));
            Button("Back",BackToHome);
        }

        private void ChooseMode(DriveMode mode)
        {
            if (!flow.SelectMode(mode)) { error="Cannot select mode.";Build();return; }
            error=null;
            if (mode==DriveMode.Exam)
            {
                string first=TrialCatalog.CoursesFor(flow.VehicleId)[0];
                if (!flow.StartCourse(first)) {error="Cannot begin exam.";BackToHome();return;}
                if (!TrialSessionService.StartNew(flow.VehicleId,mode,null,out error))
                    BackToHomePreservingError();
                return;
            }
            Build();
        }

        private void BuildCourses()
        {
            AddText("CHOOSE A COURSE",35,Color.white,62);
            AddText(TrialCatalog.DisplayVehicle(flow.VehicleId)+" | "+flow.Mode,22,Color.white,40);
            foreach (string course in TrialCatalog.CoursesFor(flow.VehicleId))
            {
                string selected=course;
                Button(selected,()=>SelectCourse(selected));
            }
            Button("Back",BackToHome);
        }

        private void SelectCourse(string course)
        {
            if (!flow.StartCourse(course)) {error="Cannot select course.";Build();return;}
            if (!TrialSessionService.StartNew(flow.VehicleId,flow.Mode,course,out error))
                BackToHomePreservingError();
        }

        private void BuildSettings()
        {
            AddText("GAME SETTINGS",36,Color.white,72);
            AddText("Steering sensitivity: "+settings.steeringSensitivity.ToString("0.00"),25,Color.white,50);
            Button("Steering -",()=>{settings.steeringSensitivity-=.25f;SaveSettings();});
            Button("Steering +",()=>{settings.steeringSensitivity+=.25f;SaveSettings();});
            AddText("Sound volume: "+Mathf.RoundToInt(settings.soundVolume*100)+"%",25,Color.white,45);
            Button("Volume -",()=>{settings.soundVolume-=.1f;SaveSettings();});
            Button("Volume +",()=>{settings.soundVolume+=.1f;SaveSettings();});
            Button("Graphics: "+settings.graphicsQuality+" (tap to change)",()=>
            {
                settings.graphicsQuality=(settings.graphicsQuality+1)%3;SaveSettings();
            });
            Button("Vibration: "+(settings.vibrationEnabled?"On":"Off"),()=>
            {
                settings.vibrationEnabled=!settings.vibrationEnabled;SaveSettings();
            });
            Button("Back",()=>{settingsOpen=false;error=null;Build();});
        }

        private void BackToHome() {error=null;BackToHomePreservingError();}
        private void BackToHomePreservingError()
        {
            settingsOpen=false;
            flow.ReturnHome();
            Build();
        }

        private Text AddText(string value,int size,Color color,float height)
        {
            var root=new GameObject("Text",typeof(RectTransform),typeof(CanvasRenderer),
                typeof(Text),typeof(LayoutElement));
            root.transform.SetParent(content.transform,false);
            root.GetComponent<LayoutElement>().preferredHeight=height;
            var label=root.GetComponent<Text>();
            label.text=value;
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize=size;
            label.color=color;
            label.alignment=TextAnchor.MiddleCenter;
            label.raycastTarget=false;
            return label;
        }

        private void Button(string value,UnityEngine.Events.UnityAction click,bool enabled=true)
        {
            var root=new GameObject("Button_"+value,typeof(RectTransform),typeof(CanvasRenderer),
                typeof(Image),typeof(UnityEngine.UI.Button),typeof(LayoutElement));
            root.transform.SetParent(content.transform,false);
            root.GetComponent<LayoutElement>().preferredHeight=66;
            root.GetComponent<Image>().color=enabled?
                new Color(.095f,.27f,.38f,.98f):new Color(.19f,.19f,.19f,1f);
            var button=root.GetComponent<UnityEngine.UI.Button>();
            button.interactable=enabled;
            button.onClick.AddListener(click);
            var labelRoot=new GameObject("Label",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
            labelRoot.transform.SetParent(root.transform,false);
            var rect=labelRoot.GetComponent<RectTransform>();
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            var label=labelRoot.GetComponent<Text>();
            label.text=value;
            label.fontSize=26;
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.alignment=TextAnchor.MiddleCenter;
            label.color=enabled?Color.white:Color.gray;
            label.raycastTarget=false;
        }
    }
}
