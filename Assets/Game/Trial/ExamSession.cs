using System;

namespace NepalTrial.Trial
{
    public enum ExamStage { Ready, Running, Passed, Failed, Cancelled }

    /// <summary>Pure state and penalty logic; caller supplies verified rules per course.</summary>
    public sealed class ExamSession
    {
        public ExamStage Stage { get; private set; } = ExamStage.Ready;
        public int Score { get; private set; }
        public int ViolationCount { get; private set; }
        public int StartingScore { get; private set; }
        public int MinimumPassingScore { get; private set; }

        public ExamSession(int startingScore, int minimumPassingScore)
        {
            if (startingScore <= 0 || minimumPassingScore < 0 || minimumPassingScore > startingScore)
                throw new ArgumentOutOfRangeException(nameof(minimumPassingScore));
            StartingScore = startingScore;
            MinimumPassingScore = minimumPassingScore;
            Reset();
        }

        public void Reset()
        {
            Stage = ExamStage.Ready;
            Score = StartingScore;
            ViolationCount = 0;
        }
        /// <summary>Restore a saved checkpoint at the start of a later stage.</summary>
        public void RestoreRunning(int score,int violations)
        {
            if(score < MinimumPassingScore || score > StartingScore || violations < 0)
                throw new ArgumentOutOfRangeException(nameof(score));
            Reset();
            Score=score;
            ViolationCount=violations;
            Stage=ExamStage.Running;
        }

        public bool Start()
        {
            if (Stage != ExamStage.Ready) return false;
            Stage = ExamStage.Running;
            return true;
        }
        public bool ApplyPenalty(int points, bool immediateFailure = false)
        {
            if (Stage != ExamStage.Running || points < 0) return false;
            Score = Math.Max(0, Score - points);
            ViolationCount++;
            if (immediateFailure || Score < MinimumPassingScore) Stage = ExamStage.Failed;
            return true;
        }
        public bool Finish()
        {
            if (Stage != ExamStage.Running) return false;
            Stage = Score >= MinimumPassingScore ? ExamStage.Passed : ExamStage.Failed;
            return true;
        }
        public bool Cancel()
        {
            if (Stage != ExamStage.Ready && Stage != ExamStage.Running) return false;
            Stage = ExamStage.Cancelled;
            return true;
        }
    }
}
