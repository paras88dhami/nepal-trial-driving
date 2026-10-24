using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>
    /// Two straight approaches joined by a semicircular turn.
    /// Coordinates: X across the lane, Z along approach.
    /// The inner/outer radii are measured from the far-end turn center.
    /// </summary>
    public static class UTurnHorseshoeMath
    {
        public static bool IsInside(Vector2 vehicleXZ,Vector2 center,
            float innerRadius,float outerRadius,float approachStartZ,
            float tolerance=0f)
        {
            if(innerRadius<=0f || outerRadius<=innerRadius)return false;
            float x=vehicleXZ.x-center.x;
            float z=vehicleXZ.y-center.y;
            if(vehicleXZ.y < approachStartZ-tolerance)return false;
            float r= z>=0f
                ? Mathf.Sqrt(x*x+z*z)
                : Mathf.Abs(x);
            return r>=innerRadius-tolerance && r<=outerRadius+tolerance;
        }
    }
}
