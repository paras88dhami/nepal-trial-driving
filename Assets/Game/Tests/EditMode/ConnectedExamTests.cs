using System.Collections.Generic;
using NepalTrial.Flow;
using NepalTrial.Progress;
using NepalTrial.Trial;
using NUnit.Framework;

namespace NepalTrial.Tests.EditMode
{
    public sealed class ConnectedExamTests
    {
        [Test]
        public void SeparatePracticeAndConnectedExamCourseCatalogs()
        {
            Assert.AreEqual(6,TrialCatalog.CoursesFor(TrialCatalog.StarterCar).Length);
            Assert.AreEqual(7,TrialCatalog.ConnectedExamStagesFor(TrialCatalog.StarterCar).Length);
            Assert.AreEqual("behavior",TrialCatalog.ConnectedExamStagesFor(TrialCatalog.StarterCar)[6]);
            Assert.AreEqual(7,TrialCatalog.ConnectedExamStagesFor(TrialCatalog.StarterScooter).Length);
            Assert.AreEqual("NepalTrial_FullCar",
                TrialCatalog.ConnectedExamSceneFor(TrialCatalog.StarterCar));
            Assert.AreEqual("NepalTrial_FullMotorcycle",
                TrialCatalog.ConnectedExamSceneFor(TrialCatalog.StarterMotorcycle));
        }

        [Test]
        public void ASingleConnectedExamResultIsRecordedOnce()
        {
            var session=TrialSessionRules.CreateConnectedExam(TrialCatalog.StarterCar);
            Assert.IsTrue(session.connectedExam);
            Assert.IsTrue(TrialSessionRules.ReportStageResult(session,true,75));
            Assert.IsTrue(session.ended);
            Assert.IsTrue(session.passed);
            Assert.AreEqual(75,session.finalScore);
            var progress=new LocalProgressData();
            Assert.IsTrue(TrialSessionRules.RecordIfNeeded(session,progress));
            Assert.IsFalse(TrialSessionRules.RecordIfNeeded(session,progress));
            Assert.AreEqual(1,progress.records[0].attempts);
        }

        [Test]
        public void ConnectedExamPassesOnlyWithSeventyOrMoreAndNoFatal()
        {
            var passed=TrialSessionRules.CreateConnectedExam(TrialCatalog.StarterScooter);
            TrialSessionRules.ReportStageResult(passed,true,70);
            Assert.IsTrue(passed.passed);

            var below=TrialSessionRules.CreateConnectedExam(TrialCatalog.StarterScooter);
            TrialSessionRules.ReportStageResult(below,true,69);
            Assert.IsFalse(below.passed);

            var fatal=TrialSessionRules.CreateConnectedExam(TrialCatalog.StarterCar);
            TrialSessionRules.ReportStageResult(fatal,false,100);
            Assert.IsFalse(fatal.passed);
        }

        [Test]
        public void FatalViolationCausesFailureEvenWithOneHundredMarks()
        {
            var run=new TrialRunCoordinator(new[] {"figure-eight","behavior"},
                new Dictionary<string,int> { {"boundary",0}, {"line",5} },
                100,70,200f,new Dictionary<string,int> { {"boundary",1}, {"line",3} });
            Assert.IsTrue(run.Start());
            Assert.IsTrue(run.ReportViolation("boundary"));
            Assert.AreEqual(100,run.Score);
            Assert.AreEqual(ExamStage.Failed,run.State);
        }

        [Test]
        public void RepeatedLineTouchFailsOnConfiguredOccurrence()
        {
            var run=new TrialRunCoordinator(new[] {"figure-eight"},
                new Dictionary<string,int> { {"line",5} },
                100,70,200f,new Dictionary<string,int> { {"line",3} });
            run.Start();
            Assert.IsTrue(run.ReportViolation("line"));
            Assert.IsTrue(run.ReportViolation("line"));
            Assert.AreEqual(90,run.Score);
            Assert.AreEqual(ExamStage.Running,run.State);
            Assert.IsTrue(run.ReportViolation("line"));
            Assert.AreEqual(ExamStage.Failed,run.State);
            Assert.AreEqual(3,run.CountViolation("line"));
            run.Reset(200f);
            Assert.AreEqual(0,run.CountViolation("line"));
            Assert.AreEqual(ExamStage.Ready,run.State);
        }

        [Test]
        public void ExamMarkBreakdownTotalsOneHundredForBothCategories()
        {
            int car=0, bike=0;
            foreach(string stage in TrialCatalog.ConnectedExamStagesFor(TrialCatalog.StarterCar))
                car+=NepalTrialScoringCatalog.MaximumMarks(stage,false);
            foreach(string stage in TrialCatalog.ConnectedExamStagesFor(TrialCatalog.StarterMotorcycle))
                bike+=NepalTrialScoringCatalog.MaximumMarks(stage,true);
            Assert.AreEqual(100,car);
            Assert.AreEqual(100,bike);
        }

        [Test]
        public void CatalogKeepsFatalAndScoredViolationsSeparate()
        {
            var rules=NepalTrialScoringCatalog.GetRules(false);
            bool boundaryFatal=false, trafficCosts15=false;
            foreach(var rule in rules)
            {
                if (rule.Code=="figure-eight-boundary")
                    boundaryFatal=rule.FailOnOccurrence==1 && rule.Points==0;
                if (rule.Code=="signal-red")
                    trafficCosts15=rule.Points==15 && rule.FailOnOccurrence==0;
            }
            Assert.IsTrue(boundaryFatal);
            Assert.IsTrue(trafficCosts15);
        }
    }
}
