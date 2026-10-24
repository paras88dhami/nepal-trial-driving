using System.Collections;
using System.Reflection;
using NepalTrial.Trial;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NepalTrial.Tests.PlayMode
{
    public sealed class TrialRuntimePlayModeTests
    {
        private GameObject root;
        private TrialGameplayRuntime trial;
        private static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root = new GameObject("Phase5RuntimeTest");
            root.SetActive(false);
            trial = root.AddComponent<TrialGameplayRuntime>();
            typeof(TrialGameplayRuntime).GetField("orderedStages", Hidden)
                .SetValue(trial, new[] { "first", "second" });
            typeof(TrialGameplayRuntime).GetField("durationSeconds", Hidden)
                .SetValue(trial, 30f);
            root.SetActive(true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PausingStopsCountdownAndRejectsStageProgression()
        {
            Assert.AreEqual(ExamStage.Running, trial.State);
            Assert.IsTrue(trial.SetPaused(true));
            float remaining = trial.RemainingSeconds;
            yield return new WaitForSeconds(.1f);
            Assert.AreEqual(remaining,trial.RemainingSeconds,.02f);
            Assert.IsFalse(trial.ReportStage("first"));
            Assert.IsTrue(trial.SetPaused(false));
            Assert.IsTrue(trial.ReportStage("first"));
            Assert.AreEqual("second",trial.CurrentStage);
        }

        [UnityTest]
        public IEnumerator RestartResetsProgressAndCountdown()
        {
            Assert.IsTrue(trial.ReportStage("first"));
            Assert.AreEqual("second",trial.CurrentStage);
            trial.RestartAttempt();
            yield return null;
            Assert.AreEqual(ExamStage.Running,trial.State);
            Assert.AreEqual("first",trial.CurrentStage);
            Assert.That(trial.RemainingSeconds,Is.InRange(29f,30f));
        }
    }
}
