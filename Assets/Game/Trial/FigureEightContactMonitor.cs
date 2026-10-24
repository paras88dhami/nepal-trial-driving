using System;
using UnityEngine;

namespace NepalTrial.Trial
{
    public enum FigureEightContactEvent { None, LineTouch, Outside }

    /// <summary>
    /// Classifies tire contacts in the UNION of two annular rings. A touch is
    /// edge proximity; crossing substantially beyond the painted line fails.
    /// One event is emitted per excursion, not once per physics frame or tire.
    /// </summary>
    public sealed class FigureEightContactMonitor
    {
        private readonly float inner, outer, touchMeters, exitMeters;
        private bool touching, outside;

        public FigureEightContactMonitor(float middleRadius,float halfWidth,
            float touchMeters=.10f,float exitMeters=.20f)
        {
            if(middleRadius<=halfWidth || halfWidth<=0f || touchMeters<=0f || exitMeters<=0f)
                throw new ArgumentOutOfRangeException();
            inner=middleRadius-halfWidth;
            outer=middleRadius+halfWidth;
            this.touchMeters=touchMeters;
            this.exitMeters=exitMeters;
        }

        public FigureEightContactEvent Sample(Vector2[] contactPoints,Vector2 first,Vector2 second)
        {
            if(contactPoints==null || contactPoints.Length==0)return FigureEightContactEvent.None;
            bool anyTouch=false, anyOutside=false;
            foreach(Vector2 p in contactPoints)
            {
                float a=DistanceFromValidBand(Vector2.Distance(p,first));
                float b=DistanceFromValidBand(Vector2.Distance(p,second));
                if(Mathf.Min(a,b)>exitMeters) anyOutside=true;
                // A painted line is touched only while the tire is still
                // sufficiently near either valid lane; avoid double charging
                // after the wheel has already left the ground markings.
                if (Mathf.Min(a,b)<=exitMeters &&
                    (NearEdge(Vector2.Distance(p,first)) ||
                     NearEdge(Vector2.Distance(p,second))))
                    anyTouch=true;
            }
            bool newOutside=anyOutside && !outside;
            bool newTouch=anyTouch && !touching;
            outside=anyOutside;
            touching=anyTouch;
            if(newOutside)return FigureEightContactEvent.Outside;
            if(newTouch)return FigureEightContactEvent.LineTouch;
            return FigureEightContactEvent.None;
        }

        private float DistanceFromValidBand(float radius)
        {
            return radius<inner ? inner-radius : radius>outer ? radius-outer : 0f;
        }
        private bool NearEdge(float radius)
        {
            return Mathf.Abs(radius-inner)<=touchMeters ||
                   Mathf.Abs(radius-outer)<=touchMeters;
        }
        public void Reset(){touching=false;outside=false;}
    }
}
