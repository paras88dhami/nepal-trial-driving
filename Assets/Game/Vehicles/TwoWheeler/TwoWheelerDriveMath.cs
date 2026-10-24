using UnityEngine;

namespace NepalTrial.Vehicles.TwoWheeler
{
    /// <summary>Deterministic, testable arcade-physics helpers. Tuning is provisional.</summary>
    public static class TwoWheelerDriveMath
    {
        public static float MotorTorque(float throttle, float forwardSpeedKph, float limitKph,
            float baseTorque, bool driveEnabled, bool driveGearEngaged, float frontBrake, float rearBrake)
        {
            if (!driveEnabled || !driveGearEngaged || Mathf.Max(frontBrake,rearBrake) > .1f ||
                forwardSpeedKph >= limitKph || baseTorque <= 0f) return 0f;
            float speedFactor = Mathf.Clamp01(1f - Mathf.Max(0f,forwardSpeedKph) /
                Mathf.Max(.1f,limitKph));
            return Mathf.Clamp01(throttle) * baseTorque * (.2f + .8f * speedFactor);
        }

        public static float SteeringAngle(float input, float speedKph, float maxAngle,
            float reducedFraction, float reductionAtKph)
        {
            float fraction = Mathf.Lerp(1f, Mathf.Clamp01(reducedFraction),
                Mathf.Clamp01(Mathf.Abs(speedKph) / Mathf.Max(.1f,reductionAtKph)));
            return Mathf.Clamp(input, -1f, 1f) * Mathf.Max(0f,maxAngle) * fraction;
        }

        public static float BrakeTorque(float brakeInput, float maximumTorque)
            => Mathf.Clamp01(brakeInput) * Mathf.Max(0f,maximumTorque);

        public static bool BothWheelsGrounded(bool frontGrounded, bool rearGrounded)
            => frontGrounded && rearGrounded;
    }
}
