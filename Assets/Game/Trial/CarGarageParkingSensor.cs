using System.Collections.Generic;
using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Validates stationary parking inside an oriented garage box.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CarGarageParkingSensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private CarDriveController car;
        [SerializeField] private BoxCollider vehicleBody;
        [SerializeField] private string stageId = "garage";
        [SerializeField, Min(0.1f)] private float requiredStopSeconds = 2f;
        [SerializeField] private bool requireReverseApproach;
        [SerializeField] private CarLBackPathSensor pathSensor;
        private bool reversedInStage;
        private bool drivingCorrectionActive;
        private BoxCollider region;
        private GarageParkingValidator validator;
        private readonly HashSet<Collider> overlaps = new HashSet<Collider>();

        private void Awake()
        {
            region = GetComponent<BoxCollider>();
            validator = new GarageParkingValidator(Mathf.Max(.1f, requiredStopSeconds));
            if (car != null && vehicleBody == null) vehicleBody = car.GetComponent<BoxCollider>();
        }

        private void Reset() => GetComponent<BoxCollider>().isTrigger = true;
        private void OnEnable() { if (trial != null) trial.AttemptRestarted += Clear; }
        private void Clear()
        {
            overlaps.Clear();
            reversedInStage=false;
            drivingCorrectionActive=false;
            if (validator != null) validator.Reset();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (car != null && other.GetComponentInParent<CarDriveController>() == car)
                overlaps.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (overlaps.Remove(other) && overlaps.Count == 0 && validator != null)
                validator.Reset();
        }

        private void FixedUpdate()
        {
            if (trial == null || trial.IsPaused || trial.State != ExamStage.Running || trial.CurrentStage != stageId || car == null || vehicleBody == null || validator == null || overlaps.Count == 0) return;
            // In the connected exam a real reverse manoeuvre is mandatory.
            // Once reversing has started, every transition back into Drive is
            // one correction; the second is a disqualifying event in the catalog.
            if (requireReverseApproach)
            {
                if (car.CurrentGear == DriveGear.Reverse)
                {
                    reversedInStage=true;
                    drivingCorrectionActive=false;
                }
                else if (reversedInStage && car.CurrentGear == DriveGear.Drive &&
                    !drivingCorrectionActive)
                {
                    drivingCorrectionActive=true;
                    trial.ReportViolation("garage-forward-correction");
                }
                if (!reversedInStage || trial.State != ExamStage.Running) return;
            }
            // Final parking alone is insufficient for a connected L-back.
            // The vehicle must first cross the approach and reverse legs.
            if (pathSensor!=null && !pathSensor.CanComplete)return;
            bool inside = CarGarageGeometry.VehicleFullyInside(region, vehicleBody);
            CourseEvent result = validator.Sample(inside, false, car.SpeedKph / 3.6f, Time.fixedDeltaTime);
            if (result.Kind == CourseEventKind.Completed) trial.ReportStage(stageId);
            if (result.Kind == CourseEventKind.Violation) trial.ReportViolation(result.Code);
        }

        private void OnDisable()
        {
            if (trial != null) trial.AttemptRestarted -= Clear;
            overlaps.Clear();
            if (validator != null) validator.Reset();
        }
    }
}
