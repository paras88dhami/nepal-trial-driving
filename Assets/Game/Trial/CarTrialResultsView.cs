using UnityEngine;
using UnityEngine.UI;

namespace NepalTrial.Trial
{
    /// <summary>Results text is refreshed even while its panel is hidden.</summary>
    public sealed class CarTrialResultsView : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private Text resultsLabel;
        private void Update()
        {
            if (trial == null || resultsLabel == null) return;
            resultsLabel.text = "Result: " + trial.State + "\nScore: " + trial.Score +
                "\nViolations: " + trial.ViolationCount;
        }
    }
}
