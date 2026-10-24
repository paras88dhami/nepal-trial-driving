using NUnit.Framework;
using NepalTrial.Trial;

namespace NepalTrial.Tests.EditMode
{
    public class TwoWheelerTrialRulesTests
    {
        [Test]
        public void PlankMustRemainInBoundsUntilTravelComplete()
        {
            var rule = new NarrowPlankValidator(5f, 20f);
            rule.Begin();
            Assert.AreEqual(CourseEventKind.None, rule.Sample(2f, 2f, true, false).Kind);
            Assert.AreEqual("plank-complete", rule.Sample(3f, 2f, true, false).Code);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(2f, 1f, false, false).Kind);
        }
        [Test]
        public void PlankRejectsWheelExitAndFootDown()
        {
            var rule = new NarrowPlankValidator(5f, 20f);
            rule.Begin();
            Assert.AreEqual("plank-off", rule.Sample(1f, 1f, false, false).Code);
            rule.Begin();
            Assert.AreEqual("plank-foot-down", rule.Sample(1f, 1f, true, true).Code);
        }
        [Test]
        public void PlankTimesOut()
        {
            var rule = new NarrowPlankValidator(5f, 2f);
            rule.Begin();
            Assert.AreEqual("plank-timeout", rule.Sample(1f, 3f, true, false).Code);
        }
        [Test]
        public void FallTriggersOnlyOnceUntilReset()
        {
            var rule = new TwoWheelerBalanceValidator(55f);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(40f).Kind);
            Assert.AreEqual("two-wheeler-fall", rule.Sample(-60f).Code);
            Assert.AreEqual(CourseEventKind.None, rule.Sample(70f).Kind);
            rule.Reset();
            Assert.AreEqual("two-wheeler-fall", rule.Sample(60f).Code);
        }
    }
}
