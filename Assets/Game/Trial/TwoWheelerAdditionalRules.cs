using System;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Practice ramp: stop within stop-zone, hold, move uphill, avoid rollback, reach exit.</summary>
    public sealed class TwoWheelerRampValidator
    {
        private readonly float maxRollback;
        private readonly float requiredStopSeconds;
        private readonly float timeoutSeconds;
        private float highest;
        private float held;
        private float elapsed;
        private bool started;
        private bool stopped;
        private bool finished;

        public bool HasCompletedStop => stopped;

        public TwoWheelerRampValidator(float maxRollbackMeters,float requiredStopSeconds,float timeoutSeconds)
        {
            if (maxRollbackMeters < 0f || requiredStopSeconds <= 0f || timeoutSeconds <= 0f)
                throw new ArgumentOutOfRangeException();
            maxRollback=maxRollbackMeters;
            this.requiredStopSeconds=requiredStopSeconds;
            this.timeoutSeconds=timeoutSeconds;
        }

        public void Begin(float signedUphillPosition)
        {
            highest=signedUphillPosition;
            held=0f;
            elapsed=0f;
            started=true;
            stopped=false;
            finished=false;
        }

        public CourseEvent Sample(float signedUphillPosition, bool inStopZone,
            float speedMetersPerSecond, bool footDown, bool crossedExit, float deltaSeconds)
        {
            if (!started || finished) return new CourseEvent();
            if (deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            elapsed+=deltaSeconds;
            highest=Mathf.Max(highest,signedUphillPosition);
            if (highest-signedUphillPosition > maxRollback)
                return Finish(CourseEventKind.Violation,"ramp-rollback");
            if (elapsed > timeoutSeconds)
                return Finish(CourseEventKind.Violation,"ramp-timeout");
            if (footDown) return Finish(CourseEventKind.Violation,"ramp-foot-down");
            if (!stopped)
            {
                held = inStopZone && Mathf.Abs(speedMetersPerSecond) < .2f ? held + deltaSeconds : 0f;
                if (held >= requiredStopSeconds) stopped=true;
                if (crossedExit) return Finish(CourseEventKind.Violation,"ramp-no-stop");
                return new CourseEvent();
            }
            return crossedExit ? Finish(CourseEventKind.Completed,"ramp-complete") : new CourseEvent();
        }

        private CourseEvent Finish(CourseEventKind kind,string code)
        {
            finished=true;
            return new CourseEvent(kind,code);
        }
    }

    /// <summary>Pre-ride safety checklist (behavioral segment); can be retried.</summary>
    public sealed class TwoWheelerBehavioralValidator
    {
        public bool HelmetFastened { get; private set; }
        public bool MirrorsChecked { get; private set; }
        public bool BrakesChecked { get; private set; }
        public bool TiresChecked { get; private set; }
        public bool Complete => HelmetFastened && MirrorsChecked && BrakesChecked && TiresChecked;

        public void SetHelmet(bool value) => HelmetFastened=value;
        public void SetMirrors(bool value) => MirrorsChecked=value;
        public void SetBrakes(bool value) => BrakesChecked=value;
        public void SetTires(bool value) => TiresChecked=value;

        public CourseEvent TryComplete()
        {
            return Complete
                ? new CourseEvent(CourseEventKind.Completed,"behavior-complete")
                : new CourseEvent(CourseEventKind.Violation,"behavior-incomplete");
        }

        public void Reset()
        {
            HelmetFastened=MirrorsChecked=BrakesChecked=TiresChecked=false;
        }
    }
}
