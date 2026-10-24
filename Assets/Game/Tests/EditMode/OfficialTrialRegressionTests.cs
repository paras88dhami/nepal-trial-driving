using System.Collections.Generic;
using NepalTrial.Flow;
using NepalTrial.Trial;
using NUnit.Framework;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public sealed class OfficialTrialRegressionTests
    {
        [Test]
        public void OneBoundaryTouchIsCountedOnceUntilWheelReentersLane()
        {
            var monitor=new FigureEightContactMonitor(4.5f,1.5f,.1f,.2f);
            var left=new Vector2(-4.5f,0);
            var right=new Vector2(4.5f,0);
            Assert.AreEqual(FigureEightContactEvent.LineTouch,
                monitor.Sample(new[]{new Vector2(-4.5f,6f)},left,right));
            Assert.AreEqual(FigureEightContactEvent.None,
                monitor.Sample(new[]{new Vector2(-4.5f,6f)},left,right));
            Assert.AreEqual(FigureEightContactEvent.None,
                monitor.Sample(new[]{new Vector2(-4.5f,4.5f)},left,right));
            Assert.AreEqual(FigureEightContactEvent.LineTouch,
                monitor.Sample(new[]{new Vector2(-4.5f,6f)},left,right));
        }

        [Test]
        public void TireBeyondOuterLineIsImmediateDeparture()
        {
            var monitor=new FigureEightContactMonitor(4.5f,1.5f,.1f,.2f);
            var left=new Vector2(-4.5f,0);
            var right=new Vector2(4.5f,0);
            Assert.AreEqual(FigureEightContactEvent.Outside,
                monitor.Sample(new[]{new Vector2(-4.5f,6.25f)},left,right));
            Assert.AreEqual(FigureEightContactEvent.None,
                monitor.Sample(new[]{new Vector2(-4.5f,6.25f)},left,right));
        }

        [Test]
        public void BikeSecondFootDownFailsWithoutReachingZeroScore()
        {
            var run=new TrialRunCoordinator(new[]{"figure-eight"},
                new Dictionary<string,int>{{"foot-down",5}},100,70,120,
                new Dictionary<string,int>{{"foot-down",2}});
            run.Start();
            Assert.IsTrue(run.ReportViolation("foot-down"));
            Assert.AreEqual(95,run.Score);
            Assert.AreEqual(ExamStage.Running,run.State);
            Assert.IsTrue(run.ReportViolation("foot-down"));
            Assert.AreEqual(90,run.Score);
            Assert.AreEqual(ExamStage.Failed,run.State);
        }

        [Test]
        public void CarThirdLineTouchIsFatal()
        {
            var run=new TrialRunCoordinator(new[]{"figure-eight"},
                new Dictionary<string,int>{{"figure-eight-line-touch",5}},100,70,120,
                new Dictionary<string,int>{{"figure-eight-line-touch",3}});
            run.Start();
            run.ReportViolation("figure-eight-line-touch");
            run.ReportViolation("figure-eight-line-touch");
            Assert.AreEqual(90,run.Score);
            Assert.AreEqual(ExamStage.Running,run.State);
            run.ReportViolation("figure-eight-line-touch");
            Assert.AreEqual(ExamStage.Failed,run.State);
        }

        [Test]
        public void RollbackOnAscentFailsButPlannedDescentDoesNot()
        {
            var hill=new HillStartValidator(.1524f,90f);
            hill.Begin(0f);
            Assert.AreEqual(CourseEventKind.None,hill.Sample(10,1,false,false).Kind);
            Assert.AreEqual(CourseEventKind.None,hill.Sample(2,1,false,true).Kind);
            Assert.AreEqual(CourseEventKind.Completed,hill.Sample(0,1,true,true).Kind);
            hill.Begin(0f);
            hill.Sample(1f,1f,false,false);
            Assert.AreEqual("hill-rollback",hill.Sample(.7f,1f,false,false).Code);
        }

        [Test]
        public void CarAndTwoWheelerFullExamsHaveSevenStagesAndHundredMarks()
        {
            foreach(string vehicle in TrialCatalog.Vehicles)
            {
                bool bike=TrialCatalog.IsTwoWheeler(vehicle);
                var stages=TrialCatalog.ConnectedExamStagesFor(vehicle);
                Assert.AreEqual(7,stages.Length);
                int total=0;
                foreach(var stage in stages)
                    total+=NepalTrialScoringCatalog.MaximumMarks(stage,bike);
                Assert.AreEqual(100,total);
            }
        }

        [Test]
        public void MissingTrafficLightCostsFifteenButDoesNotDisqualify()
        {
            var penalties=new Dictionary<string,int>{{"signal-red",15}};
            var run=new TrialRunCoordinator(new[]{"traffic-light","behavior"},penalties,100,70,90);
            run.Start();
            Assert.IsTrue(run.ReportViolation("signal-red"));
            Assert.AreEqual(85,run.Score);
            Assert.AreEqual(ExamStage.Running,run.State);
            Assert.IsTrue(run.AdvanceStage("traffic-light"));
        }
    }
}
