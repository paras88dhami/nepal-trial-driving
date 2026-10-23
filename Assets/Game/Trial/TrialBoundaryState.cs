using System.Collections.Generic;

namespace NepalTrial.Trial
{
    /// <summary>Deduplicates a boundary violation until its wheel exits the area.</summary>
    public sealed class TrialBoundaryState
    {
        private readonly HashSet<string> active = new HashSet<string>();
        public int ViolationCount { get; private set; }

        public bool Enter(string wheelId, string boundaryId)
        {
            string key = wheelId + "|" + boundaryId;
            if (!active.Add(key)) return false;
            ViolationCount++;
            return true;
        }

        public void Exit(string wheelId, string boundaryId) => active.Remove(wheelId + "|" + boundaryId);
        public void Reset()
        {
            active.Clear();
            ViolationCount = 0;
        }
    }
}
