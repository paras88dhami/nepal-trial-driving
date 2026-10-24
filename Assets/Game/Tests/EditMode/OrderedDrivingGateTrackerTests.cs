using NUnit.Framework;
using NepalTrial.Trial;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public sealed class OrderedDrivingGateTrackerTests
    {
        [Test]
        public void OrderedDirectionalCrossingsAreMandatory()
        {
            var tracker = new OrderedDrivingGateTracker(
                new[] { new Vector3(0, 0, 0), new Vector3(0, 0, 10) },
                new[] { Vector3.forward, Vector3.forward }, 2f);
            Assert.IsFalse(tracker.Sample(new Vector3(0,0,9), new Vector3(0,0,11)));
            Assert.AreEqual(0, tracker.NextIndex);
            Assert.IsFalse(tracker.Sample(new Vector3(0,0,1), new Vector3(0,0,-1)));
            Assert.IsTrue(tracker.Sample(new Vector3(0,0,-1), new Vector3(0,0,1)));
            Assert.AreEqual(1, tracker.NextIndex);
            Assert.IsTrue(tracker.Sample(new Vector3(0,0,9), new Vector3(0,0,11)));
            Assert.IsTrue(tracker.Finished);
            Assert.IsFalse(tracker.Sample(new Vector3(0,0,-1), new Vector3(0,0,11)));
            tracker.Reset();
            Assert.AreEqual(0, tracker.NextIndex);
        }

        [Test]
        public void CrossingOutsideGateWidthIsRejected()
        {
            var tracker = new OrderedDrivingGateTracker(new[] { Vector3.zero },
                new[] { Vector3.forward }, 2f);
            Assert.IsFalse(tracker.Sample(new Vector3(8,0,-1), new Vector3(8,0,1)));
            Assert.IsTrue(tracker.Sample(new Vector3(1,0,-1), new Vector3(1,0,1)));
        }
    }
}
