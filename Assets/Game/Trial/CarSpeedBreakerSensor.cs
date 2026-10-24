using System.Collections.Generic;
using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Enter a speed breaker trigger to validate crossing speed using live vehicle velocity.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CarSpeedBreakerSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private CarDriveController car;
        [SerializeField] private string stageId = "speed-breaker";
        [SerializeField, Min(0.1f)] private float maxCrossingSpeedKph = 12f;
        private SpeedBreakerValidator validator;
        private readonly HashSet<Collider> overlaps = new HashSet<Collider>();

        private void Awake() => validator = new SpeedBreakerValidator(Mathf.Max(0.1f, maxCrossingSpeedKph));
        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (trial == null || car == null || other.GetComponentInParent<CarDriveController>() != car
                || !overlaps.Add(other) || overlaps.Count != 1) return;
            CourseEvent result = validator.Sample(true, car.SpeedKph);
            if (result.Kind == CourseEventKind.Violation) trial.ReportViolation(result.Code);
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!overlaps.Remove(other) || overlaps.Count != 0) return;
            validator.Sample(false, 0f);
        }

        private void OnDisable()
        {
            overlaps.Clear();
            if (validator != null) validator.Reset();
        }
    }
}
