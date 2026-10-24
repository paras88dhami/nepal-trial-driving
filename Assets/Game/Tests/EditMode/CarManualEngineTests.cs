using NepalTrial.Vehicles.Car;
using NUnit.Framework;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CarManualEngineTests
    {
        [Test]
        public void ReleasingClutchTooQuicklyAtStandstillStallsManualCar()
        {
            var engine=new CarManualEngineState();
            Assert.IsFalse(engine.Step(DriveGear.Drive,1f,0f,0f));
            Assert.IsTrue(engine.Step(DriveGear.Drive,0f,0f,0f));
            Assert.IsFalse(engine.Running);
            Assert.AreEqual(1,engine.StallCount);
            engine.RestartEngine();
            Assert.IsTrue(engine.Running);
            Assert.AreEqual(1,engine.StallCount);
        }

        [Test]
        public void AdequateThrottlePreventsStall()
        {
            var engine=new CarManualEngineState();
            engine.Step(DriveGear.Drive,1f,0f,0f);
            Assert.IsFalse(engine.Step(DriveGear.Drive,0f,.5f,0f));
            Assert.IsTrue(engine.Running);
        }

        [Test]
        public void NeutralAndExistingVehicleMotionAvoidStall()
        {
            var engine=new CarManualEngineState();
            engine.Step(DriveGear.Neutral,1f,0f,0f);
            Assert.IsFalse(engine.Step(DriveGear.Neutral,0f,0f,0f));
            engine.Step(DriveGear.Reverse,1f,0f,0f);
            Assert.IsFalse(engine.Step(DriveGear.Reverse,0f,0f,2f));
            Assert.AreEqual(0,engine.StallCount);
        }

        [Test]
        public void ResetRestoresEngineAndStallCounter()
        {
            var engine=new CarManualEngineState();
            engine.Step(DriveGear.Drive,1f,0f,0f);
            engine.Step(DriveGear.Drive,0f,0f,0f);
            engine.Reset();
            Assert.IsTrue(engine.Running);
            Assert.AreEqual(0,engine.StallCount);
        }
    }
}
