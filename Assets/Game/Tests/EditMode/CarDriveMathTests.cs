using NUnit.Framework;
using NepalTrial.Vehicles.Car;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CarDriveMathTests
    {
        [Test]
        public void SteeringIsReducedAtSpeed()
        {
            Assert.AreEqual(30f, CarDriveMath.SteeringAngle(1f, 0f, 30f, .5f, 40f), .0001f);
            Assert.AreEqual(15f, CarDriveMath.SteeringAngle(1f, 40f, 30f, .5f, 40f), .0001f);
            Assert.AreEqual(-15f, CarDriveMath.SteeringAngle(-1f, 80f, 30f, .5f, 40f), .0001f);
        }

        [Test]
        public void TorqueIsBlockedByBrakeHandbrakeOrSpeedLimit()
        {
            Assert.AreEqual(0.8f, CarDriveMath.DriveTorque(.8f, 0f, 35f, 0f, false));
            Assert.AreEqual(-.5f, CarDriveMath.DriveTorque(-.5f, 0f, 35f, 0f, false));
            Assert.AreEqual(0f, CarDriveMath.DriveTorque(1f, 0f, 35f, .5f, false));
            Assert.AreEqual(0f, CarDriveMath.DriveTorque(1f, 0f, 35f, 0f, true));
            Assert.AreEqual(0f, CarDriveMath.DriveTorque(1f, 35f, 35f, 0f, false));
        }

        [Test]
        public void ParkHoldsAllWheelsWhileHandbrakeOnlyHoldsRear()
        {
            Assert.AreEqual(3500f, CarDriveMath.WheelBrakeTorque(0f, 3500f, true, false, false));
            Assert.AreEqual(3500f, CarDriveMath.WheelBrakeTorque(0f, 3500f, true, false, true));
            Assert.AreEqual(0f, CarDriveMath.WheelBrakeTorque(0f, 3500f, false, true, false));
            Assert.AreEqual(3500f, CarDriveMath.WheelBrakeTorque(0f, 3500f, false, true, true));
        }

        [Test]
        public void BrakeTorqueIsClamped()
        {
            Assert.AreEqual(0f, CarDriveMath.BrakeTorque(-1f, 2400f));
            Assert.AreEqual(1200f, CarDriveMath.BrakeTorque(.5f, 2400f));
            Assert.AreEqual(2400f, CarDriveMath.BrakeTorque(2f, 2400f));
        }
    }
}
