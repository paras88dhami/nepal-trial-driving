using NepalTrial.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NepalTrial.Mobile
{
    /// <summary>
    /// Analog on-screen steering wheel for the existing car physics controller.
    /// Rotate with a finger/mouse; returns to center when released.
    /// The root needs a Raycast Target Image and an EventSystem.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class MobileAnalogSteeringWheel : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private PrototypeCarInput carInput;
        [SerializeField] private RectTransform wheelGraphic;
        [SerializeField, Range(90f, 720f)] private float maxRotationDegrees = 450f;
        [SerializeField, Min(50f)] private float returnSpeedDegreesPerSecond = 390f;
        private int draggingPointer = int.MinValue;
        private float lastPointerAngle;
        private float rotationDegrees;

        public float NormalizedSteering => Mathf.Clamp(rotationDegrees/maxRotationDegrees,-1f,1f);

        public void Bind(PrototypeCarInput input,RectTransform graphic)
        {
            carInput=input;
            wheelGraphic=graphic;
            UpdateWheel();
        }

        private bool TryGetAngle(PointerEventData data,out float degrees)
        {
            degrees=0f;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform,data.position,data.pressEventCamera,out Vector2 local))
                return false;
            // The installer anchors this RectTransform at its lower-left.
            // Pointer angles must be measured relative to the visual center.
            local-=(transform as RectTransform).rect.center;
            if(local.sqrMagnitude<64f)return false;
            degrees=Mathf.Atan2(local.y,local.x)*Mathf.Rad2Deg;
            return true;
        }

        public void OnPointerDown(PointerEventData data)
        {
            if(draggingPointer!=int.MinValue)return;
            if(!TryGetAngle(data,out float angle))return;
            draggingPointer=data.pointerId;
            lastPointerAngle=angle;
            carInput?.SetAnalogSteering(NormalizedSteering);
        }

        public void OnDrag(PointerEventData data)
        {
            if(data.pointerId!=draggingPointer || !TryGetAngle(data,out float next))return;
            rotationDegrees=Mathf.Clamp(
                rotationDegrees+Mathf.DeltaAngle(lastPointerAngle,next),
                -maxRotationDegrees,maxRotationDegrees);
            lastPointerAngle=next;
            UpdateWheel();
        }

        public void OnPointerUp(PointerEventData data)
        {
            if(data.pointerId==draggingPointer)draggingPointer=int.MinValue;
        }

        private void UpdateWheel()
        {
            if(wheelGraphic!=null)
                wheelGraphic.localRotation=Quaternion.Euler(0f,0f,rotationDegrees);
            carInput?.SetAnalogSteering(NormalizedSteering);
        }

        private void Update()
        {
            if(draggingPointer!=int.MinValue)return;
            if(Mathf.Abs(rotationDegrees)>.01f)
            {
                rotationDegrees=Mathf.MoveTowards(rotationDegrees,0f,
                    returnSpeedDegreesPerSecond*Time.unscaledDeltaTime);
                UpdateWheel();
            }
        }

        private void OnDisable()
        {
            draggingPointer=int.MinValue;
            rotationDegrees=0f;
            if(wheelGraphic!=null)wheelGraphic.localRotation=Quaternion.identity;
            carInput?.ReleaseAnalogSteering();
        }
    }
}
