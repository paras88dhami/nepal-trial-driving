using System;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Provisional, configurable two-wheeler checks; not government-certified trial rules.</summary>
    public sealed class NarrowPlankValidator
    {
        private readonly float requiredTravelMeters;
        private readonly float maximumSeconds;
        private float travelled;
        private float elapsed;
        private bool active;
        public NarrowPlankValidator(float requiredTravelMeters, float maximumSeconds)
        {
            if (requiredTravelMeters <= 0f || maximumSeconds <= 0f) throw new ArgumentOutOfRangeException();
            this.requiredTravelMeters = requiredTravelMeters;
            this.maximumSeconds = maximumSeconds;
        }
        public void Begin() { travelled = 0; elapsed = 0; active = true; }
        public CourseEvent Sample(float forwardDistanceMeters, float deltaSeconds, bool bothWheelsOnPlank, bool footTouchedGround)
        {
            if (!active) return new CourseEvent();
            if (deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            elapsed += deltaSeconds;
            if (!bothWheelsOnPlank || footTouchedGround)
            {
                active = false;
                return new CourseEvent(CourseEventKind.Violation, footTouchedGround ? "plank-foot-down" : "plank-off");
            }
            if (elapsed > maximumSeconds)
            {
                active = false;
                return new CourseEvent(CourseEventKind.Violation, "plank-timeout");
            }
            travelled += Mathf.Max(0f, forwardDistanceMeters);
            if (travelled < requiredTravelMeters) return new CourseEvent();
            active = false;
            return new CourseEvent(CourseEventKind.Completed, "plank-complete");
        }
    }

    public sealed class TwoWheelerBalanceValidator
    {
        private readonly float fallAngleDegrees;
        private bool fallen;
        public TwoWheelerBalanceValidator(float fallAngleDegrees)
        {
            if (fallAngleDegrees <= 0f || fallAngleDegrees > 180f) throw new ArgumentOutOfRangeException(nameof(fallAngleDegrees));
            this.fallAngleDegrees = fallAngleDegrees;
        }
        public CourseEvent Sample(float leanDegrees)
        {
            if (fallen || Mathf.Abs(leanDegrees) < fallAngleDegrees) return new CourseEvent();
            fallen = true;
            return new CourseEvent(CourseEventKind.Violation, "two-wheeler-fall");
        }
        public void Reset() => fallen = false;
    }
}
