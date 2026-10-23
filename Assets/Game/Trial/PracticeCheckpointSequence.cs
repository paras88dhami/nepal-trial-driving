using System;
using UnityEngine;

namespace NepalTrial.Trial
{
    /// <summary>Ordered practice checkpoints. No official examination scoring.</summary>
    public sealed class PracticeCheckpointSequence : MonoBehaviour
    {
        [SerializeField] private int checkpointCount = 4;
        private int next;
        public int NextCheckpoint => next;
        public event Action<int> CheckpointReached;
        public event Action Completed;

        public void ResetSequence() => next = 0;

        public bool TryReach(int checkpointIndex)
        {
            if (checkpointCount <= 0 || next >= checkpointCount || checkpointIndex != next) return false;
            next++;
            CheckpointReached?.Invoke(checkpointIndex);
            if (next == checkpointCount) Completed?.Invoke();
            return true;
        }
    }
}
