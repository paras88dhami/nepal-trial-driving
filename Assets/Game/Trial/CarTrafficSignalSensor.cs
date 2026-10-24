using System.Collections.Generic;
using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>A trigger representing a stop line. Assign the car and trial in the Inspector.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CarTrafficSignalSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private CarDriveController car;
        [SerializeField] private string stageId = "traffic-light";
        [SerializeField] private TrafficLightColor light = TrafficLightColor.Red;
        private readonly TrafficLightValidator validator = new TrafficLightValidator();
        private readonly HashSet<Collider> overlaps = new HashSet<Collider>();

        public void SetLight(TrafficLightColor value) => light = value;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (trial == null || car == null || other.GetComponentInParent<CarDriveController>() != car
                || !overlaps.Add(other) || overlaps.Count != 1) return;
            CourseEvent result = validator.Sample(true, light);
            if (result.Kind == CourseEventKind.Violation) trial.ReportViolation(result.Code);
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!overlaps.Remove(other) || overlaps.Count != 0) return;
            validator.Sample(false, light);
        }

        private void OnDisable()
        {
            overlaps.Clear();
            validator.Reset();
        }
    }
}
