using NUnit.Framework;
using NepalTrial.Flow;
using NepalTrial.Progress;

namespace NepalTrial.Tests.EditMode
{
    public sealed class Phase4GameFlowTests
    {
        [Test]
        public void GaragePracticeToDrivingHasValidTransitions()
        {
            var flow=new GameFlowState();
            var data=TrialProgressBootstrap.EnsureStarterVehicles(new LocalProgressData());
            flow.SetAvailableVehicles(data.unlockedVehicleIds);
            Assert.IsTrue(flow.OpenVehicleSelect());
            Assert.IsTrue(flow.SelectVehicle(TrialCatalog.StarterScooter));
            Assert.IsTrue(flow.SelectMode(DriveMode.Practice));
            Assert.IsTrue(flow.StartCourse("narrow-plank"));
            Assert.AreEqual(GameScreen.Driving,flow.Screen);
            Assert.AreEqual(TrialCatalog.StarterScooter,flow.VehicleId);
            Assert.AreEqual("narrow-plank",flow.CourseId);
        }

        [Test]
        public void InvalidCourseCannotBeCreatedForWrongVehicle()
        {
            Assert.Throws<System.ArgumentException>(() =>
                TrialSessionRules.Create(TrialCatalog.StarterCar,DriveMode.Practice,"narrow-plank"));
            Assert.Throws<System.ArgumentException>(() =>
                TrialSessionRules.Create("no-vehicle",DriveMode.Exam));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                TrialSessionRules.Create(TrialCatalog.StarterCar,(DriveMode)99));
        }
    }
}
