using System.Collections.Generic;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Reports one penalty upon entry by a matching collider; re-arms on exit.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class TrialViolationGate : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private string violationCode = "collision";
        [SerializeField] private string vehicleTag = "Player";
        private readonly HashSet<Collider> inside = new HashSet<Collider>();

        private void OnTriggerEnter(Collider other)
        {
            if (trial == null || !other.CompareTag(vehicleTag) || !inside.Add(other)) return;
            trial.ReportViolation(violationCode);
        }

        private void OnTriggerExit(Collider other) => inside.Remove(other);
        private void OnDisable() => inside.Clear();
    }
}
