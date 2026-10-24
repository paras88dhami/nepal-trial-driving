using NepalTrial.Vehicles.Car;
using NepalTrial.Settings;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NepalTrial.Input
{
    /// <summary>Independent touch buttons allow simultaneous steering, throttle and brake.</summary>
    public sealed class PrototypeCarInput : MonoBehaviour
    {
        [SerializeField] private CarDriveController car;
        private bool left, right, gas, stop, handbrake, reverse, clutch;
        private bool mobileMode;
        private float steeringSensitivity = 1f;
        private void Awake() => steeringSensitivity = new LocalSettingsStore().Load().steeringSensitivity;
        private bool keyboardHandbrake;
        private DriveGear selectedGear = DriveGear.Drive;
        public bool ReverseSelected => selectedGear == DriveGear.Reverse;
        public bool HandbrakePressed => mobileMode ? handbrake : keyboardHandbrake;
        public DriveGear SelectedGear => selectedGear;

        public void SetMobileButton(int button, bool pressed)
        {
            mobileMode = true;
            switch (button)
            {
                case 0: left = pressed; break;
                case 1: right = pressed; break;
                case 2: gas = pressed; break;
                case 3: stop = pressed; break;
                case 4: handbrake = pressed; break;
            }
        }

        public void SetReverse(bool value)
        {
            selectedGear = value ? DriveGear.Reverse : DriveGear.Drive;
            reverse = value;
            mobileMode = true;
        }
        public void ToggleReverse() => SetReverse(selectedGear != DriveGear.Reverse);
        public void SelectGear(DriveGear gear)
        {
            selectedGear = gear;
            reverse = gear == DriveGear.Reverse;
            mobileMode = true;
        }
        public void ResetCar()
        {
            reverse = false;
            selectedGear = DriveGear.Drive;
            left = right = gas = stop = handbrake = clutch = keyboardHandbrake = false;
            if (car != null) car.ResetVehicle();
        }
        public void SetSteering(float value) { left = value < -0.01f; right = value > 0.01f; mobileMode = true; }
        public void SetThrottle(float value) { gas = value > 0.01f; mobileMode = true; }
        public void SetBrake(float value) { stop = value > 0.01f; mobileMode = true; }
        public void SetHandbrake(bool value) { handbrake = value; mobileMode = true; }
        public void ToggleHandbrake() { handbrake = !handbrake; mobileMode = true; }
        public void ToggleClutch() { clutch = !clutch; mobileMode = true; }
        public void SetClutch(bool value) { clutch = value; mobileMode = true; }

        public void UseKeyboard()
        {
            mobileMode = false;
            selectedGear = DriveGear.Drive;
            keyboardHandbrake = false;
            left = right = gas = stop = handbrake = reverse = clutch = false;
        }

        private void OnDisable()
        {
            left = right = gas = stop = handbrake = reverse = clutch = keyboardHandbrake = false;
            selectedGear = DriveGear.Drive;
            if (car != null) car.SetCommand(default);
        }

        private void Update()
        {
            float steer = (right ? 1f : 0f) - (left ? 1f : 0f);
            float throttle = gas ? 1f : 0f;
            float brake = stop ? 1f : 0f;
            bool park = handbrake;
            bool backward = reverse;
            float clutchValue=clutch?1f:0f;
#if ENABLE_INPUT_SYSTEM
            if (!mobileMode && Keyboard.current != null)
            {
                var k = Keyboard.current;
                steer = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f)
                    - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
                throttle = k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f;
                brake = k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f;
                if (k.spaceKey.wasPressedThisFrame) keyboardHandbrake = !keyboardHandbrake;
                park = keyboardHandbrake;
                if (k.rKey.wasPressedThisFrame)
                    selectedGear = selectedGear == DriveGear.Reverse ? DriveGear.Drive : DriveGear.Reverse;
                if (k.pKey.wasPressedThisFrame) selectedGear = DriveGear.Park;
                if (k.nKey.wasPressedThisFrame) selectedGear = DriveGear.Neutral;
                if (k.gKey.wasPressedThisFrame)
                    selectedGear = DriveGear.Drive;
                if (k.tKey.wasPressedThisFrame) ResetCar();
                backward = selectedGear == DriveGear.Reverse;
                clutchValue=k.leftShiftKey.isPressed || k.rightShiftKey.isPressed ? 1f : 0f;
            }
#endif
            if (car != null)
                car.SetCommand(new CarInputCommand
                {
                    Steering = Mathf.Clamp(steer * steeringSensitivity, -1f, 1f), Throttle = throttle, Brake = brake,
                    Reverse = backward, Handbrake = park, Clutch=clutchValue,
                    HasRequestedGear = true, RequestedGear = selectedGear
                });
        }
    }
}
