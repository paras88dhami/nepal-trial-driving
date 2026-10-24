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
        private readonly Dictionary<string,int> failAtOccurrence;
        private readonly Dictionary<string,int> eventCounts = new Dictionary<string,int>();
        private int currentStage;
        private bool paused;
        public ExamStage State => exam.Stage;
        public int Score => exam.Score;
        public int CurrentStageIndex => currentStage;
        public string CurrentStage => currentStage < stages.Count ? stages[currentStage] : "";
        public float RemainingSeconds => clock.RemainingSeconds;
        public int ViolationCount => exam.ViolationCount;
        public int CountViolation(string code) => code != null && eventCounts.TryGetValue(code,out int n) ? n : 0;
        public bool IsPaused => paused;

        public TrialRunCoordinator(IList<string> stageIds, IDictionary<string, int> violationPenalties,
            int startingScore, int minimumPassingScore, float totalSeconds,
            IDictionary<string,int> failAtOccurrence = null)
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
            this.failAtOccurrence = new Dictionary<string,int>();
            if (failAtOccurrence != null)
                foreach (var limit in failAtOccurrence)
                {
                    if (string.IsNullOrEmpty(limit.Key) || limit.Value <= 0 ||
                        !penalties.ContainsKey(limit.Key))
                        throw new ArgumentException("Invalid immediate-failure rule.");
                    this.failAtOccurrence.Add(limit.Key, limit.Value);
                }
            exam = new ExamSession(startingScore, minimumPassingScore);
            clock = new CountdownClock(totalSeconds);
        }

        public bool Start()
        {
            if (!exam.Start()) return false;
            clock.Start();
            paused = false;
            return true;
        }

        public bool SetPaused(bool value)
        {
            if (exam.Stage != ExamStage.Running) return false;
            if (paused == value) return true;
            paused = value;
            if (paused) clock.Pause();
            else clock.Start();
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            if (exam.Stage != ExamStage.Running || paused) return;
            clock.Tick(deltaSeconds);
            if (clock.HasExpired) exam.ApplyPenalty(0, true);
        }

        public bool ReportViolation(string eventCode)
        {
            if (exam.Stage != ExamStage.Running || paused || !penalties.TryGetValue(eventCode, out int points)) return false;
            int occurrence = CountViolation(eventCode) + 1;
            eventCounts[eventCode] = occurrence;
            return exam.ApplyPenalty(points, failAtOccurrence.TryGetValue(eventCode, out int limit)
                && occurrence >= limit);
        }

        public bool AdvanceStage(string completedStageId)
        {
            if (exam.Stage != ExamStage.Running || paused || completedStageId != CurrentStage) return false;
            currentStage++;
            if (currentStage == stages.Count)
            {
                clock.Pause();
                exam.Finish();
            }
            return true;
        }

        /// <summary>Resume at the beginning of the next unplayed physical stage.</summary>
        public void RestoreCheckpoint(int stageIndex,int score,int violations,float remainingSeconds)
        {
            if(stageIndex < 0 || stageIndex >= stages.Count)
                throw new ArgumentOutOfRangeException(nameof(stageIndex));
            if(remainingSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(remainingSeconds));
            exam.RestoreRunning(score,violations);
            currentStage=stageIndex;
            eventCounts.Clear();
            clock.Reset(remainingSeconds);
            clock.Start();
            paused=false;
        }

        public void Reset(float durationSeconds)
        {
            exam.Reset();
            eventCounts.Clear();
            clock.Reset(durationSeconds);
            currentStage = 0;
            paused = false;
        }

        public bool Cancel()
        {
            bool cancelled = exam.Cancel();
            if (cancelled) { clock.Pause(); paused = false; }
            return cancelled;
        }
    }
}
