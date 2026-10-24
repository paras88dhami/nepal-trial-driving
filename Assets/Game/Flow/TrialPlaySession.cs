using System;
using NepalTrial.Progress;

namespace NepalTrial.Flow
{
    /// <summary>Serializable run state. Exam grade math is provisional and configurable.</summary>
    [Serializable]
    public sealed class TrialPlaySession
    {
        public int schemaVersion = 1;
        public string vehicleId;
        public DriveMode mode;
        public string[] courses;
        public int stageIndex;
        public int completedPenaltyPoints;
        public int pendingStageScore = -1;
        public bool pendingStagePassed;
        public bool ended;
        public bool passed;
        public bool recordWritten;
        public int finalScore = 100;

        public string CurrentCourse =>
            courses != null && stageIndex >= 0 && stageIndex < courses.Length
                ? courses[stageIndex] : null;
        public bool AwaitingNext => !ended && pendingStageScore >= 0;
    }

    public static class TrialSessionRules
    {
        public const int PassingScore = 70;
        public const int StartingScore = 100;

        public static TrialPlaySession Create(string vehicleId, DriveMode mode, string practiceCourse = null)
        {
            if (!TrialCatalog.IsKnownVehicle(vehicleId)) throw new ArgumentException("Invalid vehicle.",nameof(vehicleId));
            if (!Enum.IsDefined(typeof(DriveMode), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
            string[] courses = mode == DriveMode.Exam ? TrialCatalog.CoursesFor(vehicleId) :
                new[] { practiceCourse };
            if (mode != DriveMode.Exam && TrialCatalog.SceneFor(vehicleId, practiceCourse) == null)
                throw new ArgumentException("Invalid practice course.", nameof(practiceCourse));
            return new TrialPlaySession { vehicleId = vehicleId, mode = mode, courses = courses };
        }

        /// <summary>Completing a stage is idempotent until advancing or restarting it.</summary>
        public static bool ReportStageResult(TrialPlaySession session, bool passed, int score)
        {
            if (session == null || session.ended || session.pendingStageScore >= 0 ||
                string.IsNullOrEmpty(session.CurrentCourse) || score < 0 || score > StartingScore)
                return false;
            session.pendingStageScore = score;
            session.pendingStagePassed = passed;
            if (session.mode == DriveMode.Exam)
            {
                // A failed stage stops the campaign. Success is only committed when moving on
                // or finishing the last course, so restarting a stage cannot double-count.
                if (!passed || session.stageIndex == session.courses.Length - 1)
                {
                    CommitPending(session);
                    CompleteRun(session);
                }
            }
            else
            {
                session.finalScore = score;
                session.passed = passed;
                session.ended = true;
            }
            return true;
        }

        public static bool Advance(TrialPlaySession session)
        {
            if (session == null || session.ended || session.mode != DriveMode.Exam ||
                session.pendingStageScore < 0 || !session.pendingStagePassed ||
                session.stageIndex >= session.courses.Length - 1) return false;
            CommitPending(session);
            session.stageIndex++;
            return true;
        }

        public static void RestartStage(TrialPlaySession session)
        {
            if (session == null || session.ended) return;
            session.pendingStageScore = -1;
            session.pendingStagePassed = false;
        }

        private static void CommitPending(TrialPlaySession session)
        {
            session.completedPenaltyPoints += StartingScore - session.pendingStageScore;
            session.completedPenaltyPoints = Math.Min(StartingScore,session.completedPenaltyPoints);
            session.pendingStageScore = -1;
            session.pendingStagePassed = false;
        }

        private static void CompleteRun(TrialPlaySession session)
        {
            session.ended = true;
            session.finalScore = Math.Max(0,StartingScore-session.completedPenaltyPoints);
            session.passed = session.finalScore >= PassingScore &&
                session.stageIndex == session.courses.Length - 1;
        }

        public static string RecordKey(TrialPlaySession session) =>
            session.mode == DriveMode.Exam ? "exam-" + session.vehicleId :
            "practice-" + session.vehicleId + "-" + session.CurrentCourse;

        public static bool RecordIfNeeded(TrialPlaySession session, LocalProgressData data)
        {
            if (session == null || data == null || !session.ended || session.recordWritten ||
                session.mode == DriveMode.FreeDrive) return false;
            // Practice stage results are recorded by the gameplay runtime. The session
            // service only records an EXAM aggregate to prevent double counting.
            if (session.mode != DriveMode.Exam) { session.recordWritten = true; return false; }
            ProgressRules.RecordAttempt(data,RecordKey(session),session.finalScore,session.passed);
            session.recordWritten = true;
            return true;
        }
    }
}
