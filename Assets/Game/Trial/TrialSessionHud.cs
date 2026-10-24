using UnityEngine;
using UnityEngine.UI;

namespace NepalTrial.Trial
{
    /// <summary>Optional Unity UI bridge for trial session. Assign text fields in Inspector.</summary>
    public sealed class TrialSessionHud : MonoBehaviour
    {
        [SerializeField] private TrialGameplayRuntime trial;
        [SerializeField] private Text stageLabel;
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Text timeLabel;
        [SerializeField] private Text stateLabel;
        [SerializeField] private Text violationsLabel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private CarFigureEightSensor figureEight;
        [SerializeField] private Text checkpointLabel;

        private void Update()
        {
            if (trial == null) return;
            if (stageLabel != null) stageLabel.text = "Stage: " + trial.CurrentStage;
            if (scoreLabel != null) scoreLabel.text = "Score: " + trial.Score;
            if (timeLabel != null) timeLabel.text = "Time: " + Mathf.CeilToInt(trial.RemainingSeconds);
            if (stateLabel != null) stateLabel.text = trial.IsPaused ? "Paused" : trial.State.ToString();
            if (violationsLabel != null) violationsLabel.text = "Violations: " + trial.ViolationCount;
            if (checkpointLabel != null) checkpointLabel.text = figureEight != null ?
                "Checkpoints: " + figureEight.CheckpointsReached + "/" + figureEight.CheckpointCount : "";
            if (resultsPanel != null)
            {
                bool finished = trial.State == ExamStage.Passed || trial.State == ExamStage.Failed
                    || trial.State == ExamStage.Cancelled;
                if (resultsPanel.activeSelf != finished) resultsPanel.SetActive(finished);
            }
        }

        public void Pause() { if (trial != null) trial.SetPaused(true); }
        public void Resume() { if (trial != null) trial.SetPaused(false); }
        public void Restart() { if (trial != null) trial.RestartAttempt(); }
        public void Cancel() { if (trial != null) trial.CancelAttempt(); }
    }
}
