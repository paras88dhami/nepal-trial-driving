using UnityEngine;

namespace NepalTrial.Vehicles.TwoWheeler
{
    /// <summary>
    /// Experimental Rigidbody two-wheeler with assisted upright balance.
    /// Not a validated motorcycle simulation; adjust collider/contact geometry in Unity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TwoWheelerRigidbodyController : MonoBehaviour
    {
        [SerializeField] private TwoWheelerKind kind = TwoWheelerKind.Motorcycle;
        [SerializeField, Min(0f)] private float driveAcceleration = 4f;
        [SerializeField, Min(0f)] private float brakingAcceleration = 8f;
        [SerializeField, Min(1f)] private float speedLimitKph = 30f;
        [SerializeField, Min(0f)] private float maxYawRateDegrees = 65f;
        [SerializeField, Min(0f)] private float uprightStrength = 25f;
        [SerializeField, Min(0f)] private float uprightDamping = 6f;
        [SerializeField, Min(0f)] private float maxLeanDegrees = 24f;
        [SerializeField, Min(0f)] private float leanResponse = 80f;
        [SerializeField] private Transform visualBody;
        private Rigidbody body;
        private TwoWheelerDynamics dynamics;
        private float throttle;
        private float brake;
        private float steering;
        private bool enabledDrive = true;

        public float SpeedKph => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;
        public float LeanDegrees => dynamics == null ? 0f : dynamics.LeanDegrees;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            dynamics = new TwoWheelerDynamics(kind);
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void SetControls(float requestedThrottle, float requestedBrake, float requestedSteering)
        {
            throttle = Mathf.Clamp01(requestedThrottle);
            brake = Mathf.Clamp01(requestedBrake);
            steering = Mathf.Clamp(requestedSteering, -1f, 1f);
        }

        public void SetDriveEnabled(bool value) => enabledDrive = value;

        private void FixedUpdate()
        {
            if (body == null || dynamics == null) return;
            float dt = Time.fixedDeltaTime;
            float currentThrottle = enabledDrive ? throttle : 0f;
            dynamics.Step(currentThrottle, brake, steering, SpeedKph / 3.6f, dt, maxLeanDegrees, leanResponse);

            Vector3 planarVelocity = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
            float forwardSpeed = Vector3.Dot(planarVelocity, transform.forward);
            if (currentThrottle > 0f && forwardSpeed * 3.6f < speedLimitKph && brake < 0.01f)
                body.AddForce(transform.forward * driveAcceleration * currentThrottle, ForceMode.Acceleration);
            if (brake > 0f && planarVelocity.sqrMagnitude > 0.0001f)
                body.AddForce(-planarVelocity.normalized * Mathf.Min(brakingAcceleration * brake, planarVelocity.magnitude / dt), ForceMode.Acceleration);

            float movingFactor = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 2f);
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, steering * maxYawRateDegrees * movingFactor * dt, 0f));

            // Applies an assisted righting torque; dynamic behavior requires tuning.
            Vector3 desiredUp = Quaternion.AngleAxis(dynamics.LeanDegrees, transform.forward) * Vector3.up;
            Vector3 correction = Vector3.Cross(transform.up, desiredUp);
            body.AddTorque(correction * uprightStrength - body.angularVelocity * uprightDamping, ForceMode.Acceleration);
            if (visualBody != null)
                visualBody.localRotation = Quaternion.Lerp(visualBody.localRotation,
                    Quaternion.Euler(0f, 0f, dynamics.LeanDegrees), Mathf.Clamp01(dt * 8f));
        }

        private void OnDisable()
        {
            throttle = brake = steering = 0f;
            if (dynamics != null) dynamics.Reset();
        }
    }
}
