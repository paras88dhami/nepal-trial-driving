using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Prototype geometric tests only. Distances are not official Nepal trial measurements.</summary>
    public static class FigureEightTrackMath
    {
        public static float DistanceFromCircle(Vector2 point, Vector2 center, float radius)
            => Mathf.Abs(Vector2.Distance(point, center) - radius);

        public static bool IsWithinAnnulus(Vector2 point, Vector2 center, float radius, float halfWidth)
            => DistanceFromCircle(point, center, radius) <= Mathf.Max(0f, halfWidth);

        public static bool HasCrossedPlane(Vector3 previous, Vector3 current, Vector3 planePoint, Vector3 normal)
        {
            float a = Vector3.Dot(previous - planePoint, normal);
            float b = Vector3.Dot(current - planePoint, normal);
            return a < 0f && b >= 0f;
        }
    }
}
