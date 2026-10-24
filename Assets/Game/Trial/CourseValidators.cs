using System;
using UnityEngine;

namespace NepalTrial.Trial
{
    public enum CourseEventKind { None, Completed, Violation }

    public struct CourseEvent
    {
        public CourseEventKind Kind;
        public string Code;
        public CourseEvent(CourseEventKind kind, string code) { Kind = kind; Code = code; }
    }

    /// <summary>Configurable provisional trial validators. These are NOT official Nepal licensing rules.</summary>
    public sealed class UTurnValidator
    {
        private readonly float minimumTurnDegrees;
        private readonly float maximumElapsedSeconds;
        private float accumulatedTurn;
        private float elapsed;
        private bool started;
        private bool finished;
        public UTurnValidator(float minimumTurnDegrees, float maximumElapsedSeconds)
        {
            if (minimumTurnDegrees <= 0 || maximumElapsedSeconds <= 0) throw new ArgumentOutOfRangeException();
            this.minimumTurnDegrees = minimumTurnDegrees;
            this.maximumElapsedSeconds = maximumElapsedSeconds;
        }
        public void Begin() { started = true; finished = false; accumulatedTurn = 0; elapsed = 0; }
        public CourseEvent Sample(float headingDeltaDegrees, float deltaSeconds, bool outsideBoundary, bool exitGateCrossed)
        {
            if (!started || finished) return new CourseEvent();
            if (deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            elapsed += deltaSeconds;
            // Require a net turn rather than rewarding left/right steering oscillation.
            accumulatedTurn += Mathf.DeltaAngle(0f, headingDeltaDegrees);
            if (outsideBoundary) { finished = true; return new CourseEvent(CourseEventKind.Violation, "u-turn-boundary"); }
            if (elapsed > maximumElapsedSeconds) { finished = true; return new CourseEvent(CourseEventKind.Violation, "u-turn-timeout"); }
            if (exitGateCrossed)
            {
                finished = true;
                return Mathf.Abs(accumulatedTurn) >= minimumTurnDegrees
                    ? new CourseEvent(CourseEventKind.Completed, "u-turn-complete")
                    : new CourseEvent(CourseEventKind.Violation, "u-turn-incomplete");
            }
            return new CourseEvent();
        }
    }

    public sealed class HillStartValidator
    {
        private readonly float maxRollbackMeters;
        private readonly float maximumWaitSeconds;
        private float initialDistance;
        private float maximumDistance;
        private float wait;
        private bool started;
        private bool complete;
        private bool descending;
        public HillStartValidator(float maxRollbackMeters, float maximumWaitSeconds)
        {
            if (maxRollbackMeters < 0 || maximumWaitSeconds <= 0) throw new ArgumentOutOfRangeException();
            this.maxRollbackMeters = maxRollbackMeters;
            this.maximumWaitSeconds = maximumWaitSeconds;
        }
        /// <param name="signedDistanceAlongHill">Distance along the upward direction, in meters.</param>
        public void Begin(float signedDistanceAlongHill)
        {
            initialDistance = signedDistanceAlongHill;
            maximumDistance = signedDistanceAlongHill;
            wait = 0; started = true; complete = false; descending=false;
        }
        public CourseEvent Sample(float signedDistanceAlongHill, float deltaSeconds, bool reachedExit, bool descentStarted=false)
        {
            if (!started || complete) return new CourseEvent();
            if (deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            wait += deltaSeconds;
            descending |= descentStarted;
            if (!descending)
            {
                maximumDistance = Mathf.Max(maximumDistance, signedDistanceAlongHill);
                if (maximumDistance - signedDistanceAlongHill > maxRollbackMeters)
                { complete = true; return new CourseEvent(CourseEventKind.Violation, "hill-rollback"); }
            }
            if (wait > maximumWaitSeconds)
            { complete = true; return new CourseEvent(CourseEventKind.Violation, "hill-timeout"); }
            if (reachedExit)
            { complete = true; return new CourseEvent(CourseEventKind.Completed, "hill-complete"); }
            return new CourseEvent();
        }
    }

    public enum TrafficLightColor { Red, Yellow, Green }
    public sealed class TrafficLightValidator
    {
        private bool previouslyAcross;
        public void Reset() => previouslyAcross = false;
        public CourseEvent Sample(bool acrossStopLine, TrafficLightColor light)
        {
            bool newlyCrossed = acrossStopLine && !previouslyAcross;
            previouslyAcross = acrossStopLine;
            if (!newlyCrossed) return new CourseEvent();
            return light == TrafficLightColor.Green
                ? new CourseEvent(CourseEventKind.Completed, "signal-go")
                : new CourseEvent(CourseEventKind.Violation, light == TrafficLightColor.Red ? "signal-red" : "signal-yellow");
        }
    }

    public sealed class SpeedBreakerValidator
    {
        private readonly float maxCrossingSpeedKph;
        private bool crossing;
        public SpeedBreakerValidator(float maxCrossingSpeedKph)
        {
            if (maxCrossingSpeedKph <= 0) throw new ArgumentOutOfRangeException(nameof(maxCrossingSpeedKph));
            this.maxCrossingSpeedKph = maxCrossingSpeedKph;
        }
        public CourseEvent Sample(bool overBreaker, float speedKph)
        {
            bool entry = overBreaker && !crossing;
            crossing = overBreaker;
            if (!entry) return new CourseEvent();
            return speedKph <= maxCrossingSpeedKph
                ? new CourseEvent(CourseEventKind.Completed, "breaker-safe")
                : new CourseEvent(CourseEventKind.Violation, "breaker-speed");
        }
        public void Reset() => crossing = false;
    }

    public sealed class GarageParkingValidator
    {
        private readonly float requiredHoldSeconds;
        private float heldSeconds;
        private bool complete;
        public GarageParkingValidator(float requiredHoldSeconds)
        {
            if (requiredHoldSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(requiredHoldSeconds));
            this.requiredHoldSeconds = requiredHoldSeconds;
        }
        public CourseEvent Sample(bool allVehicleCornersInside, bool obstructionHit, float speedMetersPerSecond, float deltaSeconds)
        {
            if (complete) return new CourseEvent();
            if (deltaSeconds < 0) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (obstructionHit) { complete = true; return new CourseEvent(CourseEventKind.Violation, "garage-obstruction"); }
            heldSeconds = allVehicleCornersInside && Mathf.Abs(speedMetersPerSecond) < 0.1f ? heldSeconds + deltaSeconds : 0;
            if (heldSeconds < requiredHoldSeconds) return new CourseEvent();
            complete = true;
            return new CourseEvent(CourseEventKind.Completed, "garage-complete");
        }
        public void Reset() { heldSeconds = 0; complete = false; }
    }
}
