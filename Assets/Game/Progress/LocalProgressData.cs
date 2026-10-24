using System;
using System.Collections.Generic;

namespace NepalTrial.Progress
{
    [Serializable]
    public sealed class LocalProgressData
    {
        public int schemaVersion = 1;
        public string selectedVehicleId = "starter-car";
        public List<string> unlockedVehicleIds = new List<string> { "starter-car" };
        public List<CourseRecord> records = new List<CourseRecord>();
    }

    [Serializable]
    public sealed class CourseRecord
    {
        public string courseId;
        public int bestScore;
        public int attempts;
        public int completions;
        public string lastRunId;
    }

    /// <summary>Platform-independent data updates; serialize using Unity JsonUtility or another store.</summary>
    public static class ProgressRules
    {
        public static CourseRecord RecordAttempt(LocalProgressData progress, string courseId, int score, bool passed)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (string.IsNullOrWhiteSpace(courseId)) throw new ArgumentException("Course ID is required.", nameof(courseId));
            if (progress.records == null) progress.records = new List<CourseRecord>();
            CourseRecord record = progress.records.Find(x => x.courseId == courseId);
            if (record == null)
            {
                record = new CourseRecord { courseId = courseId };
                progress.records.Add(record);
            }
            record.attempts++;
            if (passed) record.completions++;
            record.bestScore = Math.Max(record.bestScore, score);
            return record;
        }

        /// <summary>Ensures recovering a saved exam run does not count it twice.</summary>
        public static bool RecordRunOnce(LocalProgressData progress, string courseId,
            int score, bool passed, string runId)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (string.IsNullOrEmpty(runId)) throw new ArgumentException("Run ID is required.", nameof(runId));
            if (progress.records != null)
            {
                var existing = progress.records.Find(x => x != null && x.courseId == courseId);
                if (existing != null && existing.lastRunId == runId) return false;
            }
            var record = RecordAttempt(progress,courseId,score,passed);
            record.lastRunId = runId;
            return true;
        }

        public static bool UnlockVehicle(LocalProgressData progress, string vehicleId)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (string.IsNullOrWhiteSpace(vehicleId)) return false;
            if (progress.unlockedVehicleIds == null) progress.unlockedVehicleIds = new List<string>();
            if (progress.unlockedVehicleIds.Contains(vehicleId)) return false;
            progress.unlockedVehicleIds.Add(vehicleId);
            return true;
        }
    }
}
