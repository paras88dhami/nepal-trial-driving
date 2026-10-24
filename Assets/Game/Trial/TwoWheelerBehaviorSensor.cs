using UnityEngine;
using UnityEngine.UI;

namespace NepalTrial.Trial
{
    /// <summary>Interactive training checklist, not a validated government examiner.</summary>
    public sealed class TwoWheelerBehaviorSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private Text checklistLabel;
        [SerializeField] private string stageId="behavior";
        private readonly TwoWheelerBehavioralValidator validator=new TwoWheelerBehavioralValidator();

        private void OnEnable(){ if(trial!=null)trial.AttemptRestarted+=ResetAttempt; }
        private void OnDisable(){ if(trial!=null)trial.AttemptRestarted-=ResetAttempt; }
        public void ResetAttempt(){ validator.Reset(); UpdateHud(); }
        public void CheckHelmet(){ validator.SetHelmet(true); UpdateHud(); }
        public void CheckMirrors(){ validator.SetMirrors(true); UpdateHud(); }
        public void CheckBrakes(){ validator.SetBrakes(true); UpdateHud(); }
        public void CheckTires(){ validator.SetTires(true); UpdateHud(); }
        public void Submit()
        {
            if(trial==null || trial.State!=ExamStage.Running || trial.IsPaused ||
                trial.CurrentStage!=stageId)return;
            CourseEvent evt=validator.TryComplete();
            if(evt.Kind==CourseEventKind.Completed) trial.ReportStage(stageId);
            else trial.ReportViolation(evt.Code);
            UpdateHud();
        }
        private void Start()=>UpdateHud();
        private void UpdateHud()
        {
            if(checklistLabel==null)return;
            checklistLabel.text="Safety checks\n"+
                "Helmet: "+(validator.HelmetFastened?"OK":"Missing")+"\n"+
                "Mirrors: "+(validator.MirrorsChecked?"OK":"Missing")+"\n"+
                "Brakes: "+(validator.BrakesChecked?"OK":"Missing")+"\n"+
                "Tires: "+(validator.TiresChecked?"OK":"Missing");
        }
    }
}
