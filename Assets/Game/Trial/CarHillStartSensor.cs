using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Tracks signed uphill position after entering the hill-start section.</summary>
    public sealed class CarHillStartSensor : MonoBehaviour
    {
        [SerializeField] private CarDriveController car;
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private Transform uphillAxis;
        [SerializeField] private string stageId = "hill-start";
        [SerializeField] private BoxCollider uphillStopZone;
        [SerializeField] private BoxCollider downhillStopZone;
        [SerializeField] private Transform descentStart;
        [SerializeField, Min(.1f)] private float requiredStopSeconds = .8f;
        private float uphillHold, downhillHold;
        private bool uphillStopped, downhillStopped, descending;
        [SerializeField, Min(0f)] private float maxRollbackMeters = 0.5f;
        [SerializeField, Min(1f)] private float maximumSeconds = 20f;
        private HillStartValidator validator;
        private bool running;
        private bool exitRequested;

        private float PositionAlongHill()
        {
            Vector3 axis = uphillAxis == null ? transform.forward : uphillAxis.forward;
            return Vector3.Dot(car.transform.position, axis.normalized);
        }

        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }
        public void ResetAttempt()
        {
            running=false;exitRequested=false;validator=null;
            uphillHold=downhillHold=0f;
            uphillStopped=downhillStopped=descending=false;
        }
        public void Begin()
        {
            if (running || car == null || trial == null || trial.State != ExamStage.Running ||
                trial.IsPaused || trial.CurrentStage != stageId) return;
            validator = new HillStartValidator(maxRollbackMeters, maximumSeconds);
            validator.Begin(PositionAlongHill());
            running = true;
            exitRequested = false;
            uphillHold=downhillHold=0f;
            uphillStopped=downhillStopped=descending=false;
        }

        public void RequestExit() => exitRequested = true;

        private bool Inside(BoxCollider region)
        {
            Vector3 p=region.transform.InverseTransformPoint(car.transform.position)-region.center;
            Vector3 half=region.size*.5f;
            return Mathf.Abs(p.x)<=half.x && Mathf.Abs(p.z)<=half.z;
        }

        private void FixedUpdate()
        {
            if (!running || car == null || trial == null || validator == null || trial.IsPaused ||
                trial.State != ExamStage.Running || trial.CurrentStage != stageId) return;
            if (uphillStopZone != null && !uphillStopped)
            {
                uphillHold = Inside(uphillStopZone) && car.SpeedKph < .72f
                    ? uphillHold + Time.fixedDeltaTime : 0f;
                uphillStopped = uphillHold >= requiredStopSeconds;
            }
            if (descentStart != null)
                descending |= PositionAlongHill() >= Vector3.Dot(descentStart.position,
                    (uphillAxis == null ? transform.forward : uphillAxis.forward).normalized);
            if (downhillStopZone != null && descending && !downhillStopped)
            {
                downhillHold = Inside(downhillStopZone) && car.SpeedKph < .72f
                    ? downhillHold + Time.fixedDeltaTime : 0f;
                downhillStopped = downhillHold >= requiredStopSeconds;
            }
            bool exitNow=exitRequested;
            CourseEvent result = validator.Sample(PositionAlongHill(),Time.fixedDeltaTime,
                exitNow,descending);
            exitRequested=false;
            if (exitNow && ((uphillStopZone!=null && !uphillStopped) ||
                            (downhillStopZone!=null && !downhillStopped)))
                result = new CourseEvent(CourseEventKind.Violation,"hill-no-stop");
            if (result.Kind == CourseEventKind.None) return;
            running = false;
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
            else trial.ReportViolation(result.Code);
        }
    }
}
