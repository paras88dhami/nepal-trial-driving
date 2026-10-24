using System.Collections.Generic;
using NUnit.Framework;
using NepalTrial.Trial;
using NepalTrial.Progress;

namespace NepalTrial.Tests.EditMode
{
    public sealed class TrialIntegrationScenariosTests
    {
        [Test]
        public void OrderedExamWithPenaltyAndSavedResult()
        {
            var controller = new TrialRunCoordinator(
                new[] { "figure-eight", "u-turn", "garage" },
                new Dictionary<string, int> { { "line", 10 } }, 100, 70, 120f);
            Assert.IsTrue(controller.Start());
            Assert.IsFalse(controller.AdvanceStage("garage"));
            Assert.IsTrue(controller.ReportViolation("line"));
            Assert.AreEqual(90, controller.Score);
            Assert.IsTrue(controller.AdvanceStage("figure-eight"));
            controller.Tick(5f);
            Assert.IsTrue(controller.AdvanceStage("u-turn"));
            Assert.IsTrue(controller.AdvanceStage("garage"));
            Assert.AreEqual(ExamStage.Passed, controller.State);

            var progress = new LocalProgressData();
            var result = ProgressRules.RecordAttempt(progress, "car-trial", controller.Score,
                controller.State == ExamStage.Passed);
            Assert.AreEqual(1, result.attempts);
            Assert.AreEqual(1, result.completions);
            Assert.AreEqual(90, result.bestScore);
        }

        [Test]
        public void TooManyViolationsStopsCourse()
        {
            var controller = new TrialRunCoordinator(new[] { "eight", "hill" },
                new Dictionary<string, int> { { "rollback", 40 } }, 100, 70, 100f);
            controller.Start();
            Assert.IsTrue(controller.ReportViolation("rollback"));
            Assert.AreEqual(ExamStage.Failed, controller.State);
            Assert.IsFalse(controller.AdvanceStage("eight"));
        }

        [Test]
        public void RestartAfterTimeoutAllowsNewAttempt()
        {
            var controller = new TrialRunCoordinator(new[] { "eight" },
                new Dictionary<string, int>(), 100, 70, 5f);
            controller.Start();
            controller.Tick(6f);
            Assert.AreEqual(ExamStage.Failed, controller.State);
            controller.Reset(5f);
            Assert.IsTrue(controller.Start());
            Assert.AreEqual(ExamStage.Running, controller.State);
        }
    }
}
