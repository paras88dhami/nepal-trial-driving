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
        [SerializeField] private string stageId = "ramp";
        [SerializeField, Min(0f)] private float maxRollbackMeters=.45f;
        [SerializeField, Min(.1f)] private float holdSeconds=.8f;
        [SerializeField, Min(1f)] private float timeoutSeconds=45f;
        private TwoWheelerRampValidator validator;
        private bool running,exitRequested;
        public bool IsRunning => running;

        private float Position()
        {
            Vector3 axis=uphillDirection==null?transform.forward:uphillDirection.forward;
            return Vector3.Dot(vehicle.transform.position,axis.normalized);
        }
        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }
        public void ResetAttempt() { running=false; exitRequested=false; validator=null; }
        public void Begin()
        {
            if (running || trial == null || vehicle == null ||
                trial.State != ExamStage.Running || trial.IsPaused || trial.CurrentStage != stageId) return;
            validator=new TwoWheelerRampValidator(maxRollbackMeters,holdSeconds,timeoutSeconds);
            validator.Begin(Position());
            running=true; exitRequested=false;
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
            CourseEvent result=validator.Sample(Position(),withinStop,vehicle.SpeedKph/3.6f,
                vehicle.FootDown,exitRequested,Time.fixedDeltaTime);
            exitRequested=false;
            if (result.Kind == CourseEventKind.None) return;
            running=false;
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
            else trial.ReportViolation(result.Code);
        }
    }
}
