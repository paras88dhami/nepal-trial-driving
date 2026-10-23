using NUnit.Framework;
using NepalTrial.Trial;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CheckpointSequenceTests
    {
        [Test]
        public void MustReachCheckpointsInOrder()
        {
            var go = new GameObject("Sequence");
            try
            {
                var sequence = go.AddComponent<PracticeCheckpointSequence>();
                Assert.IsFalse(sequence.TryReach(2));
                Assert.IsTrue(sequence.TryReach(0));
                Assert.IsFalse(sequence.TryReach(0));
                Assert.AreEqual(1, sequence.NextCheckpoint);
                sequence.ResetSequence();
                Assert.AreEqual(0, sequence.NextCheckpoint);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
