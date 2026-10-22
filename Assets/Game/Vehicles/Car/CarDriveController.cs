using UnityEngine;

namespace NepalTrial.Vehicles.Car
{
    /// <summary>Simple four-wheel car foundation. Needs Unity Editor + driving tests before release.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarDriveController : MonoBehaviour
    {
        [Header("Wheels")]
        [SerializeField] private WheelCollider frontLeft;
        [SerializeField] private WheelCollider frontRight;
        [SerializeField] private WheelCollider rearLeft;
        [SerializeField] private WheelCollider rearRight;

        [Header("Tuning")]
        [SerializeField, Min(0)] private float motorTorque = 1100f;
        [SerializeField, Min(0)] private float brakeTorque = 2400f;
        [SerializeField, Range(5, 45)] private float maxSteerAngle = 30f;
        [SerializeField, Min(1)] private float topSpeedKph = 35f;
        [SerializeField, Min(0)] private float handbrakeTorque = 3500f;

        private Rigidbody body;
        private CarInputCommand command;

        public float SpeedKph => body != null ? body.linearVelocity.magnitude * 3.6f : 0f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 1050f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            if (!WheelsReady())
                Debug.LogError("Assign all four WheelColliders before driving.", this);
        }

        public void SetCommand(CarInputCommand value) => command = value.Clamped();

        private bool WheelsReady() =>
            frontLeft != null && frontRight != null && rearLeft != null && rearRight != null;

        private void FixedUpdate()
        {
            if (!WheelsReady()) return;

            float steering = command.Steering * maxSteerAngle;
            frontLeft.steerAngle = steering;
            frontRight.steerAngle = steering;

            float requestedTorque = SpeedKph < topSpeedKph
                ? command.Throttle * motorTorque * (command.Reverse ? -1f : 1f)
                : 0f;

            rearLeft.motorTorque = requestedTorque * 0.5f;
            rearRight.motorTorque = requestedTorque * 0.5f;

            float normalBrake = command.Brake * brakeTorque;
            frontLeft.brakeTorque = normalBrake;
            frontRight.brakeTorque = normalBrake;
            rearLeft.brakeTorque = normalBrake + (command.Handbrake ? handbrakeTorque : 0f);
            rearRight.brakeTorque = normalBrake + (command.Handbrake ? handbrakeTorque : 0f);
        }

        private void OnDisable()
        {
            command = default;
            if (!WheelsReady()) return;
            rearLeft.motorTorque = rearRight.motorTorque = 0f;
        }
    }
}
