using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>One fall violation per attempt; reports foot-down on press transitions.</summary>
    public sealed class TwoWheelerFallReporter : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField, Range(30f,85f)] private float fallThreshold = 58f;
        [SerializeField] private string fallCode = "two-wheeler-fall";
        [SerializeField] private string footDownCode = "foot-down";
        private TwoWheelerBalanceValidator validator;
        private bool footPreviouslyDown;

        private void Awake() => validator = new TwoWheelerBalanceValidator(fallThreshold);
        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; }

        private void ResetAttempt()
        {
            footPreviouslyDown=false;
            if (validator != null) validator.Reset();
        }

        private void FixedUpdate()
        {
            if (trial == null || vehicle == null || validator == null ||
                trial.State != ExamStage.Running || trial.IsPaused) return;
            CourseEvent fall = validator.Sample(vehicle.TiltDegrees);
            if (fall.Kind == CourseEventKind.Violation) trial.ReportViolation(fallCode);
            bool footDown = vehicle.FootDown;
            if (footDown && !footPreviouslyDown) trial.ReportViolation(footDownCode);
            footPreviouslyDown = footDown;
        }
    }
}
