using UnityEngine;
using UnityEngine.UI;

namespace NepalTrial.Trial
{
    /// <summary>
    /// Explicit training checklist for the behavioral section of the connected
    /// course. This is not an automated examiner or real-world safety detector.
    /// </summary>
    public sealed class TrialBehaviorChecklist : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private bool twoWheeler;
        private bool checkedVehicle, checkedSafety, checkedControl;
        private GameObject panel;
        private Text instructions;
        private bool submitted;

        private void OnEnable()
        {
            if (trial != null) trial.AttemptRestarted += ResetChecklist;
        }

        private void OnDisable()
        {
            if (trial != null) trial.AttemptRestarted -= ResetChecklist;
        }

        private void Start()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Connected behavioral stage requires an existing UI Canvas.",this);
                enabled = false;
                return;
            }
            panel = new GameObject("BehavioralAssessmentPanel",typeof(RectTransform),typeof(Image));
            panel.transform.SetParent(canvas.transform,false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f);
            rect.sizeDelta = new Vector2(650,430);
            panel.GetComponent<Image>().color = new Color(.04f,.12f,.19f,.95f);

            instructions = AddLabel(panel.transform,175,"");
            AddButton(panel.transform,"Vehicle checks",80,CheckVehicle);
            AddButton(panel.transform,twoWheeler ? "Helmet strap locked" : "Seatbelt fastened",
                0,CheckSafety);
            AddButton(panel.transform,"Safe and controlled driving",-80,CheckControl);
            AddButton(panel.transform,"Submit assessment",-165,Submit);
            panel.SetActive(false);
        }

        private static Text AddLabel(Transform parent,float verticalOffset,string initial)
        {
            var go=new GameObject("Text",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
            go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.sizeDelta=new Vector2(600,65);
            rect.anchoredPosition=new Vector2(0,verticalOffset);
            var label=go.GetComponent<Text>();
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text=initial;
            label.color=Color.white;
            label.alignment=TextAnchor.MiddleCenter;
            label.fontSize=27;
            label.raycastTarget=false;
            return label;
        }

        private static void AddButton(Transform parent,string title,float verticalOffset,
            UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(CanvasRenderer),
                typeof(Image),typeof(Button));
            go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.sizeDelta=new Vector2(550,65);
            rect.anchoredPosition=new Vector2(0,verticalOffset);
            go.GetComponent<Image>().color=new Color(.10f,.32f,.45f,1f);
            go.GetComponent<Button>().onClick.AddListener(action);
            Text text=AddLabel(go.transform,0,title);
            var labelRect=text.GetComponent<RectTransform>();
            labelRect.anchorMin=Vector2.zero;
            labelRect.anchorMax=Vector2.one;
            labelRect.offsetMin=labelRect.offsetMax=Vector2.zero;
        }

        private void CheckVehicle() { checkedVehicle=true; UpdateLabel(); }
        private void CheckSafety() { checkedSafety=true; UpdateLabel(); }
        private void CheckControl() { checkedControl=true; UpdateLabel(); }

        private void UpdateLabel()
        {
            if (instructions == null) return;
            instructions.text = "Behavioral assessment (10 marks)\n" +
                (checkedVehicle ? "[OK] " : "[ ] ") + "Vehicle checks  " +
                (checkedSafety ? "[OK] " : "[ ] ") + (twoWheeler ? "Helmet  " : "Seatbelt  ") +
                (checkedControl ? "[OK]" : "[ ]") + " Control";
        }

        private void Submit()
        {
            if (submitted || trial == null || trial.State != ExamStage.Running ||
                trial.CurrentStage != "behavior" || trial.IsPaused) return;
            submitted=true;
            if (!checkedVehicle) trial.ReportViolation("behavior-check");
            if (!checkedSafety) trial.ReportViolation("behavior-safety");
            if (!checkedControl) trial.ReportViolation("behavior-control");
            if (trial.State == ExamStage.Running) trial.ReportStage("behavior");
        }

        private void Update()
        {
            if (panel == null || trial == null) return;
            bool visible = trial.State == ExamStage.Running && trial.CurrentStage == "behavior";
            if (panel.activeSelf != visible)
            {
                panel.SetActive(visible);
                if (visible) UpdateLabel();
            }
        }

        private void ResetChecklist()
        {
            checkedVehicle=checkedSafety=checkedControl=submitted=false;
            UpdateLabel();
        }
    }
}
