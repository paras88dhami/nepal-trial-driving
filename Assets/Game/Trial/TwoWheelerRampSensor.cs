using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Ramp checkpoint requires stationary stop in zone before crossing exit.</summary>
    public sealed class TwoWheelerRampSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private Transform uphillDirection;
        [SerializeField] private BoxCollider stopZone;
        [SerializeField] private Transform descentStart;
        [SerializeField] private BoxCollider downhillStopZone;
        private float downhillHold;
        private bool downhillStopped;
        [SerializeField] private string stageId = "ramp";
        [SerializeField, Min(0f)] private float maxRollbackMeters=.45f;
        [SerializeField, Min(.1f)] private float holdSeconds=.8f;
        [SerializeField, Min(1f)] private float timeoutSeconds=45f;
        private TwoWheelerRampValidator validator;
        private bool running,exitRequested;
        private bool reachedDescent;
        public bool IsRunning => running;

        private float Position()
        {
            Vector3 axis=uphillDirection==null?transform.forward:uphillDirection.forward;
            return Vector3.Dot(vehicle.transform.position,axis.normalized);
        }
        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }
        public void ResetAttempt() { running=false; exitRequested=false; reachedDescent=false; validator=null;
            downhillStopped=false;downhillHold=0f; }
        public void Begin()
        {
            if (running || trial == null || vehicle == null ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId) return;
            validator=new TwoWheelerRampValidator(maxRollbackMeters,holdSeconds,timeoutSeconds);
            validator.Begin(Position());
            running=true; exitRequested=false; reachedDescent=false;
            downhillStopped=false;downhillHold=0f;
        }
        public void Exit() => exitRequested=true;
        private void FixedUpdate()
        {
            if (!running || vehicle == null || trial == null ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId) return;
            bool withinStop=false;
            if (stopZone != null)
            {
                Vector3 p=stopZone.transform.InverseTransformPoint(vehicle.transform.position)-stopZone.center;
                Vector3 h=stopZone.size*.5f;
                withinStop=Mathf.Abs(p.x)<=h.x && Mathf.Abs(p.z)<=h.z;
            }
            if (descentStart != null)
            {
                Vector3 axis=uphillDirection==null?transform.forward:uphillDirection.forward;
                reachedDescent |= Position() >= Vector3.Dot(descentStart.position,axis.normalized);
            }
            if(downhillStopZone!=null && reachedDescent && !downhillStopped)
            {
                Vector3 p=downhillStopZone.transform.InverseTransformPoint(
                    vehicle.transform.position)-downhillStopZone.center;
                Vector3 h=downhillStopZone.size*.5f;
                bool inside=Mathf.Abs(p.x)<=h.x && Mathf.Abs(p.z)<=h.z;
                downhillHold=inside && vehicle.SpeedKph < .72f
                    ? downhillHold+Time.fixedDeltaTime:0f;
                downhillStopped=downhillHold>=holdSeconds;
            }
            bool exitNow=exitRequested;
            CourseEvent result=validator.Sample(Position(),withinStop,vehicle.SpeedKph/3.6f,
                vehicle.FootDown,exitNow,Time.fixedDeltaTime,reachedDescent);
            if(exitNow && downhillStopZone!=null && !downhillStopped)
                result=new CourseEvent(CourseEventKind.Violation,"ramp-no-stop");
            exitRequested=false;
            if (result.Kind == CourseEventKind.None) return;
            running=false;
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
            else trial.ReportViolation(result.Code);
        }
    }
}
