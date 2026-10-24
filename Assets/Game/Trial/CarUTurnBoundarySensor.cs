using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Checks the complete vehicle body remains inside a rectangular U-turn driving area.</summary>
    public sealed class CarUTurnBoundarySensor : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private CarUTurnSensor maneuver;
        [SerializeField] private BoxCollider drivingRegion;
        [SerializeField] private BoxCollider vehicleBody;

        private void FixedUpdate()
        {
            if (trial == null || maneuver == null || drivingRegion == null || vehicleBody == null) return;
            if (trial.State != ExamStage.Running || trial.IsPaused ||
                trial.CurrentStage != "u-turn" || !maneuver.IsRunning) return;
            maneuver.SetOutsideBoundary(!CarGarageGeometry.VehicleFullyInside(drivingRegion, vehicleBody, .1f));
        }
    }
}
