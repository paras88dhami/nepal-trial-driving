using NUnit.Framework;
using NepalTrial.Input;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CarMobileInputTests
    {
        [Test]
        public void ReverseTogglesAndResetClearsTouchInput()
        {
            var gameObject = new GameObject("CarMobileInputTests");
            try
            {
                var input = gameObject.AddComponent<PrototypeCarInput>();
                Assert.IsFalse(input.ReverseSelected);
                input.ToggleReverse();
                Assert.IsTrue(input.ReverseSelected);
                input.SetHandbrake(true);
                Assert.IsTrue(input.HandbrakePressed);
                input.ToggleReverse();
                Assert.IsFalse(input.ReverseSelected);
                input.ToggleReverse();
                input.ResetCar();
                Assert.IsFalse(input.ReverseSelected);
                Assert.IsFalse(input.HandbrakePressed);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
