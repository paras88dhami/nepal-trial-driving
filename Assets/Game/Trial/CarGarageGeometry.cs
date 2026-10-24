using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Checks all eight vehicle box corners against an oriented parking-region box.</summary>
    public static class CarGarageGeometry
    {
        public static bool VehicleFullyInside(BoxCollider parkingRegion, BoxCollider vehicleBody, float tolerance = 0.001f)
        {
            if (parkingRegion == null || vehicleBody == null) return false;
            Vector3 half = vehicleBody.size * 0.5f;
            Vector3 zoneHalf = parkingRegion.size * 0.5f;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 localCorner = vehicleBody.center +
                    new Vector3(half.x * x, half.y * y, half.z * z);
                Vector3 world = vehicleBody.transform.TransformPoint(localCorner);
                Vector3 region = parkingRegion.transform.InverseTransformPoint(world) - parkingRegion.center;
                if (Mathf.Abs(region.x) > zoneHalf.x + tolerance ||
                    Mathf.Abs(region.y) > zoneHalf.y + tolerance ||
                    Mathf.Abs(region.z) > zoneHalf.z + tolerance)
                    return false;
            }
            return true;
        }
    }
}
