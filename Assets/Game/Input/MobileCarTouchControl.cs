using NepalTrial.Input;
using NepalTrial.Vehicles.Car;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NepalTrial.Mobile
{
    /// <summary>Finger-aware press/release; multiple buttons can remain held simultaneously.</summary>
    public sealed class MobileCarTouchControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum Control { SteerLeft, SteerRight, Throttle, Brake, Handbrake, Reverse, Reset, Drive, Neutral, Park }
        [SerializeField] private PrototypeCarInput target;
        [SerializeField] private Control control;
        private int pointerId = int.MinValue;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerId != int.MinValue) return;
            pointerId = eventData.pointerId;
            Apply(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerId != eventData.pointerId) return;
            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Do not release when the finger moves outside: release only on pointer up.
        }

        private void OnDisable() => Release();

        private void Release()
        {
            if (pointerId == int.MinValue) return;
            pointerId = int.MinValue;
            Apply(false);
        }

        private void Apply(bool pressed)
        {
            if (target == null) return;
            if (control == Control.Reverse)
            {
                // Gear button toggles on press, not on pointer release.
                if (pressed) target.ToggleReverse();
                return;
            }
            if (control == Control.Reset)
            {
                if (pressed) target.ResetCar();
                return;
            }
            if (pressed)
            {
                if (control == Control.Drive) { target.SelectGear(DriveGear.Drive); return; }
                if (control == Control.Neutral) { target.SelectGear(DriveGear.Neutral); return; }
                if (control == Control.Park) { target.SelectGear(DriveGear.Park); return; }
            }
            if (control == Control.Drive || control == Control.Neutral || control == Control.Park) return;
            target.SetMobileButton((int)control, pressed);
        }
    }
}
