namespace NepalTrial.Vehicles.TwoWheeler
{
    public enum MotorcycleGear { Neutral, First, Second, Third, Fourth, Fifth }

    /// <summary>Logical gear/clutch requirements; actual engine torque integration is future work.</summary>
    public sealed class TwoWheelerTransmission
    {
        public TwoWheelerKind Kind { get; private set; }
        public MotorcycleGear Gear { get; private set; }
        public TwoWheelerTransmission(TwoWheelerKind kind) { Kind = kind; Gear = MotorcycleGear.Neutral; }
        public bool SelectGear(MotorcycleGear next, bool clutchPressed, float speedMetersPerSecond)
        {
            if (Kind == TwoWheelerKind.Scooter) return false;
            if (!clutchPressed || speedMetersPerSecond < 0f) return false;
            if (next < MotorcycleGear.Neutral || next > MotorcycleGear.Fifth) return false;
            Gear = next;
            return true;
        }
        public bool CanDrive(bool clutchPressed) => Kind == TwoWheelerKind.Scooter || (Gear != MotorcycleGear.Neutral && !clutchPressed);
        public void Reset() => Gear = MotorcycleGear.Neutral;
    }
}
