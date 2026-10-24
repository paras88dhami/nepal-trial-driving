using System.Collections.Generic;
using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Reports collision with explicitly marked trial obstacles, never the ordinary ground.</summary>
    [RequireComponent(typeof(CarDriveController))]
    public sealed class CarCollisionReporter : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private string violationCode = "obstacle-hit";
        [SerializeField, Min(0f)] private float minimumImpactSpeed = 0.25f;
        private readonly HashSet<TrialObstacle> activeContacts = new HashSet<TrialObstacle>();

        private void OnCollisionEnter(Collision collision)
        {
            if (trial == null || collision.relativeVelocity.magnitude < minimumImpactSpeed) return;
            TrialObstacle obstacle = collision.collider.GetComponentInParent<TrialObstacle>();
            if (obstacle == null || !activeContacts.Add(obstacle)) return;
            trial.ReportViolation(violationCode);
        }

        private void OnCollisionExit(Collision collision)
        {
            TrialObstacle obstacle = collision.collider.GetComponentInParent<TrialObstacle>();
            if (obstacle != null) activeContacts.Remove(obstacle);
        }

        private void OnDisable() => activeContacts.Clear();
    }
}
