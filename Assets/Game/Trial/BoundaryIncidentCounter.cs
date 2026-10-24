namespace NepalTrial.Trial
{
    /// <summary>Counts transitions from all wheels in bounds to any wheel out of bounds.
    /// Does not claim to implement official Nepal driving examination penalties.</summary>
    public sealed class BoundaryIncidentCounter
    {
        public int IncidentCount { get; private set; }
        public bool IsOutside { get; private set; }
        public bool Update(bool anyWheelOutside)
        {
            bool newIncident = anyWheelOutside && !IsOutside;
            if (newIncident) IncidentCount++;
            IsOutside = anyWheelOutside;
            return newIncident;
        }
        public void Reset()
        {
            IncidentCount = 0;
            IsOutside = false;
        }
    }
}
