using System.Collections.Generic;
using NUnit.Framework;
using NepalTrial.Trial;

namespace NepalTrial.Tests.EditMode
{
    public class TrialRunCoordinatorTests
    {
        private TrialRunCoordinator NewRun() => new TrialRunCoordinator(
            new[] { "eight", "uturn", "hill" }, new Dictionary<string, int> { { "line", 5 }, { "rollback", 20 } }, 100, 70, 30f);

        [Test] public void ValidSequenceCompletesAndPasses()
        {
            var run = NewRun();
            Assert.IsFalse(run.AdvanceStage("eight"));
            Assert.IsTrue(run.Start());
            Assert.IsFalse(run.AdvanceStage("uturn"));
            Assert.IsTrue(run.AdvanceStage("eight"));
            Assert.IsTrue(run.ReportViolation("line"));
            Assert.AreEqual(95, run.Score);
            Assert.IsTrue(run.AdvanceStage("uturn"));
            Assert.IsTrue(run.AdvanceStage("hill"));
            Assert.AreEqual(ExamStage.Passed, run.State);
            Assert.IsFalse(run.ReportViolation("line"));
        }

        [Test] public void TimerExpiresAndStopsProgress()
        {
            var run = NewRun();
            run.Start(); run.Tick(31f);
            Assert.AreEqual(ExamStage.Failed, run.State);
            Assert.IsFalse(run.AdvanceStage("eight"));
        }

        [Test] public void ResetRestoresNewAttempt()
        {
            var run = NewRun();
            run.Start(); run.ReportViolation("rollback"); run.Cancel();
            Assert.AreEqual(ExamStage.Cancelled, run.State);
            run.Reset(60);
            Assert.AreEqual(100, run.Score);
            Assert.AreEqual(0, run.CurrentStageIndex);
            Assert.AreEqual(ExamStage.Ready, run.State);
            Assert.IsTrue(run.Start());
        }

        [Test] public void UnknownPenaltyDoesNotChangeScore()
        {
            var run = NewRun();
            run.Start();
            Assert.IsFalse(run.ReportViolation("undefined"));
            Assert.AreEqual(100, run.Score);
        }
    }
}
