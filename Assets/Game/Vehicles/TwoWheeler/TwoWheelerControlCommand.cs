using UnityEngine;

namespace NepalTrial.Vehicles.TwoWheeler
{
    /// <summary>Device-independent ride command; applied during physics updates.</summary>
    public struct TwoWheelerControlCommand
    {
        public float Steering;
        public float Throttle;
        public float FrontBrake;
        public float RearBrake;
        public float Clutch;
        public bool FootDown;
        public bool LeftIndicator;
        public bool RightIndicator;

        public TwoWheelerControlCommand Clamped()
        {
            return new TwoWheelerControlCommand
            {
                Steering = Mathf.Clamp(Steering, -1f, 1f),
                Throttle = Mathf.Clamp01(Throttle),
                FrontBrake = Mathf.Clamp01(FrontBrake),
                RearBrake = Mathf.Clamp01(RearBrake),
                Clutch = Mathf.Clamp01(Clutch),
                FootDown = FootDown,
                LeftIndicator = LeftIndicator,
                RightIndicator = RightIndicator
            };
        }
    }
}
