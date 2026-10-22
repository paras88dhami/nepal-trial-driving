using UnityEngine;
using UnityEngine.EventSystems;

namespace NepalTrial.Input
{
    /// <summary>Attach to individual UI graphics with Raycast Target enabled.</summary>
    public sealed class MobileHoldPedal : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum Control { Throttle, Brake, SteerLeft, SteerRight, Handbrake }
        [SerializeField] private PrototypeCarInput input;
        [SerializeField] private Control control;
        private bool held;

        public void OnPointerDown(PointerEventData eventData) => Set(true);
        public void OnPointerUp(PointerEventData eventData) => Set(false);
        public void OnPointerExit(PointerEventData eventData) => Set(false);

        private void Set(bool value)
        {
            if (input == null) return;
            held = value;
            switch (control)
            {
                case Control.Throttle: input.SetThrottle(value ? 1f : 0f); break;
                case Control.Brake: input.SetBrake(value ? 1f : 0f); break;
                case Control.Handbrake: input.SetHandbrake(value); break;
                case Control.SteerLeft: input.SetSteering(value ? -1f : 0f); break;
                case Control.SteerRight: input.SetSteering(value ? 1f : 0f); break;
            }
        }

        private void OnDisable() { if (held) Set(false); }
    }
}
