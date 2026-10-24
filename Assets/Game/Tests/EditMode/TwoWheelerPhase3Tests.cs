using NUnit.Framework;
using NepalTrial.Trial;
using NepalTrial.Vehicles.TwoWheeler;

namespace NepalTrial.Tests.EditMode
{
    public sealed class TwoWheelerPhase3Tests
    {
        [Test]
        public void MotorcycleDoesNotPowerNeutralOrWithClutchDisengaged()
        {
            var gearbox = new TwoWheelerTransmission(TwoWheelerKind.Motorcycle);
            Assert.IsFalse(gearbox.CanDrive(false));
            Assert.IsTrue(gearbox.SelectGear(MotorcycleGear.First,true,0));
            Assert.IsFalse(gearbox.CanDrive(true));
            Assert.IsTrue(gearbox.CanDrive(false));
            Assert.AreEqual(0f,TwoWheelerDriveMath.MotorTorque(1f,0f,40f,150f,
                true,gearbox.CanDrive(true),0f,0f));
            Assert.Greater(TwoWheelerDriveMath.MotorTorque(1f,0f,40f,150f,
                true,gearbox.CanDrive(false),0f,0f),0f);
        }

        [Test]
        public void ScooterIsAutomaticAndDoesNotShiftManually()
        {
            var scooter = new TwoWheelerTransmission(TwoWheelerKind.Scooter);
            Assert.IsTrue(scooter.CanDrive(false));
            Assert.IsFalse(scooter.SelectGear(MotorcycleGear.First,true,0));
        }

        [Test]
        public void SpeedLimiterBrakesAndSteeringAreClamped()
        {
            Assert.AreEqual(0f,TwoWheelerDriveMath.MotorTorque(1f,41f,40f,150f,true,true,0,0));
            Assert.AreEqual(0f,TwoWheelerDriveMath.MotorTorque(1f,0f,40f,150f,true,true,1f,0));
            Assert.AreEqual(250f,TwoWheelerDriveMath.BrakeTorque(2f,250f));
            Assert.AreEqual(12f,TwoWheelerDriveMath.SteeringAngle(1f,40f,30f,.4f,40f),.001f);
            Assert.IsFalse(TwoWheelerDriveMath.BothWheelsGrounded(true,false));
        }

        [Test]
        public void RampRequiresStopWithinZone()
        {
            var ramp=new TwoWheelerRampValidator(.5f,.8f,20f);
            ramp.Begin(0f);
            Assert.AreEqual(CourseEventKind.None,ramp.Sample(1f,true,0f,false,false,.4f).Kind);
            Assert.AreEqual(CourseEventKind.None,ramp.Sample(1f,true,0f,false,false,.4f).Kind);
            Assert.IsTrue(ramp.HasCompletedStop);
            Assert.AreEqual("ramp-complete",ramp.Sample(5f,false,1f,false,true,.1f).Code);
        }

        [Test]
        public void RampFailsIfNoStopOrRollingBack()
        {
            var ramp=new TwoWheelerRampValidator(.4f,.8f,20f);
            ramp.Begin(0f);
            Assert.AreEqual("ramp-no-stop",ramp.Sample(2f,false,2f,false,true,1f).Code);
            ramp.Begin(0f);
            ramp.Sample(3f,false,3f,false,false,1f);
            Assert.AreEqual("ramp-rollback",ramp.Sample(2.5f,false,2f,false,false,1f).Code);
        }

        [Test]
        public void BehavioralChecksMustBeCompleted()
        {
            var rule=new TwoWheelerBehavioralValidator();
            Assert.AreEqual("behavior-incomplete",rule.TryComplete().Code);
            rule.SetHelmet(true); rule.SetMirrors(true);
            rule.SetBrakes(true); rule.SetTires(true);
            Assert.IsTrue(rule.Complete);
            Assert.AreEqual(CourseEventKind.Completed,rule.TryComplete().Kind);
            rule.Reset();
            Assert.IsFalse(rule.Complete);
        }

        [Test]
        public void AllSevenTrialStageIdsCanCompleteIndividually()
        {
            foreach (var stage in new[]{"figure-eight","narrow-plank","u-turn",
                "traffic-light","speed-breaker","ramp","behavior"})
            {
                var runtime=new TrialRunCoordinator(new[]{stage},null,100,70,180f);
                Assert.IsTrue(runtime.Start());
                Assert.IsTrue(runtime.AdvanceStage(stage),stage);
                Assert.AreEqual(ExamStage.Passed,runtime.State,stage);
            }
        }
    }
}
