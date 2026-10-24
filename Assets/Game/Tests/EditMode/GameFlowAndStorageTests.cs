using NUnit.Framework;
using NepalTrial.Flow;
using NepalTrial.Progress;
using NepalTrial.Settings;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public class GameFlowAndStorageTests
    {
        [Test]
        public void GameFlowRequiresValidSequenceAndAvailableVehicle()
        {
            var flow = new GameFlowState();
            flow.SetAvailableVehicles(new[] { "starter-car", "starter-bike" });
            Assert.IsFalse(flow.StartCourse("eight"));
            Assert.IsTrue(flow.OpenVehicleSelect());
            Assert.IsFalse(flow.SelectVehicle("locked-car"));
            Assert.IsTrue(flow.SelectVehicle("starter-car"));
            Assert.IsTrue(flow.SelectMode(DriveMode.Practice));
            Assert.IsTrue(flow.StartCourse("figure-eight"));
            Assert.IsTrue(flow.TogglePause());
            Assert.IsFalse(flow.CompleteCourse());
            Assert.IsTrue(flow.TogglePause());
            Assert.IsTrue(flow.CompleteCourse());
            Assert.IsTrue(flow.Retry());
            Assert.AreEqual(GameScreen.Driving, flow.Screen);
            flow.ReturnHome();
            Assert.AreEqual(GameScreen.Home, flow.Screen);
        }

        [Test]
        public void ProgressStoreRoundTripsAndUsesFreshKey()
        {
            string key = "NepalTrial.TestProgress." + System.Guid.NewGuid().ToString("N");
            try
            {
                var store = new LocalProgressStore(key);
                Assert.AreEqual("starter-car", store.Load().selectedVehicleId);
                var data = new LocalProgressData();
                ProgressRules.RecordAttempt(data, "figure-eight", 75, true);
                store.Save(data);
                Assert.AreEqual(75, store.Load().records[0].bestScore);
            }
            finally { PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
        }

        [Test]
        public void SettingsNormalizeRanges()
        {
            var settings = new LocalGameSettings { steeringSensitivity = 99, soundVolume = -3, graphicsQuality = 12 };
            GameSettingsRules.Normalize(settings);
            Assert.AreEqual(2f, settings.steeringSensitivity);
            Assert.AreEqual(0f, settings.soundVolume);
            Assert.AreEqual(2, settings.graphicsQuality);
        }
    }
}
