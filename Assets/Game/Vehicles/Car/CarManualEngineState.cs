using UnityEngine;

namespace NepalTrial.Vehicles.Car
{
    /// <summary>
    /// Lightweight manual-clutch training aid, not a combustion simulation.
    /// A stationary D/R pull-away can stall when the clutch is released
    /// after being pressed with insufficient throttle.
    /// </summary>
    public sealed class CarManualEngineState
    {
        private bool clutchWasHeld;
        public bool Running {get;private set;}=true;
        public int StallCount {get;private set;}

        public bool Step(DriveGear gear,float clutch,float throttle,float speedMps)
        {
            bool clutchHeld=Mathf.Clamp01(clutch)>.65f;
            if(!Running){clutchWasHeld=clutchHeld;return false;}
            bool pullingAway=gear==DriveGear.Drive || gear==DriveGear.Reverse;
            bool stall=pullingAway && clutchWasHeld && !clutchHeld &&
                Mathf.Abs(speedMps)<.5f && Mathf.Clamp01(throttle)<.18f;
            clutchWasHeld=clutchHeld;
            if(stall){Running=false;StallCount++;}
            return stall;
        }

        public void RestartEngine() {Running=true;clutchWasHeld=false;}
        public void Reset()
        {
            Running=true;
            StallCount=0;
            clutchWasHeld=false;
        }
    }
}
