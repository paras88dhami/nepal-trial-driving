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
        [SerializeField, Min(100f)] private float vehicleMassKg = 1050f;
        [SerializeField, Min(0)] private float motorTorque = 1100f;
        [SerializeField, Min(0)] private float brakeTorque = 2400f;
        [SerializeField, Range(5, 45)] private float maxSteerAngle = 30f;
        [SerializeField, Min(1)] private float topSpeedKph = 35f;
        [SerializeField, Min(0)] private float handbrakeTorque = 3500f;
        [SerializeField, Range(0.1f, 1f)] private float highSpeedSteeringFraction = 0.45f;
        [SerializeField, Min(1)] private float steeringReductionSpeedKph = 35f;

        [SerializeField] private bool manualClutchMode;
        private readonly CarManualEngineState engine=new CarManualEngineState();
        private Rigidbody body;
        private CarInputCommand command;
        private Vector3 resetPosition;
        private Quaternion resetRotation;
        private readonly TransmissionState transmission = new TransmissionState();
        public DriveGear CurrentGear => transmission.Gear;
        public bool EngineRunning => engine.Running;
        public int StallCount => engine.StallCount;
        public bool ManualClutchMode => manualClutchMode;
        public bool HandbrakeEngaged => transmission.HandbrakeOn;

        public float SpeedKph => body != null ? body.linearVelocity.magnitude * 3.6f : 0f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = Mathf.Max(100f, vehicleMassKg);
            body.interpolation = RigidbodyInterpolation.Interpolate;
            resetPosition = body.position;
            resetRotation = body.rotation;
            // Lower center of mass to improve prototype rollover resistance.
            body.centerOfMass += new Vector3(0f, -0.25f, 0f);
            // Preserve prototype behaviour: begin in Drive with parking brake released.
            transmission.TrySelect(DriveGear.Drive, 0f, true);
            transmission.SetHandbrake(false);
            if (!WheelsReady())
                Debug.LogError("Assign all four WheelColliders before driving.", this);
        }

        public void SetCommand(CarInputCommand value) => command = value.Clamped();
        public void StartEngine() => engine.RestartEngine();

        /// <summary>Returns car to initial position; clears momentum, gear and wheel torques.</summary>
        public void ResetVehicle()
        {
            if (body == null) return;
            command = default;
            engine.Reset();
            body.position = resetPosition;
            body.rotation = resetRotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            transmission.SetHandbrake(false);
            transmission.TrySelect(DriveGear.Drive, 0f, true);
            ClearWheelForces();
            body.WakeUp();
        }

        private void ClearWheelForces()
        {
            if (!WheelsReady()) return;
            foreach (var wheel in new[] { frontLeft, frontRight, rearLeft, rearRight })
            {
                wheel.motorTorque = 0f;
                wheel.brakeTorque = 0f;
            }
        }

        private bool WheelsReady() =>
            frontLeft != null && frontRight != null && rearLeft != null && rearRight != null;

        private void FixedUpdate()
        {
            if (!WheelsReady()) return;

            // Reverse is requested by the existing mobile/keyboard input; the interlock
            // refuses a Drive/Reverse switch while moving.
            DriveGear requested = command.HasRequestedGear ? command.RequestedGear :
                (command.Reverse ? DriveGear.Reverse : DriveGear.Drive);
            if (transmission.Gear != requested)
                transmission.TrySelect(requested, SpeedKph / 3.6f, command.Brake > 0.1f || SpeedKph < 0.72f);
            transmission.SetHandbrake(command.Handbrake);
            if(manualClutchMode)
                engine.Step(transmission.Gear,command.Clutch,command.Throttle,SpeedKph/3.6f);

            float steering = CarDriveMath.SteeringAngle(command.Steering, SpeedKph,
                maxSteerAngle, highSpeedSteeringFraction, steeringReductionSpeedKph);
            frontLeft.steerAngle = steering;
            frontRight.steerAngle = steering;

            // Never propel in the old direction while a requested gear change is blocked.
            float signedThrottle = transmission.Gear == requested ? transmission.SignedThrottle(command.Throttle) : 0f;
            float requestedTorque = (!manualClutchMode ||
                (engine.Running && command.Clutch<.65f))
                ? CarDriveMath.DriveTorque(signedThrottle, SpeedKph,
                    topSpeedKph, command.Brake, transmission.HandbrakeOn) * motorTorque
                : 0f;
            rearLeft.motorTorque = requestedTorque * 0.5f;
            rearRight.motorTorque = requestedTorque * 0.5f;

            float normalBrake = CarDriveMath.BrakeTorque(command.Brake, brakeTorque);
            bool parked = transmission.Gear == DriveGear.Park;
            frontLeft.brakeTorque = CarDriveMath.WheelBrakeTorque(normalBrake, handbrakeTorque, parked, transmission.HandbrakeOn, false);
            frontRight.brakeTorque = CarDriveMath.WheelBrakeTorque(normalBrake, handbrakeTorque, parked, transmission.HandbrakeOn, false);
            rearLeft.brakeTorque = CarDriveMath.WheelBrakeTorque(normalBrake, handbrakeTorque, parked, transmission.HandbrakeOn, true);
            rearRight.brakeTorque = CarDriveMath.WheelBrakeTorque(normalBrake, handbrakeTorque, parked, transmission.HandbrakeOn, true);
        }

        private void OnDisable()
        {
            command = default;
            ClearWheelForces();
        }
    }
}
