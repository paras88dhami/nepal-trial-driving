using NUnit.Framework;
using NepalTrial.Trial;
using NepalTrial.Vehicles.Car;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CoreGameplayLogicTests
    {
        [Test]
        public void WholeCarBoundaryCountsOneIncidentUntilAllWheelsReturn()
        {
            var counter = new BoundaryIncidentCounter();
            Assert.IsFalse(counter.Update(false));
            Assert.IsTrue(counter.Update(true));
            Assert.IsFalse(counter.Update(true));
            Assert.AreEqual(1, counter.IncidentCount);
            Assert.IsFalse(counter.Update(false));
            Assert.IsTrue(counter.Update(true));
            Assert.AreEqual(2, counter.IncidentCount);
            counter.Reset();
            Assert.AreEqual(0, counter.IncidentCount);
        }

        [Test]
        public void ExamRequiresStartAndPassesOnThreshold()
        {
            var session = new ExamSession(100, 70);
            Assert.IsFalse(session.ApplyPenalty(5));
            Assert.IsTrue(session.Start());
            Assert.IsFalse(session.Start());
            Assert.IsTrue(session.ApplyPenalty(30));
            Assert.AreEqual(70, session.Score);
            Assert.IsTrue(session.Finish());
            Assert.AreEqual(ExamStage.Passed, session.Stage);
            Assert.IsFalse(session.ApplyPenalty(5));
            session.Reset();
            Assert.AreEqual(ExamStage.Ready, session.Stage);
        }

        [Test]
        public void ExamFailsOnThresholdOrImmediateFailure()
        {
            var a = new ExamSession(100, 70);
            a.Start();
            a.ApplyPenalty(31);
            Assert.AreEqual(ExamStage.Failed, a.Stage);
            var b = new ExamSession(100, 70);
            b.Start();
            b.ApplyPenalty(0, true);
            Assert.AreEqual(ExamStage.Failed, b.Stage);
        }

        [Test]
        public void GearInterlocksPreventUnsafeReverseAndPark()
        {
            var state = new TransmissionState();
            Assert.IsFalse(state.TrySelect(DriveGear.Drive, 0f, false));
            Assert.IsTrue(state.TrySelect(DriveGear.Drive, 0f, true));
            state.SetHandbrake(false);
            Assert.AreEqual(0.5f, state.SignedThrottle(.5f));
            Assert.IsFalse(state.TrySelect(DriveGear.Reverse, 2f, true));
            Assert.IsTrue(state.TrySelect(DriveGear.Reverse, 0f, true));
            Assert.AreEqual(-.5f, state.SignedThrottle(.5f));
            state.SetHandbrake(true);
            Assert.AreEqual(0f, state.SignedThrottle(1f));
        }

        [Test]
        public void CountdownPausesAndStopsAtZero()
        {
            var timer = new CountdownClock(10f);
            timer.Tick(5f);
            Assert.AreEqual(10f, timer.RemainingSeconds);
            timer.Start();
            timer.Tick(3f);
            timer.Pause();
            timer.Tick(2f);
            Assert.AreEqual(7f, timer.RemainingSeconds);
            timer.Start();
            timer.Tick(20f);
            Assert.IsTrue(timer.HasExpired);
            Assert.AreEqual(0f, timer.RemainingSeconds);
            Assert.IsFalse(timer.IsRunning);
        }
    }
}
