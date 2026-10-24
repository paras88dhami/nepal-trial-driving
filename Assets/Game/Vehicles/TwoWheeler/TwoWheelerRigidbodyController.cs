using UnityEngine;

namespace NepalTrial.Vehicles.TwoWheeler
{
    /// <summary>
    /// Physics-connected two-wheeler foundation using front/rear WheelColliders.
    /// The assisted balance is deliberately arcade-like: tune on real Android hardware.
    /// No claim of exact motorcycle dynamics or official Nepal vehicle parameters.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TwoWheelerRigidbodyController : MonoBehaviour
    {
        [Header("Vehicle and wheels")]
        [SerializeField] private TwoWheelerKind kind = TwoWheelerKind.Motorcycle;
        [SerializeField] private WheelCollider frontWheel;
        [SerializeField] private WheelCollider rearWheel;
        [SerializeField] private Transform frontWheelVisual;
        [SerializeField] private Transform rearWheelVisual;
        [SerializeField] private Transform visualBody;

        [Header("Drive and brakes")]
        [SerializeField, Min(0f)] private float rearMotorTorque = 160f;
        [SerializeField, Min(0f)] private float frontBrakeTorque = 250f;
        [SerializeField, Min(0f)] private float rearBrakeTorque = 220f;
        [SerializeField, Min(1f)] private float speedLimitKph = 40f;
        [SerializeField, Range(1f,40f)] private float maxSteerAngle = 24f;
        [SerializeField, Range(.1f,1f)] private float highSpeedSteerFraction = .38f;
        [SerializeField, Min(1f)] private float reduceSteeringAtKph = 35f;

        [Header("Stability and body")]
        [SerializeField, Min(30f)] private float vehicleMassKg = 170f;
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f,-.4f,0f);
        [SerializeField, Min(0f)] private float balanceStrength = 35f;
        [SerializeField, Min(0f)] private float balanceDamping = 7f;
        [SerializeField, Range(0f,40f)] private float maximumAssistedLean = 17f;
        [SerializeField, Min(0f)] private float leanResponseDegreesPerSecond = 90f;
        [SerializeField, Min(0f)] private float turningAssist = 1.8f;
        [SerializeField, Min(0f)] private float fallingThresholdDegrees = 57f;

        private Rigidbody body;
        private TwoWheelerDynamics dynamics;
        private TwoWheelerTransmission transmission;
        private TwoWheelerEngineState engine;
        private TwoWheelerControlCommand command;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private bool driveEnabled = true;

        public TwoWheelerKind Kind => kind;
        public MotorcycleGear CurrentGear => transmission == null ? MotorcycleGear.Neutral : transmission.Gear;
        public bool EngineRunning => engine == null || engine.Running;
        public int StallCount => engine == null ? 0 : engine.StallCount;
        public float SpeedKph => body == null ? 0f : body.linearVelocity.magnitude * 3.6f;
        public float ForwardSpeedKph => body == null ? 0f :
            Vector3.Dot(body.linearVelocity, transform.forward) * 3.6f;
        public float LeanDegrees => dynamics == null ? 0f : dynamics.LeanDegrees;
        public float TiltDegrees => Vector3.Angle(transform.up, Vector3.up);
        public bool HasFallen => TiltDegrees >= fallingThresholdDegrees;
        public bool FrontGrounded => frontWheel != null && frontWheel.isGrounded;
        public bool RearGrounded => rearWheel != null && rearWheel.isGrounded;
        public bool FootDown => command.FootDown;
        public bool LeftIndicator => command.LeftIndicator;
        public bool RightIndicator => command.RightIndicator;
        public bool ClutchPressed => command.Clutch >= .5f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            spawnPosition = body.position;
            spawnRotation = body.rotation;
            body.mass = Mathf.Max(30f, vehicleMassKg);
            body.centerOfMass += centerOfMassOffset;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            dynamics = new TwoWheelerDynamics(kind);
            transmission = new TwoWheelerTransmission(kind);
            engine = new TwoWheelerEngineState(kind);
            if (frontWheel == null || rearWheel == null)
            {
                Debug.LogError("Assign both WheelColliders to the two-wheeler controller.",this);
                enabled = false;
            }
        }

        public void SetControls(float requestedThrottle, float requestedBrake, float requestedSteering)
        {
            command = new TwoWheelerControlCommand
            {
                Throttle = requestedThrottle,
                FrontBrake = requestedBrake,
                RearBrake = requestedBrake,
                Steering = requestedSteering
            }.Clamped();
        }

        public void SetControls(TwoWheelerControlCommand next) => command = next.Clamped();
        public void SetDriveEnabled(bool value) => driveEnabled = value;
        public void StartEngine() { if (engine != null) engine.StartEngine(); }

        public bool SelectGear(MotorcycleGear gear)
        {
            return transmission != null &&
                transmission.SelectGear(gear, command.Clutch >= .5f, Mathf.Abs(ForwardSpeedKph) / 3.6f);
        }

        public void ResetVehicle()
        {
            if (body == null) return;
            body.position = spawnPosition;
            body.rotation = spawnRotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            command = default;
            driveEnabled = true;
            if (transmission != null) transmission.Reset();
            if (engine != null) engine.Reset();
            if (dynamics != null) dynamics.Reset();
            ClearWheelForces();
            body.WakeUp();
        }

        private void ClearWheelForces()
        {
            if (frontWheel != null)
            {
                frontWheel.motorTorque = 0f;
                frontWheel.brakeTorque = 0f;
                frontWheel.steerAngle = 0f;
            }
            if (rearWheel != null)
            {
                rearWheel.motorTorque = 0f;
                rearWheel.brakeTorque = 0f;
                rearWheel.steerAngle = 0f;
            }
        }

        private void FixedUpdate()
        {
            if (body == null || frontWheel == null || rearWheel == null) return;
            float dt = Time.fixedDeltaTime;
            float forwardSpeed = Mathf.Abs(ForwardSpeedKph);
            float effectiveThrottle = driveEnabled ? command.Throttle : 0f;
            dynamics.Step(effectiveThrottle,Mathf.Max(command.FrontBrake,command.RearBrake),
                command.Steering,SpeedKph / 3.6f,dt,maximumAssistedLean,
                leanResponseDegreesPerSecond);

            if (engine != null) engine.Step(transmission.Gear,command.Clutch,command.Throttle,
                forwardSpeed / 3.6f,FrontGrounded && RearGrounded,dt);
            bool driveGear = EngineRunning && transmission.CanDrive(command.Clutch >= .5f);
            frontWheel.motorTorque = 0f;
            rearWheel.motorTorque = TwoWheelerDriveMath.MotorTorque(
                effectiveThrottle,forwardSpeed,speedLimitKph,rearMotorTorque,
                driveEnabled,driveGear,command.FrontBrake,command.RearBrake);
            frontWheel.brakeTorque = TwoWheelerDriveMath.BrakeTorque(
                command.FrontBrake,frontBrakeTorque);
            rearWheel.brakeTorque = TwoWheelerDriveMath.BrakeTorque(
                command.RearBrake,rearBrakeTorque);
            frontWheel.steerAngle = TwoWheelerDriveMath.SteeringAngle(
                command.Steering,SpeedKph,maxSteerAngle,highSpeedSteerFraction,
                reduceSteeringAtKph);

            // Upright assist works on Rigidbody torque, NOT teleporting the rotation.
            // Lean is a speed-dependent target, reduced when the player brakes.
            Vector3 desiredUp = Quaternion.AngleAxis(dynamics.LeanDegrees,transform.forward) * Vector3.up;
            Vector3 rollCorrection = Vector3.Cross(transform.up,desiredUp);
            Vector3 tiltAngularVelocity = Vector3.ProjectOnPlane(body.angularVelocity,Vector3.up);
            if (FrontGrounded || RearGrounded)
            {
                body.AddTorque(rollCorrection * balanceStrength -
                    tiltAngularVelocity * balanceDamping,ForceMode.Acceleration);
                if (FrontGrounded && forwardSpeed > .5f)
                    body.AddTorque(Vector3.up * command.Steering *
                        turningAssist * Mathf.Clamp01(forwardSpeed / 18f),
                        ForceMode.Acceleration);
            }

            if (visualBody != null)
                visualBody.localRotation = Quaternion.Lerp(visualBody.localRotation,
                    Quaternion.Euler(0f,0f,dynamics.LeanDegrees * .35f),
                    Mathf.Clamp01(dt*6f));
            UpdateVisual(frontWheel,frontWheelVisual);
            UpdateVisual(rearWheel,rearWheelVisual);
        }

        private static void UpdateVisual(WheelCollider wheel,Transform target)
        {
            if (wheel == null || target == null) return;
            wheel.GetWorldPose(out Vector3 position,out Quaternion rotation);
            target.SetPositionAndRotation(position,rotation);
        }

        private void OnDisable()
        {
            command = default;
            ClearWheelForces();
            if (dynamics != null) dynamics.Reset();
        }
    }
}
