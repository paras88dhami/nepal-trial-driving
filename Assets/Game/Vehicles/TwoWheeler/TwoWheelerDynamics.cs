using System;
using UnityEngine;

namespace NepalTrial.Vehicles.TwoWheeler
{
    public enum TwoWheelerKind { Motorcycle, Scooter }

    /// <summary>Deterministic assistance model only; Rigidbody steering, tire grip and collisions still need scene integration.</summary>
    public sealed class TwoWheelerDynamics
    {
        public TwoWheelerKind Kind { get; private set; }
        public float LeanDegrees { get; private set; }
        public float SpeedMetersPerSecond { get; private set; }
        public float MotorResponse { get; private set; }
        public TwoWheelerDynamics(TwoWheelerKind kind) { Kind = kind; }

        public void Step(float throttle, float brake, float steering, float observedSpeedMetersPerSecond,
            float deltaSeconds, float maxLeanDegrees, float leanResponsePerSecond)
        {
            if (deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (maxLeanDegrees < 0 || leanResponsePerSecond < 0) throw new ArgumentOutOfRangeException();
            SpeedMetersPerSecond = Mathf.Max(0, observedSpeedMetersPerSecond);
            throttle = Mathf.Clamp01(throttle);
            brake = Mathf.Clamp01(brake);
            steering = Mathf.Clamp(steering, -1f, 1f);
            float targetLean = brake > 0.5f ? 0f : -steering * maxLeanDegrees * Mathf.Clamp01(SpeedMetersPerSecond / 5f);
            LeanDegrees = Mathf.MoveTowards(LeanDegrees, targetLean, leanResponsePerSecond * deltaSeconds);
            MotorResponse = brake > 0.01f ? 0f : throttle;
        }

        public void Reset()
        {
            LeanDegrees = 0;
            SpeedMetersPerSecond = 0;
            MotorResponse = 0;
        }
    }
}
