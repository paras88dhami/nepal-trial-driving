using NUnit.Framework;
using NepalTrial.Progress;
using NepalTrial.Trial;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public class GameplayProgressTests
    {
        [Test]
        public void GateOnlyCountsForwardCrossingWithinWidth()
        {
            Vector3 center = Vector3.zero;
            Assert.IsTrue(DirectionalCheckpointGate.Crossed(new Vector3(0,0,-1), new Vector3(0,0,1), center, Vector3.forward, 2f));
            Assert.IsFalse(DirectionalCheckpointGate.Crossed(new Vector3(0,0,1), new Vector3(0,0,-1), center, Vector3.forward, 2f));
            Assert.IsFalse(DirectionalCheckpointGate.Crossed(new Vector3(5,0,-1), new Vector3(5,0,1), center, Vector3.forward, 2f));
            Assert.IsFalse(DirectionalCheckpointGate.Crossed(new Vector3(0,0,-1), new Vector3(0,0,-0.2f), center, Vector3.forward, 2f));
        }

        [Test]
        public void ProgressTracksAttemptsScoresAndUnlocks()
        {
            var data = new LocalProgressData();
            ProgressRules.RecordAttempt(data, "figure-eight", 65, false);
            ProgressRules.RecordAttempt(data, "figure-eight", 80, true);
            ProgressRules.RecordAttempt(data, "figure-eight", 70, true);
            Assert.AreEqual(3, data.records[0].attempts);
            Assert.AreEqual(2, data.records[0].completions);
            Assert.AreEqual(80, data.records[0].bestScore);
            Assert.IsTrue(ProgressRules.UnlockVehicle(data, "car-two"));
            Assert.IsFalse(ProgressRules.UnlockVehicle(data, "car-two"));
        }

        [Test]
        public void ProgressCanRoundTripViaUnityJsonUtility()
        {
            var data = new LocalProgressData();
            ProgressRules.RecordAttempt(data, "u-turn", 90, true);
            var restored = JsonUtility.FromJson<LocalProgressData>(JsonUtility.ToJson(data));
            Assert.AreEqual("starter-car", restored.selectedVehicleId);
            Assert.AreEqual("u-turn", restored.records[0].courseId);
        }
    }
}
