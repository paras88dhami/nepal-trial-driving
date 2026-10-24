using NepalTrial.Vehicles.TwoWheeler;
using NepalTrial.Settings;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NepalTrial.Input
{
    /// <summary>Independent, simultaneous mobile controls and keyboard fallback.</summary>
    public sealed class TwoWheelerInput : MonoBehaviour
    {
        public enum Control { Left, Right, Throttle, FrontBrake, RearBrake, Clutch, FootDown,
            ShiftUp, ShiftDown, LeftIndicator, RightIndicator, Reset, Ignition }

        [SerializeField] private TwoWheelerRigidbodyController vehicle;
        private bool left, right, gas, frontBrake, rearBrake, clutch, footDown;
        private bool leftIndicator, rightIndicator, mobileMode;
        private float steeringSensitivity = 1f;
        private void Awake() => steeringSensitivity = new LocalSettingsStore().Load().steeringSensitivity;
        public bool FootDown => footDown;
        public bool LeftIndicator => leftIndicator;
        public bool RightIndicator => rightIndicator;

        public void SetVehicle(TwoWheelerRigidbodyController target) => vehicle = target;

        public void SetMobile(Control control, bool pressed)
        {
            mobileMode = true;
            switch (control)
            {
                case Control.Left: left = pressed; break;
                case Control.Right: right = pressed; break;
                case Control.Throttle: gas = pressed; break;
                case Control.FrontBrake: frontBrake = pressed; break;
                case Control.RearBrake: rearBrake = pressed; break;
                case Control.Clutch: clutch = pressed; break;
                case Control.FootDown: footDown = pressed; break;
                case Control.ShiftUp: if (pressed) Shift(1); break;
                case Control.ShiftDown: if (pressed) Shift(-1); break;
                case Control.LeftIndicator: if (pressed) { leftIndicator = !leftIndicator; rightIndicator = false; } break;
                case Control.RightIndicator: if (pressed) { rightIndicator = !rightIndicator; leftIndicator = false; } break;
                case Control.Reset: if (pressed) ResetVehicle(); break;
                case Control.Ignition: if (pressed && vehicle!=null) vehicle.StartEngine(); break;
            }
            Push();
        }

        public void SetClutch(bool pressed) { clutch = pressed; mobileMode = true; Push(); }
        public void UseKeyboard()
        {
            mobileMode = false;
            ClearInputs();
            Push();
        }

        public void Shift(int step)
        {
            if (vehicle == null || vehicle.Kind == TwoWheelerKind.Scooter) return;
            Push();
            int current = (int)vehicle.CurrentGear;
            int requested = Mathf.Clamp(current + (step > 0 ? 1 : -1),
                (int)MotorcycleGear.Neutral,(int)MotorcycleGear.Fifth);
            vehicle.SelectGear((MotorcycleGear)requested);
        }

        public void ResetVehicle()
        {
            ClearInputs();
            if (vehicle != null) vehicle.ResetVehicle();
        }

        private void ClearInputs()
        {
            left = right = gas = frontBrake = rearBrake = clutch = footDown = false;
            leftIndicator = rightIndicator = false;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (!mobileMode && Keyboard.current != null)
            {
                var k = Keyboard.current;
                left = k.aKey.isPressed || k.leftArrowKey.isPressed;
                right = k.dKey.isPressed || k.rightArrowKey.isPressed;
                gas = k.wKey.isPressed || k.upArrowKey.isPressed;
                frontBrake = k.sKey.isPressed || k.downArrowKey.isPressed;
                rearBrake = k.spaceKey.isPressed;
                clutch = k.leftShiftKey.isPressed || k.rightShiftKey.isPressed;
                footDown = k.fKey.isPressed;
                Push();
                if (k.eKey.wasPressedThisFrame) Shift(1);
                if (k.qKey.wasPressedThisFrame) Shift(-1);
                if (k.zKey.wasPressedThisFrame) { leftIndicator = !leftIndicator; rightIndicator = false; }
                if (k.xKey.wasPressedThisFrame) { rightIndicator = !rightIndicator; leftIndicator = false; }
                if (k.tKey.wasPressedThisFrame) ResetVehicle();
                if (k.iKey.wasPressedThisFrame && vehicle!=null) vehicle.StartEngine();
            }
#endif
            Push();
        }

        private void Push()
        {
            if (vehicle == null) return;
            vehicle.SetControls(new TwoWheelerControlCommand
            {
                Steering = Mathf.Clamp(((right ? 1f : 0f) - (left ? 1f : 0f)) * steeringSensitivity, -1f, 1f),
                Throttle = gas ? 1f : 0f,
                FrontBrake = frontBrake ? 1f : 0f,
                RearBrake = rearBrake ? 1f : 0f,
                Clutch = clutch ? 1f : 0f,
                FootDown = footDown,
                LeftIndicator = leftIndicator,
                RightIndicator = rightIndicator
            });
        }

        private void OnDisable()
        {
            ClearInputs();
            Push();
        }
    }
}
