using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Observes heading throughout a U-turn; trigger gates call Begin/Exit.</summary>
    public sealed class CarUTurnSensor : MonoBehaviour
    {
        [SerializeField] private CarDriveController car;
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private string stageId = "u-turn";
        [SerializeField, Min(1f)] private float minimumTurnDegrees = 140f;
        [SerializeField, Min(1f)] private float maximumSeconds = 30f;
        private UTurnValidator validator;
        private bool running;
        private bool outside;
        private bool exitRequested;
        private float lastHeading;
        public bool IsRunning => running;

        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }
        public void ResetAttempt() { running = false; exitRequested = false; validator = null; }
        public void Begin()
        {
            if (car == null || trial == null || trial.State != ExamStage.Running ||
                trial.IsPaused || trial.CurrentStage != stageId) return;
            validator = new UTurnValidator(minimumTurnDegrees, maximumSeconds);
            validator.Begin();
            lastHeading = car.transform.eulerAngles.y;
            running = true;
            outside = false;
            exitRequested = false;
        }

        public void RequestExit() => exitRequested = true;
        public void SetOutsideBoundary(bool value) => outside = value;

        private void FixedUpdate()
        {
            if (!running || car == null || trial == null || validator == null || trial.IsPaused ||
                trial.State != ExamStage.Running || trial.CurrentStage != stageId) return;
            float nextHeading = car.transform.eulerAngles.y;
            float delta = Mathf.DeltaAngle(lastHeading, nextHeading);
            lastHeading = nextHeading;
            CourseEvent result = validator.Sample(delta, Time.fixedDeltaTime, outside, exitRequested);
            exitRequested = false;
            if (result.Kind == CourseEventKind.None) return;
            running = false;
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
            else trial.ReportViolation(result.Code);
        }
    }
}
