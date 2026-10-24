using UnityEngine;
using UnityEngine.EventSystems;

namespace NepalTrial.Input
{
    /// <summary>Hold controls release on pointer-up; one-shot controls trigger only on pointer-down.</summary>
    public sealed class MobileTwoWheelerTouchControl : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerCancelHandler
    {
        [SerializeField] private TwoWheelerInput input;
        [SerializeField] private TwoWheelerInput.Control control;
        private int heldPointer = int.MinValue;

        public void OnPointerDown(PointerEventData data)
        {
            if (heldPointer != int.MinValue) return;
            heldPointer = data.pointerId;
            if (input != null) input.SetMobile(control,true);
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (heldPointer != data.pointerId) return;
            Clear();
        }

        public void OnPointerCancel(PointerEventData data)
        {
            if (heldPointer != data.pointerId) return;
            Clear();
        }

        private void OnDisable() => Clear();

        private void Clear()
        {
            if (heldPointer == int.MinValue) return;
            heldPointer = int.MinValue;
            if (input != null) input.SetMobile(control,false);
        }
    }
}
