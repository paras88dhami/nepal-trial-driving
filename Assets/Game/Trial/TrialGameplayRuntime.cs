using System;
using System.Collections.Generic;
using NepalTrial.Progress;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>
    /// Scene bridge for a configured trial. Bind stage triggers to StageGate and violation triggers
    /// to ViolationGate. Defaults are DEMONSTRATION rules, not official licensing criteria.
    /// </summary>
    public sealed class TrialGameplayRuntime : MonoBehaviour
    {
        [Serializable]
        public struct PenaltyRule { public string code; public int points; }

        [SerializeField] private string courseId = "prototype-figure-eight";
        [SerializeField] private string[] orderedStages = { "start", "finish" };
        [SerializeField] private PenaltyRule[] penaltyRules;
        [SerializeField, Min(1)] private int startingScore = 100;
        [SerializeField, Min(0)] private int passingScore = 70;
        [SerializeField, Min(1f)] private float durationSeconds = 180f;
        [SerializeField] private bool startAutomatically = true;

        private TrialRunCoordinator coordinator;
        private bool recorded;
        public ExamStage State => coordinator == null ? ExamStage.Ready : coordinator.State;
        public int Score => coordinator == null ? startingScore : coordinator.Score;
        public string CurrentStage => coordinator == null ? "" : coordinator.CurrentStage;
        public float RemainingSeconds => coordinator == null ? durationSeconds : coordinator.RemainingSeconds;
        public bool IsPaused => coordinator != null && coordinator.IsPaused;
        public int ViolationCount => coordinator == null ? 0 : coordinator.ViolationCount;
        public event Action<ExamStage> Finished;
        public event Action AttemptRestarted;
        public event Action<string> StageCompleted;

        private void Awake()
        {
            try
            {
                var penalties = new Dictionary<string, int>();
                if (penaltyRules != null)
                    foreach (var rule in penaltyRules)
                        if (!string.IsNullOrEmpty(rule.code) && !penalties.ContainsKey(rule.code))
                            penalties.Add(rule.code, rule.points);
                coordinator = new TrialRunCoordinator(orderedStages, penalties, startingScore,
                    passingScore, durationSeconds);
            }
            catch (Exception exception)
            {
                enabled = false;
                Debug.LogError("Invalid trial runtime configuration: " + exception.Message, this);
            }
        }

        private void Start()
        {
            if (startAutomatically) StartAttempt();
        }

        public bool StartAttempt() => coordinator != null && coordinator.Start();

        public bool SetPaused(bool paused) => coordinator != null && coordinator.SetPaused(paused);

        private void Update()
        {
            if (coordinator == null || coordinator.State != ExamStage.Running) return;
            coordinator.Tick(Time.deltaTime);
            RecordIfFinished();
        }

        public bool ReportStage(string stageId)
        {
            if (coordinator == null || !coordinator.AdvanceStage(stageId)) return false;
            StageCompleted?.Invoke(stageId);
            RecordIfFinished();
            return true;
        }

        public bool ReportViolation(string code)
        {
            if (coordinator == null || !coordinator.ReportViolation(code)) return false;
            RecordIfFinished();
            return true;
        }

        public void RestartAttempt()
        {
            if (coordinator == null) return;
            coordinator.Reset(durationSeconds);
            recorded = false;
            AttemptRestarted?.Invoke();
            coordinator.Start();
        }

        public void CancelAttempt()
        {
            if (coordinator == null) return;
            coordinator.Cancel();
            RecordIfFinished();
        }

        private void RecordIfFinished()
        {
            if (recorded || coordinator == null || coordinator.State == ExamStage.Running
                || coordinator.State == ExamStage.Ready) return;
            recorded = true;
            if (coordinator.State == ExamStage.Passed || coordinator.State == ExamStage.Failed)
            {
                var store = new LocalProgressStore();
                var progress = store.Load();
                ProgressRules.RecordAttempt(progress, courseId, coordinator.Score, coordinator.State == ExamStage.Passed);
                store.Save(progress);
            }
            Finished?.Invoke(coordinator.State);
        }
    }
}
