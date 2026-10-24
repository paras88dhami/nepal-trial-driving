using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Course-aware motorcycle engine-stall penalties, once per stall event.</summary>
    public sealed class TwoWheelerStallReporter : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        private int lastSeenStalls;
        private int courseStalls;

        private void OnEnable()
        {
            if (trial!=null)
            {
                trial.AttemptRestarted+=ResetAttempt;
                trial.StageCompleted+=ResetStageStalls;
            }
        }
        private void OnDisable()
        {
            if (trial!=null)
            {
                trial.AttemptRestarted-=ResetAttempt;
                trial.StageCompleted-=ResetStageStalls;
            }
        }
        private void ResetStageStalls(string stage) => courseStalls=0;
        public void ResetAttempt() { lastSeenStalls=0;courseStalls=0; }

        private void FixedUpdate()
        {
            if(trial==null || vehicle==null || trial.State!=ExamStage.Running || trial.IsPaused) return;
            int total=vehicle.StallCount;
            if(total < lastSeenStalls){lastSeenStalls=total;courseStalls=0;return;}
            if(total==lastSeenStalls)return;
            int newStalls=total-lastSeenStalls;
            lastSeenStalls=total;
            for (int i=0;i<newStalls;i++)
            {
                courseStalls++;
                string course=trial.CurrentStage;
                string code=course=="figure-eight"?"figure-eight-stall":
                    course=="ramp"?(courseStalls==1?"ramp-first-stall":"ramp-repeat-stall"):
                    course=="speed-breaker"?(courseStalls==1?"breaker-first-stall":"breaker-repeat-stall"):
                    "engine-stall";
                trial.ReportViolation(code);
            }
        }
    }
}
