using System;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Direction-sensitive ordered checkpoint progression; pure, testable route logic.</summary>
    public sealed class OrderedDrivingGateTracker
    {
        private readonly Vector3[] centers;
        private readonly Vector3[] forward;
        private readonly float halfWidth;
        public int NextIndex { get; private set; }
        public int Total => centers.Length;
        public bool Finished => NextIndex == centers.Length;

        public OrderedDrivingGateTracker(Vector3[] positions, Vector3[] directions, float gateHalfWidth)
        {
            if (positions == null || directions == null || positions.Length == 0 ||
                positions.Length != directions.Length || gateHalfWidth <= 0f)
                throw new ArgumentException("Gates and directions must have matching nonempty lengths and positive width.");
            centers = (Vector3[])positions.Clone();
            forward = (Vector3[])directions.Clone();
            halfWidth = gateHalfWidth;
        }

        public bool Sample(Vector3 from, Vector3 to)
        {
            if (Finished) return false;
            if (!DirectionalCheckpointGate.Crossed(from, to, centers[NextIndex], forward[NextIndex], halfWidth))
                return false;
            NextIndex++;
            return true;
        }

        public void Reset() => NextIndex = 0;
    }
}
