using NUnit.Framework;
using NepalTrial.Vehicles.TwoWheeler;

namespace NepalTrial.Tests.EditMode
{
    public class TwoWheelerLogicTests
    {
        [Test] public void LeanRespondsAndResets()
        {
            var dynamics = new TwoWheelerDynamics(TwoWheelerKind.Motorcycle);
            dynamics.Step(1, 0, 1, 10, 1, 30, 10);
            Assert.AreEqual(-10f, dynamics.LeanDegrees, .001f);
            Assert.AreEqual(1f, dynamics.MotorResponse);
            dynamics.Step(1, 1, 1, 10, 1, 30, 10);
            Assert.AreEqual(0f, dynamics.MotorResponse);
            dynamics.Reset();
            Assert.AreEqual(0f, dynamics.LeanDegrees);
        }
        [Test] public void MotorcycleNeedsClutchToShift()
        {
            var transmission = new TwoWheelerTransmission(TwoWheelerKind.Motorcycle);
            Assert.IsFalse(transmission.SelectGear(MotorcycleGear.First, false, 0));
            Assert.IsTrue(transmission.SelectGear(MotorcycleGear.First, true, 0));
            Assert.IsFalse(transmission.CanDrive(true));
            Assert.IsTrue(transmission.CanDrive(false));
        }
        [Test] public void ScooterUsesAutomaticDrive()
        {
            var transmission = new TwoWheelerTransmission(TwoWheelerKind.Scooter);
            Assert.IsFalse(transmission.SelectGear(MotorcycleGear.First, true, 0));
            Assert.IsTrue(transmission.CanDrive(false));
        }
    }
}
