using NepalTrial.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NepalTrial.Mobile
{
    /// <summary>Attach to a UI EventTrigger-compatible button and configure Control.</summary>
    public sealed class MobileCarTouchControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum Control { SteerLeft, SteerRight, Throttle, Brake, Handbrake }
        [SerializeField] private PrototypeCarInput target;
        [SerializeField] private Control control;

        public void OnPointerDown(PointerEventData eventData) => SetActive(true);
        public void OnPointerUp(PointerEventData eventData) => SetActive(false);
        public void OnPointerExit(PointerEventData eventData) => SetActive(false);

        private void OnDisable() => SetActive(false);

        private void SetActive(bool active)
        {
            if (target == null) return;
            float axis = active ? 1f : 0f;
            switch (control)
            {
                case Control.SteerLeft: target.SetSteering(-axis); break;
                case Control.SteerRight: target.SetSteering(axis); break;
                case Control.Throttle: target.SetThrottle(axis); break;
                case Control.Brake: target.SetBrake(axis); break;
                case Control.Handbrake: target.SetHandbrake(active); break;
            }
        }
    }
}
