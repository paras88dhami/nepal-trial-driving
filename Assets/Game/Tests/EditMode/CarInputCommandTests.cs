using NepalTrial.Vehicles.Car;
using NUnit.Framework;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CarInputCommandTests
    {
        [Test]
        public void ClampsOutOfRangeInputs()
        {
            var cmd = new CarInputCommand { Steering = 3f, Throttle = -1f, Brake = 4f }.Clamped();
            Assert.AreEqual(1f, cmd.Steering);
            Assert.AreEqual(0f, cmd.Throttle);
            Assert.AreEqual(1f, cmd.Brake);
        }
    }
}
