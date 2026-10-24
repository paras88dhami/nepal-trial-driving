using NUnit.Framework;
using NepalTrial.Input;
using NepalTrial.Vehicles.Car;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CarGearSelectionTests
    {
        [Test]
        public void ExplicitGearSelectionPreservesParkNeutralAndDrive()
        {
            var command = new CarInputCommand
            {
                Steering = 2f,
                Throttle = 2f,
                HasRequestedGear = true,
                RequestedGear = DriveGear.Neutral
            }.Clamped();
            Assert.IsTrue(command.HasRequestedGear);
            Assert.AreEqual(DriveGear.Neutral, command.RequestedGear);
            Assert.AreEqual(1f, command.Steering);
            Assert.AreEqual(1f, command.Throttle);
        }

        [Test]
        public void TouchGearSelectionAndReset()
        {
            GameObject root = new GameObject("InputGearTest");
            try
            {
                var input = root.AddComponent<PrototypeCarInput>();
                input.SelectGear(DriveGear.Park);
                Assert.AreEqual(DriveGear.Park, input.SelectedGear);
                input.SelectGear(DriveGear.Neutral);
                Assert.AreEqual(DriveGear.Neutral, input.SelectedGear);
                input.ToggleReverse();
                Assert.AreEqual(DriveGear.Reverse, input.SelectedGear);
                input.ToggleReverse();
                Assert.AreEqual(DriveGear.Drive, input.SelectedGear);
                input.ResetCar();
                Assert.AreEqual(DriveGear.Drive, input.SelectedGear);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GearChangesRejectMovementAndRequireBrake()
        {
            var gearbox = new TransmissionState();
            Assert.IsFalse(gearbox.TrySelect(DriveGear.Reverse, 2f, true));
            Assert.IsFalse(gearbox.TrySelect(DriveGear.Reverse, 0f, false));
            Assert.IsTrue(gearbox.TrySelect(DriveGear.Drive, 0f, true));
            Assert.IsTrue(gearbox.TrySelect(DriveGear.Neutral, 5f, false));
            Assert.AreEqual(DriveGear.Neutral, gearbox.Gear);
            Assert.IsTrue(gearbox.TrySelect(DriveGear.Park, 0f, true));
        }
    }
}
