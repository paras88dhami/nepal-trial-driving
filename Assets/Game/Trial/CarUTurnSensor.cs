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
        private float signedTurn;
        private bool signaledLeft, signaledRight;
        public bool IsRunning => running;

        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }
        public void ResetAttempt()
        {
            running=false; exitRequested=false; validator=null;
            signedTurn=0f; signaledLeft=signaledRight=false;
        }
        public void Begin()
        {
            if (running || car == null || trial == null || trial.State != ExamStage.Running ||
                trial.IsPaused || trial.CurrentStage != stageId) return;
            // Older practice scenes have no indicator component yet.
            // Install it in PlayMode so the U-turn has keyboard/mobile inputs.
            if(car.GetComponent<CarTurnIndicators>()==null)
                car.gameObject.AddComponent<CarTurnIndicators>();
            validator = new UTurnValidator(minimumTurnDegrees, maximumSeconds);
            validator.Begin();
            lastHeading = car.transform.eulerAngles.y;
            running = true;
            signedTurn=0f;
            signaledLeft=signaledRight=false;
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
            signedTurn+=delta;
            var indicator=car.GetComponent<CarTurnIndicators>();
            if(indicator!=null)
            {
                signaledLeft |= indicator.Left;
                signaledRight |= indicator.Right;
            }
            CourseEvent result = validator.Sample(delta, Time.fixedDeltaTime, outside, exitRequested);
            exitRequested = false;
            if (result.Kind == CourseEventKind.None) return;
            running = false;
            if (result.Kind == CourseEventKind.Completed)
            {
                if (!TwoWheelerIndicatorRules.HasCorrectTurnSignal(
                    signedTurn,signaledLeft,signaledRight))
                    trial.ReportViolation("u-turn-no-indicator");
                if (trial.State == ExamStage.Running)
                    trial.ReportStage(stageId);
            }
            else trial.ReportViolation(result.Code);
        }
    }
}
