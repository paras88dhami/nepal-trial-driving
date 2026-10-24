using System;
using System.Collections.Generic;

namespace NepalTrial.Trial
{
    /// <summary>Headless trial orchestration. Scene observers call Report/Advance and Tick.</summary>
    public sealed class TrialRunCoordinator
    {
        private readonly ExamSession exam;
        private readonly CountdownClock clock;
        private readonly List<string> stages;
        private readonly Dictionary<string, int> penalties;
        private int currentStage;
        public ExamStage State => exam.Stage;
        public int Score => exam.Score;
        public int CurrentStageIndex => currentStage;
        public string CurrentStage => currentStage < stages.Count ? stages[currentStage] : "";
        public float RemainingSeconds => clock.RemainingSeconds;
        public int ViolationCount => exam.ViolationCount;

        public TrialRunCoordinator(IList<string> stageIds, IDictionary<string, int> violationPenalties,
            int startingScore, int minimumPassingScore, float totalSeconds)
        {
            if (stageIds == null || stageIds.Count == 0) throw new ArgumentException("At least one stage required.");
            stages = new List<string>();
            foreach (string id in stageIds)
            {
                if (string.IsNullOrEmpty(id) || stages.Contains(id)) throw new ArgumentException("Stage IDs must be unique and nonempty.");
                stages.Add(id);
            }
            penalties = new Dictionary<string, int>();
            if (violationPenalties != null)
                foreach (var rule in violationPenalties)
                {
                    if (string.IsNullOrEmpty(rule.Key) || rule.Value < 0) throw new ArgumentException("Invalid penalty.");
                    penalties.Add(rule.Key, rule.Value);
                }
            exam = new ExamSession(startingScore, minimumPassingScore);
            clock = new CountdownClock(totalSeconds);
        }

        public bool Start()
        {
            if (!exam.Start()) return false;
            clock.Start();
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            if (exam.Stage != ExamStage.Running) return;
            clock.Tick(deltaSeconds);
            if (clock.HasExpired) exam.ApplyPenalty(0, true);
        }

        public bool ReportViolation(string eventCode)
        {
            if (exam.Stage != ExamStage.Running || !penalties.TryGetValue(eventCode, out int points)) return false;
            return exam.ApplyPenalty(points);
        }

        public bool AdvanceStage(string completedStageId)
        {
            if (exam.Stage != ExamStage.Running || completedStageId != CurrentStage) return false;
            currentStage++;
            if (currentStage == stages.Count)
            {
                clock.Pause();
                exam.Finish();
            }
            return true;
        }

        public void Reset(float durationSeconds)
        {
            exam.Reset();
            clock.Reset(durationSeconds);
            currentStage = 0;
        }

        public bool Cancel()
        {
            bool cancelled = exam.Cancel();
            if (cancelled) clock.Pause();
            return cancelled;
        }
    }
}
