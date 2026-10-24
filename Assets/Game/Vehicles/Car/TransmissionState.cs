namespace NepalTrial.Vehicles.Car
{
    public enum DriveGear { Park, Neutral, Drive, Reverse }

    /// <summary>Pure transmission interlock, independent of WheelCollider implementation.</summary>
    public sealed class TransmissionState
    {
        public DriveGear Gear { get; private set; } = DriveGear.Park;
        public bool HandbrakeOn { get; private set; } = true;
        public bool TrySelect(DriveGear next, float speedMetersPerSecond, bool brakePressed)
        {
            if (speedMetersPerSecond < 0f) speedMetersPerSecond = -speedMetersPerSecond;
            if ((next == DriveGear.Park || next == DriveGear.Reverse || next == DriveGear.Drive)
                && (speedMetersPerSecond > 0.2f || !brakePressed)) return false;
            Gear = next;
            return true;
        }
        public void SetHandbrake(bool enabled) => HandbrakeOn = enabled;
        public float SignedThrottle(float throttle)
        {
            if (HandbrakeOn || Gear == DriveGear.Park || Gear == DriveGear.Neutral) return 0f;
            if (throttle < 0f) throttle = 0f;
            if (throttle > 1f) throttle = 1f;
            return Gear == DriveGear.Reverse ? -throttle : throttle;
        }
    }
}
