using NUnit.Framework;
using NepalTrial.Trial;
using NepalTrial.Vehicles.TwoWheeler;
using System.Collections.Generic;

namespace NepalTrial.Tests.EditMode
{
    public sealed class TwoWheelerPhase3IntegrationTests
    {
        [Test]
        public void TurnIndicatorMustMatchActualTurnDirection()
        {
            Assert.IsTrue(TwoWheelerIndicatorRules.HasCorrectTurnSignal(170f,false,true));
            Assert.IsFalse(TwoWheelerIndicatorRules.HasCorrectTurnSignal(170f,true,false));
            Assert.IsTrue(TwoWheelerIndicatorRules.HasCorrectTurnSignal(-170f,true,false));
            Assert.IsFalse(TwoWheelerIndicatorRules.HasCorrectTurnSignal(-170f,false,true));
        }

        [Test]
        public void MotorbikeControlsClampAndKeepBrakeAxesIndependent()
        {
            var control=new TwoWheelerControlCommand{
                Steering=4f,Throttle=3f,FrontBrake=-1f,RearBrake=.75f,Clutch=2f
            }.Clamped();
            Assert.AreEqual(1f,control.Steering);
            Assert.AreEqual(1f,control.Throttle);
            Assert.AreEqual(0f,control.FrontBrake);
            Assert.AreEqual(.75f,control.RearBrake);
            Assert.AreEqual(1f,control.Clutch);
        }

        [Test]
        public void RampCanDescendAfterSuccessfulAscentWithoutFalseRollback()
        {
            var ramp=new TwoWheelerRampValidator(.45f,.5f,60f);
            ramp.Begin(0f);
            ramp.Sample(2f,true,0f,false,false,.6f);
            Assert.IsTrue(ramp.HasCompletedStop);
            Assert.AreEqual(CourseEventKind.None,
                ramp.Sample(8f,false,3f,false,false,1f,true).Kind);
            Assert.AreEqual("ramp-complete",
                ramp.Sample(1f,false,3f,false,true,1f,true).Code);
        }

        [Test]
        public void SevenStageTrialCompletesInOrderAndSavesScoreInDomain()
        {
            string[] stages={"figure-eight","narrow-plank","u-turn","traffic-light",
                "speed-breaker","ramp","behavior"};
            var penalties=new Dictionary<string,int>{
                {"figure-eight-boundary",5},{"u-turn-no-indicator",10}
            };
            var trial=new TrialRunCoordinator(stages,penalties,100,70,300f);
            Assert.IsTrue(trial.Start());
            Assert.IsFalse(trial.AdvanceStage("ramp"));
            Assert.IsTrue(trial.ReportViolation("figure-eight-boundary"));
            foreach (string stage in stages)
                Assert.IsTrue(trial.AdvanceStage(stage),stage);
            Assert.AreEqual(ExamStage.Passed,trial.State);
            Assert.AreEqual(95,trial.Score);
        }
    }
}
