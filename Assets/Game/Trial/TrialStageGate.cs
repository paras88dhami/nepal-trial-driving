using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>One-direction trigger gate for trial-stage completion.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class TrialStageGate : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private string stageId = "start";
        [SerializeField] private Transform playerRoot;
        [SerializeField, Min(0.1f)] private float halfWidth = 3f;
        private Vector3 previousPosition;
        private bool hasPrevious;

        private void OnEnable() { hasPrevious = false; }
        private void FixedUpdate()
        {
            if (trial == null || playerRoot == null) return;
            Vector3 current = playerRoot.position;
            if (hasPrevious && DirectionalCheckpointGate.Crossed(previousPosition, current,
                    transform.position, transform.forward, halfWidth))
                trial.ReportStage(stageId);
            previousPosition = current;
            hasPrevious = true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position - transform.right * halfWidth,
                transform.position + transform.right * halfWidth);
        }
    }
}
