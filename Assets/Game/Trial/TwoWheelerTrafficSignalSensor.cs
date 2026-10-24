using System.Collections.Generic;
using NepalTrial.Vehicles.TwoWheeler;
using UnityEngine;

namespace NepalTrial.Trial
{
    [RequireComponent(typeof(Collider))]
    public sealed class TwoWheelerTrafficSignalSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        [SerializeField] private string stageId = "traffic-light";
        [SerializeField] private TrafficLightColor light = TrafficLightColor.Red;
        private readonly TrafficLightValidator validator=new TrafficLightValidator();
        private readonly HashSet<Collider> overlaps=new HashSet<Collider>();

        public void SetLight(TrafficLightColor value) => light=value;
        private void Reset() => GetComponent<Collider>().isTrigger=true;
        private void OnEnable() { if (trial != null) trial.AttemptRestarted += ResetAttempt; }
        private void OnDisable() { if (trial != null) trial.AttemptRestarted -= ResetAttempt; ResetAttempt(); }
        public void ResetAttempt() { overlaps.Clear(); validator.Reset(); }

        private void OnTriggerEnter(Collider other)
        {
            if (trial == null || vehicle == null || trial.State != ExamStage.Running ||
                trial.IsPaused || trial.CurrentStage != stageId ||
                other.GetComponentInParent<TwoWheelerRigidbodyController>() != vehicle ||
                !overlaps.Add(other) || overlaps.Count != 1) return;
            CourseEvent evt=validator.Sample(true,light);
            if (evt.Kind == CourseEventKind.Violation) trial.ReportViolation(evt.Code);
            if (trial.State == ExamStage.Running &&
                (evt.Kind == CourseEventKind.Completed || evt.Kind == CourseEventKind.Violation))
                trial.ReportStage(stageId);
        }
        private void OnTriggerExit(Collider other)
        {
            if (overlaps.Remove(other) && overlaps.Count == 0) validator.Sample(false,light);
        }
    }
}
