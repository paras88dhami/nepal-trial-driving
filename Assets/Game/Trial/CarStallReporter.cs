using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Once-per-stall deductions for manual connected car examination.</summary>
    public sealed class CarStallReporter : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private CarDriveController car;
        private int observedStalls;
        private int stallsThisStage;

        private void OnEnable()
        {
            if (trial == null) return;
            trial.AttemptRestarted += ResetAttempt;
            trial.StageCompleted += OnStageCompleted;
        }

        private void OnDisable()
        {
            if (trial == null) return;
            trial.AttemptRestarted -= ResetAttempt;
            trial.StageCompleted -= OnStageCompleted;
        }

        private void ResetAttempt()
        {
            observedStalls=0;
            stallsThisStage=0;
        }

        private void OnStageCompleted(string stage)
        {
            stallsThisStage=0;
        }

        private void FixedUpdate()
        {
            if (trial == null || car == null || trial.State != ExamStage.Running ||
                trial.IsPaused) return;
            int current=car.StallCount;
            if (current < observedStalls)
            {
                observedStalls=current;
                stallsThisStage=0;
                return;
            }
            while (observedStalls < current && trial.State == ExamStage.Running)
            {
                observedStalls++;
                stallsThisStage++;
                string code=null;
                if (trial.CurrentStage == "figure-eight")
                    code="figure-eight-stall";
                else if (trial.CurrentStage == "speed-breaker")
                    code=stallsThisStage==1 ? "breaker-first-stall" : "breaker-repeat-stall";
                else if (trial.CurrentStage == "hill-start")
                    code=stallsThisStage==1 ? "hill-first-stall" : "hill-repeat-stall";
                if (code != null) trial.ReportViolation(code);
            }
        }
    }
}
