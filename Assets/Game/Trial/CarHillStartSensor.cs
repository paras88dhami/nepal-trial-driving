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
        public void ResetAttempt() { running = false; exitRequested = false; validator = null; }
        public void Begin()
        {
            if (car == null || trial == null || trial.State != ExamStage.Running ||
                trial.IsPaused || trial.CurrentStage != stageId) return;
            validator = new HillStartValidator(maxRollbackMeters, maximumSeconds);
            validator.Begin(PositionAlongHill());
            running = true;
            exitRequested = false;
        }

        public void RequestExit() => exitRequested = true;

        private void FixedUpdate()
        {
            if (!running || car == null || trial == null || validator == null || trial.IsPaused ||
                trial.State != ExamStage.Running || trial.CurrentStage != stageId) return;
            CourseEvent result = validator.Sample(PositionAlongHill(), Time.fixedDeltaTime, exitRequested);
            exitRequested = false;
            if (result.Kind == CourseEventKind.None) return;
            running = false;
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
            else trial.ReportViolation(result.Code);
        }
    }
}
