using UnityEngine;

namespace NepalTrial.Vehicles.Car
{
    /// <summary>Device-independent inputs, each normalized to 0..1 or -1..1.</summary>
    public struct CarInputCommand
    {
        public float Steering;
        public float Throttle;
        public float Brake;
        public bool Reverse;
        public bool Handbrake;

        public CarInputCommand Clamped()
        {
            return new CarInputCommand
            {
                Steering = Mathf.Clamp(Steering, -1f, 1f),
                Throttle = Mathf.Clamp01(Throttle),
                Brake = Mathf.Clamp01(Brake),
                Reverse = Reverse,
                Handbrake = Handbrake
            };
        }
    }
}
