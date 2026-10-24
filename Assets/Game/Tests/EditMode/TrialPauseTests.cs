using System.Collections.Generic;
using NUnit.Framework;
using NepalTrial.Trial;

namespace NepalTrial.Tests.EditMode
{
    public sealed class TrialPauseTests
    {
        private static TrialRunCoordinator Create() => new TrialRunCoordinator(
            new[] { "start", "finish" },
            new Dictionary<string, int> { { "line", 5 } }, 100, 70, 20f);

        [Test]
        public void PauseStopsTimerPenaltiesAndStageProgress()
        {
            var session = Create();
            Assert.IsFalse(session.SetPaused(true));
            Assert.IsTrue(session.Start());
            session.Tick(5f);
            Assert.IsTrue(session.SetPaused(true));
            Assert.IsTrue(session.IsPaused);
            session.Tick(100f);
            Assert.AreEqual(15f, session.RemainingSeconds, .001f);
            Assert.IsFalse(session.ReportViolation("line"));
            Assert.IsFalse(session.AdvanceStage("start"));
            Assert.IsTrue(session.SetPaused(false));
            Assert.IsTrue(session.ReportViolation("line"));
            Assert.AreEqual(95, session.Score);
            Assert.IsTrue(session.AdvanceStage("start"));
        }

        [Test]
        public void RestartClearsPauseAndRestoresScore()
        {
            var session = Create();
            session.Start();
            session.ReportViolation("line");
            session.SetPaused(true);
            session.Reset(20f);
            Assert.IsFalse(session.IsPaused);
            Assert.AreEqual(100, session.Score);
            Assert.AreEqual(ExamStage.Ready, session.State);
            Assert.IsTrue(session.Start());
        }

        [Test]
        public void PauseCannotReactivateCompletedAttempt()
        {
            var session = Create();
            session.Start();
            session.AdvanceStage("start");
            session.AdvanceStage("finish");
            Assert.AreEqual(ExamStage.Passed, session.State);
            Assert.IsFalse(session.SetPaused(true));
        }
    }
}
