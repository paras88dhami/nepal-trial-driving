using NUnit.Framework;
using NepalTrial.Trial;

namespace NepalTrial.Tests.EditMode
{
    public sealed class CourseValidatorsTests
    {
        [Test]
        public void UTurnRequiresTurnAndExitAndRejectsBoundary()
        {
            var rule = new UTurnValidator(140, 30);
            rule.Begin();
            Assert.AreEqual(CourseEventKind.None, rule.Sample(80, 1, false, false).Kind);
            Assert.AreEqual(CourseEventKind.Completed, rule.Sample(80, 1, false, true).Kind);
            rule.Begin();
            Assert.AreEqual("u-turn-incomplete", rule.Sample(40, 1, false, true).Code);
            rule.Begin();
            Assert.AreEqual("u-turn-boundary", rule.Sample(10, 1, true, false).Code);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(10, 1, false, true).Kind);
        }

        [Test]
        public void UTurnTimesOut()
        {
            var rule = new UTurnValidator(150, 3);
            rule.Begin();
            Assert.AreEqual("u-turn-timeout", rule.Sample(20, 4, false, false).Code);
        }

        [Test]
        public void HillStartDetectsRollbackTimeoutAndCompletion()
        {
            var rule = new HillStartValidator(.5f, 10f);
            rule.Begin(10);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(9.7f, 1, false).Kind);
            Assert.AreEqual("hill-rollback", rule.Sample(9.4f, 1, false).Code);
            rule.Begin(10);
            Assert.AreEqual("hill-timeout", rule.Sample(10, 11, false).Code);
            rule.Begin(10);
            Assert.AreEqual(CourseEventKind.Completed, rule.Sample(11, 1, true).Kind);
        }

        [Test]
        public void TrafficSignalOnlyCountsNewCrossings()
        {
            var rule = new TrafficLightValidator();
            Assert.AreEqual("signal-red", rule.Sample(true, TrafficLightColor.Red).Code);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(true, TrafficLightColor.Red).Kind);
            rule.Sample(false, TrafficLightColor.Red);
            Assert.AreEqual("signal-go", rule.Sample(true, TrafficLightColor.Green).Code);
        }

        [Test]
        public void SpeedBreakerChecksEntrySpeedOnly()
        {
            var rule = new SpeedBreakerValidator(12);
            Assert.AreEqual("breaker-speed", rule.Sample(true, 20).Code);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(true, 20).Kind);
            rule.Sample(false, 0);
            Assert.AreEqual("breaker-safe", rule.Sample(true, 6).Code);
        }

        [Test]
        public void ParkingRequiresFullHoldAndResetsIfMoving()
        {
            var rule = new GarageParkingValidator(2);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(true, false, 0, 1).Kind);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(true, false, .4f, 1).Kind);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(true, false, 0, 1).Kind);
            Assert.AreEqual("garage-complete", rule.Sample(true, false, 0, 1).Code);
            rule.Reset();
            Assert.AreEqual("garage-obstruction", rule.Sample(false, true, 0, 1).Code);
        }
    }
}
