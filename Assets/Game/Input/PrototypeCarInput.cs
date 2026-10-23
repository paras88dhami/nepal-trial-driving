using NepalTrial.Vehicles.Car;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NepalTrial.Input
{
    /// <summary>Touch commands with keyboard fallback for desktop testing.</summary>
    public sealed class PrototypeCarInput : MonoBehaviour
    {
        [SerializeField] private CarDriveController car;
        private float steering, throttle, brake;
        private bool reverse, handbrake;
        private bool touchActive;

        public void SetSteering(float value) { touchActive = true; steering = Mathf.Clamp(value, -1f, 1f); }
        public void SetThrottle(float value) { touchActive = true; throttle = Mathf.Clamp01(value); }
        public void SetBrake(float value) { touchActive = true; brake = Mathf.Clamp01(value); }
        public void SetReverse(bool value) { touchActive = true; reverse = value; }
        public void SetHandbrake(bool value) { touchActive = true; handbrake = value; }
        public void UseKeyboard() { touchActive = false; steering = throttle = brake = 0f; handbrake = reverse = false; }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (!touchActive)
            {
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
