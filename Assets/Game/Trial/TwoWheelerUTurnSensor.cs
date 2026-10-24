using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    public sealed class TwoWheelerUTurnSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private BoxCollider ridingRegion;
        [SerializeField] private string stageId = "u-turn";
        [SerializeField, Min(1f)] private float minTurnDegrees = 140f;
        [SerializeField, Min(1f)] private float maxSeconds = 35f;
        private UTurnValidator validator;
        private bool running, exitRequested, usedIndicator;
        private float lastHeading;
        public bool IsRunning => running;

        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }
        public void ResetAttempt() { running=false; exitRequested=false; usedIndicator=false; validator=null; }

        public void Begin()
        {
            if (running || trial == null || vehicle == null ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId) return;
            validator=new UTurnValidator(minTurnDegrees,maxSeconds);
            validator.Begin();
            running=true;
            lastHeading=vehicle.transform.eulerAngles.y;
            usedIndicator=false;
            exitRequested=false;
        }
        public void Exit() => exitRequested=true;

        private void FixedUpdate()
        {
            if (!running || trial == null || vehicle == null ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId) return;
            usedIndicator |= vehicle.LeftIndicator || vehicle.RightIndicator;
            float heading=vehicle.transform.eulerAngles.y;
            float turn=Mathf.DeltaAngle(lastHeading,heading);
            lastHeading=heading;
            bool outside=false;
            if (ridingRegion != null)
            {
                Vector3 p=ridingRegion.transform.InverseTransformPoint(vehicle.transform.position)-ridingRegion.center;
                Vector3 half=ridingRegion.size*.5f;
                outside=Mathf.Abs(p.x)>half.x || Mathf.Abs(p.z)>half.z;
            }
            CourseEvent result=validator.Sample(turn,Time.fixedDeltaTime,outside,exitRequested);
            exitRequested=false;
            if (result.Kind == CourseEventKind.None) return;
            running=false;
            if (result.Kind == CourseEventKind.Completed)
            {
                if (!usedIndicator) trial.ReportViolation("u-turn-no-indicator");
                trial.ReportStage(stageId);
            }
            else trial.ReportViolation(result.Code);
        }
    }
}
