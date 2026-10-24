using System;
using UnityEngine;

namespace NepalTrial.Vehicles.TwoWheeler
{
    /// <summary>
    /// Simple low-speed stall model for manual motorcycle; scooter automatic is not affected.
    /// Continuous clutch release in gear with no throttle can stall a stopped bike.
    /// Not a combustion engine simulation; values require gameplay tuning.
    /// </summary>
    public sealed class TwoWheelerEngineState
    {
        private readonly TwoWheelerKind kind;
        private readonly float stallDelaySeconds;
        private float riskySeconds;
        public bool Running { get; private set; } = true;
        public int StallCount { get; private set; }

        public TwoWheelerEngineState(TwoWheelerKind kind,float stallDelaySeconds=.8f)
        {
            if (stallDelaySeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(stallDelaySeconds));
            this.kind=kind;
            this.stallDelaySeconds=stallDelaySeconds;
        }

        /// <summary>Returns true on a NEW stall; callers report it to the trial once.</summary>
        public bool Step(MotorcycleGear gear,float clutch,float throttle,
            float absSpeedMetersPerSecond,bool touchingGround,float deltaSeconds)
        {
            if(deltaSeconds < 0f)throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if(!Running || kind==TwoWheelerKind.Scooter)return false;
            bool atRisk=gear!=MotorcycleGear.Neutral && Mathf.Clamp01(clutch)<.2f &&
                Mathf.Clamp01(throttle)<.1f && absSpeedMetersPerSecond<.6f &&
                touchingGround;
            riskySeconds=atRisk ? riskySeconds+deltaSeconds : 0f;
            if(riskySeconds<stallDelaySeconds)return false;
            Running=false;
            StallCount++;
            riskySeconds=0f;
            return true;
        }

        public void StartEngine()
        {
            Running=true;
            riskySeconds=0f;
        }

        public void Reset()
        {
            Running=true;
            StallCount=0;
            riskySeconds=0f;
        }
    }
}
