using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    public sealed class TwoWheelerUTurnSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private BoxCollider ridingRegion;
        [SerializeField] private bool useHorseshoeBoundary;
        [SerializeField] private Transform curveCenter;
        [SerializeField, Min(1f)] private float innerTurnRadius=2.8f;
        [SerializeField, Min(1f)] private float outerTurnRadius=5.2f;
        [SerializeField] private float approachStartZ=-11f;
        [SerializeField] private string stageId = "u-turn";
        [SerializeField, Min(1f)] private float minTurnDegrees = 140f;
        [SerializeField, Min(1f)] private float maxSeconds = 35f;
        private UTurnValidator validator;
        private bool running, exitRequested, usedLeft, usedRight;
        private float netHeadingChange;
        private float lastHeading;
        public bool IsRunning => running;

        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }
        public void ResetAttempt() { running=false; exitRequested=false; usedLeft=usedRight=false;
            netHeadingChange=0f; validator=null; }

        public void Begin()
        {
            if (running || trial == null || vehicle == null ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId) return;
            validator=new UTurnValidator(minTurnDegrees,maxSeconds);
            validator.Begin();
            running=true;
            lastHeading=vehicle.transform.eulerAngles.y;
            usedLeft=usedRight=false;
            netHeadingChange=0f;
            exitRequested=false;
        }
        public void Exit() => exitRequested=true;

        private void FixedUpdate()
        {
            if (!running || trial == null || vehicle == null ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId) return;
            usedLeft |= vehicle.LeftIndicator;
            usedRight |= vehicle.RightIndicator;
            float heading=vehicle.transform.eulerAngles.y;
            float turn=Mathf.DeltaAngle(lastHeading,heading);
            lastHeading=heading;
            netHeadingChange+=turn;
            bool outside=false;
            if(useHorseshoeBoundary && curveCenter!=null)
            {
                Vector3 p=vehicle.transform.position;
                outside=!UTurnHorseshoeMath.IsInside(
                    new Vector2(p.x,p.z),
                    new Vector2(curveCenter.position.x,curveCenter.position.z),
                    innerTurnRadius,outerTurnRadius,
                    curveCenter.position.z+approachStartZ,.08f);
            }
            else if (ridingRegion != null)
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
                if (!TwoWheelerIndicatorRules.HasCorrectTurnSignal(netHeadingChange,usedLeft,usedRight))
                    trial.ReportViolation("u-turn-no-indicator");
                if(trial.State==ExamStage.Running)trial.ReportStage(stageId);
            }
            else trial.ReportViolation(result.Code);
        }
    }
}
