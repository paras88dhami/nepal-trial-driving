using System.Collections.Generic;
using NUnit.Framework;
using NepalTrial.Trial;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CarTrialPhase2Tests
    {
        [Test]
        public void UTurnCannotBeCheatedByOscillatingSteering()
        {
            var rule = new UTurnValidator(140f,30f);
            rule.Begin();
            rule.Sample(90f,1f,false,false);
            Assert.AreEqual("u-turn-incomplete",rule.Sample(-90f,1f,false,true).Code);
        }

        [Test]
        public void HillRollbackCountsFromHighestReachedPosition()
        {
            var rule = new HillStartValidator(.5f,20f);
            rule.Begin(0f);
            Assert.AreEqual(CourseEventKind.None,rule.Sample(3f,1f,false).Kind);
            Assert.AreEqual("hill-rollback",rule.Sample(2.4f,1f,false).Code);
        }

        [Test]
        public void AllSixCarCoursesCanIndependentlyComplete()
        {
            foreach (string id in new[] { "figure-eight","u-turn","hill-start",
                "traffic-light","speed-breaker","garage" })
            {
                var rules = new Dictionary<string,int> { { "obstacle-hit",35 } };
                var session = new TrialRunCoordinator(new[] { id },rules,100,70,180);
                Assert.IsTrue(session.Start(),id);
                Assert.IsFalse(session.AdvanceStage("wrong-stage"),id);
                Assert.IsTrue(session.AdvanceStage(id),id);
                Assert.AreEqual(ExamStage.Passed,session.State,id);
            }
        }

        [Test]
        public void FatalCarTrialPenaltyStopsFurtherProgress()
        {
            var session = new TrialRunCoordinator(new[] { "garage" },
                new Dictionary<string,int> { { "obstacle-hit",35 } },100,70,180);
            session.Start();
            Assert.IsTrue(session.ReportViolation("obstacle-hit"));
            Assert.AreEqual(ExamStage.Failed,session.State);
            Assert.IsFalse(session.AdvanceStage("garage"));
        }
    }
}
