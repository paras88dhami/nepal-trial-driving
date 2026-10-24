using System;

namespace NepalTrial.Trial
{
    /// <summary>Deterministic timer. Tick with unscaled or scaled delta from the hosting mode.</summary>
    public sealed class CountdownClock
    {
        public float RemainingSeconds { get; private set; }
        public bool IsRunning { get; private set; }
        public bool HasExpired => RemainingSeconds <= 0f;
        public CountdownClock(float durationSeconds) => Reset(durationSeconds);
        public void Reset(float durationSeconds)
        {
            if (durationSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            RemainingSeconds = durationSeconds;
            IsRunning = false;
        }
        public bool Start()
        {
            if (HasExpired || IsRunning) return false;
            IsRunning = true;
            return true;
        }
        public void Pause() => IsRunning = false;
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (!IsRunning) return;
            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaSeconds);
            if (HasExpired) IsRunning = false;
        }
    }
}
