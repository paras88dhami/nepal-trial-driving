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

        public void Begin()
        {
            if (car == null) return;
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
            if (!running || car == null || trial == null || validator == null) return;
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
