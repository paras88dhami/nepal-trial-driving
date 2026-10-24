using System;
using NepalTrial.Progress;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NepalTrial.Flow
{
    /// <summary>
    /// Persists the selected game mode and progress across Unity scene loads and app restarts.
    /// No cloud account is required. Do not use saved progress as an anti-cheat/security boundary.
    /// </summary>
    public static class TrialSessionService
    {
        private const string SessionKey = "NepalTrial.ActiveSession.v1";
        private static TrialPlaySession current;
        private static bool loaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticCache()
        {
            // Also handles Unity Editor sessions with domain reload disabled.
            current = null;
            loaded = false;
        }

        public static TrialPlaySession Current
        {
            get { if (!loaded) Load(); return current; }
        }

        public static bool IsSessionScene(string sceneName)
        {
            TrialPlaySession session = Current;
            return session != null &&
                TrialCatalog.SceneFor(session.vehicleId,session.CurrentCourse) == sceneName;
        }

        public static bool IsFreeDriveScene(string sceneName)
            => IsSessionScene(sceneName) && Current.mode == DriveMode.FreeDrive;

        public static bool StartNew(string vehicle, DriveMode mode, string course, out string error)
        {
            error = null;
            var progress = TrialProgressBootstrap.LoadReady();
            if (progress.unlockedVehicleIds == null ||
                !progress.unlockedVehicleIds.Contains(vehicle))
            {
                error = "Vehicle is locked.";
                return false;
            }
            TrialPlaySession proposed;
            try { proposed = TrialSessionRules.Create(vehicle,mode,course); }
            catch (ArgumentException ex) { error=ex.Message; return false; }
            string sceneName = TrialCatalog.SceneFor(proposed.vehicleId,proposed.CurrentCourse);
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                error = "Scene not in Build Settings: " + sceneName +
                    ". Generate trial scenes and run Phase 4 / Register Build Scenes.";
                return false;
            }
            current = proposed;
            loaded = true;
            progress.selectedVehicleId = vehicle;
            new LocalProgressStore().Save(progress);
            Save();
            SceneManager.LoadScene(sceneName);
            return true;
        }

        public static bool Resume(out string error)
        {
            error = null;
            if (Current == null || Current.ended)
            {
                error = "No unfinished session to resume.";
                return false;
            }
            // If a stage finished before leaving the game, resume the next stage rather
            // than restarting the completed stage and risking duplicate score submission.
            bool pendingNext = Current.mode == DriveMode.Exam && Current.AwaitingNext;
            string course = pendingNext && Current.stageIndex + 1 < Current.courses.Length
                ? Current.courses[Current.stageIndex + 1] : Current.CurrentCourse;
            string next = TrialCatalog.SceneFor(Current.vehicleId,course);
            if (!Application.CanStreamedLevelBeLoaded(next))
            {
                error = "Scene not in Build Settings: " + next;
                return false;
            }
            if (pendingNext && TrialSessionRules.Advance(Current)) Save();
            SceneManager.LoadScene(next);
            return true;
        }

        public static bool ReportStageResult(bool passed, int score)
        {
            if (Current == null || Current.mode == DriveMode.FreeDrive) return false;
            if (!TrialSessionRules.ReportStageResult(Current,passed,score)) return false;
            if (Current.ended) PersistExamResult();
            Save();
            return true;
        }

        public static bool Advance(out string error)
        {
            error=null;
            if (Current == null || !Current.AwaitingNext ||
                Current.stageIndex >= Current.courses.Length - 1)
            {
                error = "The current exam stage is not ready to advance.";
                return false;
            }
            string next = TrialCatalog.SceneFor(Current.vehicleId,
                Current.courses[Current.stageIndex + 1]);
            if (!Application.CanStreamedLevelBeLoaded(next))
            {
                error = "Scene not in Build Settings: " + next;
                return false;
            }
            if (!TrialSessionRules.Advance(Current)) return false;
            Save();
            SceneManager.LoadScene(next);
            return true;
        }

        public static void RetryStage()
        {
            if (Current == null) return;
            if (Current.ended)
            {
                // A finished exam restarts at its first stage, never at the scene
                // where the old exam happened to end.
                var old = Current;
                current = TrialSessionRules.Create(old.vehicleId,old.mode,
                    old.mode == DriveMode.Exam ? null : old.CurrentCourse);
                Save();
                string first = TrialCatalog.SceneFor(current.vehicleId,current.CurrentCourse);
                if (Application.CanStreamedLevelBeLoaded(first) &&
                    SceneManager.GetActiveScene().name != first)
                    SceneManager.LoadScene(first);
            }
            else
            {
                TrialSessionRules.RestartStage(Current);
                Save();
            }
        }

        public static void ReturnMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(TrialCatalog.MenuScene))
            {
                Debug.LogError("Main menu scene missing from Build Settings.");
                return;
            }
            SceneManager.LoadScene(TrialCatalog.MenuScene);
        }

        public static void Abandon()
        {
            current=null;
            loaded=true;
            PlayerPrefs.DeleteKey(SessionKey);
            PlayerPrefs.Save();
        }

        private static void PersistExamResult()
        {
            if (Current == null || Current.mode != DriveMode.Exam ||
                Current.recordWritten || !Current.ended) return;
            var store=new LocalProgressStore();
            var progress=store.Load();
            if (TrialSessionRules.RecordIfNeeded(Current,progress))
                store.Save(progress);
        }

        private static void Save()
        {
            if (current == null) PlayerPrefs.DeleteKey(SessionKey);
            else PlayerPrefs.SetString(SessionKey,JsonUtility.ToJson(current));
            PlayerPrefs.Save();
        }

        private static void Load()
        {
            loaded=true;
            current=null;
            string json=PlayerPrefs.GetString(SessionKey,string.Empty);
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var candidate=JsonUtility.FromJson<TrialPlaySession>(json);
                if (candidate == null || candidate.schemaVersion != 1 ||
                    string.IsNullOrEmpty(candidate.runId) ||
                    !TrialCatalog.IsKnownVehicle(candidate.vehicleId) ||
                    candidate.courses == null || candidate.courses.Length == 0 ||
                    candidate.stageIndex < 0 || candidate.stageIndex >= candidate.courses.Length)
                    return;
                string[] expected=TrialCatalog.CoursesFor(candidate.vehicleId);
                if (candidate.mode == DriveMode.Exam)
                {
                    if (candidate.courses.Length != expected.Length) return;
                    for (int i=0;i<expected.Length;i++)
                        if (candidate.courses[i] != expected[i]) return;
                }
                else if (candidate.courses.Length != 1 ||
                    TrialCatalog.SceneFor(candidate.vehicleId,candidate.courses[0]) == null) return;
                if (!Enum.IsDefined(typeof(DriveMode),candidate.mode)) return;
                current=candidate;
                // Idempotently recover a result after app termination during saving.
                PersistExamResult();
                if (current != null) Save();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NullReferenceException)
            {
                Debug.LogWarning("Could not load saved trial session: "+ex.Message);
            }
        }
    }
}
