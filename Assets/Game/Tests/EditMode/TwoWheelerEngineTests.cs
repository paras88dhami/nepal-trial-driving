using NUnit.Framework;
using NepalTrial.Vehicles.TwoWheeler;

namespace NepalTrial.Tests.EditMode
{
    public sealed class TwoWheelerEngineTests
    {
        [Test]
        public void MotorcycleCanStallAfterSustainedBadClutchRelease()
        {
            var engine=new TwoWheelerEngineState(TwoWheelerKind.Motorcycle,.8f);
            Assert.IsFalse(engine.Step(MotorcycleGear.First,0f,0f,0f,true,.4f));
            Assert.IsTrue(engine.Running);
            Assert.IsTrue(engine.Step(MotorcycleGear.First,0f,0f,0f,true,.4f));
            Assert.IsFalse(engine.Running);
            Assert.AreEqual(1,engine.StallCount);
            Assert.IsFalse(engine.Step(MotorcycleGear.First,0f,0f,0f,true,2f));
            engine.StartEngine();
            Assert.IsTrue(engine.Running);
            Assert.AreEqual(1,engine.StallCount);
        }

        [Test]
        public void ClutchThrottleAndNeutralPreventIdleStalls()
        {
            var engine=new TwoWheelerEngineState(TwoWheelerKind.Motorcycle,.5f);
            Assert.IsFalse(engine.Step(MotorcycleGear.First,1f,0f,0f,true,2f));
            Assert.IsFalse(engine.Step(MotorcycleGear.First,0f,.5f,0f,true,2f));
            Assert.IsFalse(engine.Step(MotorcycleGear.Neutral,0f,0f,0f,true,2f));
            Assert.IsTrue(engine.Running);
        }

        [Test]
        public void ScooterAutomaticCannotStallFromManualClutchBehavior()
        {
            var engine=new TwoWheelerEngineState(TwoWheelerKind.Scooter);
            Assert.IsFalse(engine.Step(MotorcycleGear.Neutral,0f,0f,0f,true,100f));
            Assert.AreEqual(0,engine.StallCount);
            Assert.IsTrue(engine.Running);
        }

        [Test]
        public void ResetClearsEngineStallHistory()
        {
            var engine=new TwoWheelerEngineState(TwoWheelerKind.Motorcycle,.25f);
            engine.Step(MotorcycleGear.First,0f,0f,0f,true,.3f);
            engine.Reset();
            Assert.IsTrue(engine.Running);
            Assert.AreEqual(0,engine.StallCount);
        }
    }
}
