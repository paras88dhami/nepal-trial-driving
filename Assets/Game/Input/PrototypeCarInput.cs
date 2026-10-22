using NepalTrial.Vehicles.Car;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NepalTrial.Input
{
    /// <summary>Keyboard fallback for desktop testing. Mobile UI feeds the same controller.</summary>
    public sealed class PrototypeCarInput : MonoBehaviour
    {
        [SerializeField] private CarDriveController car;
        private float steering, throttle, brake;
        private bool reverse, handbrake;

        public void SetSteering(float value) => steering = Mathf.Clamp(value, -1f, 1f);
        public void SetThrottle(float value) => throttle = Mathf.Clamp01(value);
        public void SetBrake(float value) => brake = Mathf.Clamp01(value);
        public void SetReverse(bool value) => reverse = value;
        public void SetHandbrake(bool value) => handbrake = value;

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                steering = (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? -1f : 0f)
                    + (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f);
                throttle = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f;
                brake = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f;
                handbrake = keyboard.spaceKey.isPressed;
                reverse = keyboard.rKey.isPressed;
            }
#endif
            if (car != null)
                car.SetCommand(new CarInputCommand {
                    Steering = steering, Throttle = throttle, Brake = brake,
                    Reverse = reverse, Handbrake = handbrake
                });
        }
    }
}
