using System.Collections.Generic;
using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Counts bump entry speed and foot-down incident without a duplicate per collider.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class TwoWheelerBumpySensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private string stageId = "speed-breaker";
        [SerializeField, Min(.1f)] private float maxSafeSpeedKph = 12f;
        [SerializeField] private bool enforceTrainingSpeedLimit = true;
        private SpeedBreakerValidator validator;
        private readonly HashSet<Collider> overlaps=new HashSet<Collider>();
        private bool footBefore;
        private bool crossed;

        private void Awake() => validator=new SpeedBreakerValidator(Mathf.Max(.1f,maxSafeSpeedKph));
        private void Reset() => GetComponent<Collider>().isTrigger=true;
        private void OnEnable() { if(trial!=null) trial.AttemptRestarted+=ResetAttempt; }
        private void OnDisable() { if(trial!=null) trial.AttemptRestarted-=ResetAttempt; ResetAttempt(); }
        public void ResetAttempt()
        {
            overlaps.Clear();footBefore=false;crossed=false;
            if (validator!=null)validator.Reset();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (trial == null || vehicle == null || trial.State != ExamStage.Running ||
                trial.IsPaused || trial.CurrentStage != stageId || crossed ||
                other.GetComponentInParent<TwoWheelerRigidbodyController>() != vehicle ||
                !overlaps.Add(other) || overlaps.Count != 1) return;
            CourseEvent evt=validator.Sample(true,
                enforceTrainingSpeedLimit ? vehicle.SpeedKph : 0f);
            if (evt.Kind==CourseEventKind.Violation) trial.ReportViolation(evt.Code);
        }
        private void FixedUpdate()
        {
            if (trial==null || vehicle==null || trial.State!=ExamStage.Running || trial.IsPaused ||
                trial.CurrentStage!=stageId || overlaps.Count==0 || crossed) return;
            bool foot=vehicle.FootDown;
            if(foot && !footBefore) trial.ReportViolation("breaker-foot-down");
            footBefore=foot;
        }
        private void OnTriggerExit(Collider other)
        {
            if (!overlaps.Remove(other) || overlaps.Count!=0 || crossed) return;
            if (validator!=null) validator.Sample(false,0f);
            if (trial!=null && trial.CurrentStage==stageId && trial.State==ExamStage.Running &&
                !trial.IsPaused && trial.ReportStage(stageId)) crossed=true;
        }
    }
}
