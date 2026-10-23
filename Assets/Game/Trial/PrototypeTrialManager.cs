using UnityEngine;
using UnityEngine.UI;

namespace NepalTrial.Trial
{
    /// <summary>Temporary feedback only; no official Nepal marks are assumed.</summary>
    public sealed class PrototypeTrialManager : MonoBehaviour
    {
        [SerializeField] private BoundaryDetector[] boundaries;
        [SerializeField] private Text feedback;
        [SerializeField] private Text countLabel;
        private int hits;

        private void OnEnable()
        {
            if (boundaries == null) return;
            foreach (var boundary in boundaries)
                if (boundary != null) boundary.BoundaryHit += HandleHit;
        }

        private void OnDisable()
        {
            if (boundaries == null) return;
            foreach (var boundary in boundaries)
                if (boundary != null) boundary.BoundaryHit -= HandleHit;
        }

        private void Start() => UpdateLabel();

        private void HandleHit(string name)
        {
            hits++;
            if (feedback != null) feedback.text = "Practice warning: boundary contacted (" + name + ")";
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (countLabel != null) countLabel.text = "Boundary contacts: " + hits;
        }

        public void ResetPractice()
        {
            hits = 0;
            if (feedback != null) feedback.text = "Practice mode - unofficial course";
            UpdateLabel();
        }
    }
}
