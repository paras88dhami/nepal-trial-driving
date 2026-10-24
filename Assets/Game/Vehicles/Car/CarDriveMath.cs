using UnityEngine;

namespace NepalTrial.Vehicles.Car
{
    /// <summary>Deterministic vehicle input calculations, separated from WheelColliders for tests.</summary>
    public static class CarDriveMath
    {
        public static float SteeringAngle(float steer, float speedKph, float maxAngle, float highSpeedFraction, float speedForReductionKph)
        {
            float blend = speedForReductionKph > 0f ? Mathf.Clamp01(speedKph / speedForReductionKph) : 1f;
            return Mathf.Clamp(steer, -1f, 1f) * Mathf.Max(0f, maxAngle) *
                Mathf.Lerp(1f, Mathf.Clamp01(highSpeedFraction), blend);
        }

        public static float DriveTorque(float signedThrottle, float speedKph, float topSpeedKph, float brake, bool handbrake)
        {
            if (handbrake || brake > 0.01f || speedKph >= topSpeedKph) return 0f;
            return Mathf.Clamp(signedThrottle, -1f, 1f);
        }

        public static float BrakeTorque(float brake, float torque) =>
            Mathf.Clamp01(brake) * Mathf.Max(0f, torque);
    }
}
