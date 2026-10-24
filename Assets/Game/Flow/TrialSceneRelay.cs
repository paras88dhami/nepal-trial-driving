using NepalTrial.Trial;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NepalTrial.Flow
{
    /// <summary>
    /// Automatically connects the already-generated Phase 2/3 scenes to the persistent
    /// Phase 4 session, including Next Stage / Home buttons. Does not edit scenes.
    /// </summary>
    public sealed class TrialSceneRelay : MonoBehaviour
    {
        private TrialGameplayRuntime trial;
        private TrialPlaySession active;
        private Button nextButton;
        private Text statusLabel;
        private string error;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Attach()
        {
            if (FindFirstObjectByType<TrialSceneRelay>() != null) return;
            if (FindFirstObjectByType<TrialGameplayRuntime>() == null) return;
            new GameObject("Phase4TrialSessionRelay").AddComponent<TrialSceneRelay>();
        }

        private void Awake()
        {
            trial = FindFirstObjectByType<TrialGameplayRuntime>();
            active = TrialSessionService.Current;
            if (trial == null) { enabled=false; return; }
            trial.Finished += OnFinished;
            trial.AttemptRestarted += OnRestart;
        }

        private void Start() => BuildControls();

        private void OnDestroy()
        {
            if (trial == null) return;
            trial.Finished -= OnFinished;
            trial.AttemptRestarted -= OnRestart;
        }

        private void OnFinished(ExamStage state)
        {
            if (state != ExamStage.Passed && state != ExamStage.Failed) return;
            if (!TrialSessionService.IsSessionScene(SceneManager.GetActiveScene().name)) return;
            TrialSessionService.ReportStageResult(state==ExamStage.Passed,trial.Score);
            active = TrialSessionService.Current;
            UpdateStatus();
        }

        private void OnRestart()
        {
            if (TrialSessionService.IsSessionScene(SceneManager.GetActiveScene().name))
                TrialSessionService.RetryStage();
            active = TrialSessionService.Current;
            error=null;
            UpdateStatus();
        }

        private void Update()
        {
            if (nextButton != null)
            {
                bool available = active != null && active.mode == DriveMode.Exam && active.AwaitingNext;
                if (nextButton.gameObject.activeSelf != available)
                    nextButton.gameObject.SetActive(available);
            }
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (statusLabel == null) return;
            if (!string.IsNullOrEmpty(error)) { statusLabel.text=error; return; }
            if (active == null || !TrialSessionService.IsSessionScene(SceneManager.GetActiveScene().name))
            {
                statusLabel.text="Standalone trial | test only";
                return;
            }
            if (active.mode==DriveMode.FreeDrive)
            {
                statusLabel.text="FREE DRIVE | scoring disabled";
                return;
            }
            if (active.ended)
            {
                statusLabel.text=active.mode+" | "+(active.passed?"PASSED":"FAILED")+
                    " | Total score "+active.finalScore;
                return;
            }
            statusLabel.text=active.mode+" | "+
                (active.stageIndex+1)+"/"+active.courses.Length+" | "+
                (active.AwaitingNext?"Stage passed - tap Next":"Drive the course");
        }

        private void Next()
        {
            if (!TrialSessionService.Advance(out error)) UpdateStatus();
        }

        private static Text Label(Transform parent,string text,int size)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
            go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            var label=go.GetComponent<Text>();
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize=size;
            label.text=text;
            label.color=Color.white;
            label.alignment=TextAnchor.MiddleCenter;
            label.raycastTarget=false;
            return label;
        }

        private static Button MakeButton(Transform parent,string text,Vector2 offset)
        {
            var go=new GameObject(text,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Button));
            go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(1,1);
            rect.pivot=new Vector2(1,1);
            rect.sizeDelta=new Vector2(180,65);
            rect.anchoredPosition=offset;
            go.GetComponent<Image>().color=new Color(.06f,.18f,.28f,.95f);
            Label(go.transform,text,26);
            return go.GetComponent<Button>();
        }

        private void BuildControls()
        {
            // Attach to scene's existing touch canvas so event-system setup stays in one place.
            Canvas canvas=FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogError("Trial scene has no Canvas for navigation."); return; }
            var home=MakeButton(canvas.transform,"Home",new Vector2(-165,-370));
            home.onClick.AddListener(TrialSessionService.ReturnMenu);
            nextButton=MakeButton(canvas.transform,"Next Stage",new Vector2(-365,-370));
            nextButton.onClick.AddListener(Next);
            var indicator=new GameObject("SessionStatus",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
            indicator.transform.SetParent(canvas.transform,false);
            var rect=indicator.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,1f);
            rect.sizeDelta=new Vector2(860,65);
            rect.anchoredPosition=new Vector2(0,-385);
            statusLabel=indicator.GetComponent<Text>();
            statusLabel.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusLabel.fontSize=26;
            statusLabel.alignment=TextAnchor.MiddleCenter;
            statusLabel.color=Color.white;
            statusLabel.raycastTarget=false;
            UpdateStatus();
        }
    }
}
