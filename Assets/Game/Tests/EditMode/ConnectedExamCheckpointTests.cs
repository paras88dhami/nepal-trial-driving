using System.Collections.Generic;
using NepalTrial.Flow;
using NepalTrial.Trial;
using NUnit.Framework;

namespace NepalTrial.Tests.EditMode
{
    public sealed class ConnectedExamCheckpointTests
    {
        [Test]
        public void RestoreContinuesAtNextStationWithPriorDeductions()
        {
            var stages=TrialCatalog.ConnectedExamStagesFor(TrialCatalog.StarterCar);
            var rules=new Dictionary<string,int>{{"signal-red",15}};
            var run=new TrialRunCoordinator(stages,rules,100,70,1200f);
            Assert.IsTrue(run.Start());
            Assert.IsTrue(run.AdvanceStage("figure-eight"));
            Assert.IsTrue(run.AdvanceStage("speed-breaker"));
            int index=run.CurrentStageIndex;
            run.Tick(45f);
            var recovered=new TrialRunCoordinator(stages,rules,100,70,1200f);
            recovered.RestoreCheckpoint(index,85,1,run.RemainingSeconds);
            Assert.AreEqual("u-turn",recovered.CurrentStage);
            Assert.AreEqual(85,recovered.Score);
            Assert.AreEqual(1,recovered.ViolationCount);
            Assert.That(recovered.RemainingSeconds,Is.InRange(1154.8f,1155.2f));
            Assert.AreEqual(ExamStage.Running,recovered.State);
            Assert.IsTrue(recovered.AdvanceStage("u-turn"));
        }

        [Test]
        public void InvalidCheckpointCannotRestore()
        {
            var run=new TrialRunCoordinator(new[]{"figure-eight","behavior"},
                new Dictionary<string,int>(),100,70,60f);
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                run.RestoreCheckpoint(-1,90,0,60f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                run.RestoreCheckpoint(2,90,0,60f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                run.RestoreCheckpoint(1,90,0,0f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                run.RestoreCheckpoint(1,69,0,60f));
        }

        [Test]
        public void ConnectedExamResultRecordsOnlyOnceAfterResume()
        {
            TrialPlaySession session=TrialSessionRules.CreateConnectedExam(
                TrialCatalog.StarterMotorcycle);
            session.stageIndex=4;
            session.completedPenaltyPoints=15;
            session.checkpointSeconds=320f;
            session.checkpointViolations=2;
            Assert.IsFalse(session.ended);
            Assert.AreEqual("speed-breaker",session.CurrentCourse);
            Assert.IsTrue(TrialSessionRules.ReportStageResult(session,true,75));
            Assert.IsTrue(session.ended);
            Assert.IsTrue(session.passed);
            Assert.AreEqual(75,session.finalScore);
        }

        [Test]
        public void SevenStageCatalogStillHasOneBehaviorStage()
        {
            foreach(var id in TrialCatalog.Vehicles)
            {
                var stages=TrialCatalog.ConnectedExamStagesFor(id);
                Assert.AreEqual(7,stages.Length);
                Assert.AreEqual("behavior",stages[6]);
            }
        }
    }
}
