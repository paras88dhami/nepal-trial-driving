using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Evaluates crossing a checkpoint plane in its forward direction, within gate width.</summary>
    public static class DirectionalCheckpointGate
    {
        public static bool Crossed(Vector3 previous, Vector3 current, Vector3 center, Vector3 forward, float halfWidth)
        {
            if (halfWidth <= 0f) return false;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return false;
            forward.Normalize();
            Vector3 side = new Vector3(-forward.z, 0f, forward.x);
            Vector3 previousOffset = previous - center;
            Vector3 currentOffset = current - center;
            float before = Vector3.Dot(previousOffset, forward);
            float after = Vector3.Dot(currentOffset, forward);
            if (before >= 0f || after < 0f || after <= before) return false;
            float fraction = -before / (after - before);
            Vector3 crossing = Vector3.Lerp(previousOffset, currentOffset, fraction);
            return Mathf.Abs(Vector3.Dot(crossing, side)) <= halfWidth;
        }
    }
}
