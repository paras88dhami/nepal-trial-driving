using NUnit.Framework;
using NepalTrial.Trial;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public sealed class TrialRuleMathTests
    {
        [Test]
        public void AnnulusChecksInsideAndOutside()
        {
            Assert.IsTrue(FigureEightTrackMath.IsWithinAnnulus(new Vector2(5f, 0f), Vector2.zero, 5f, .5f));
            Assert.IsFalse(FigureEightTrackMath.IsWithinAnnulus(Vector2.zero, Vector2.zero, 5f, .5f));
        }

        [Test]
        public void CrossingMustGoForward()
        {
            Assert.IsTrue(FigureEightTrackMath.HasCrossedPlane(new Vector3(-1,0,0), new Vector3(1,0,0), Vector3.zero, Vector3.right));
            Assert.IsFalse(FigureEightTrackMath.HasCrossedPlane(new Vector3(1,0,0), new Vector3(-1,0,0), Vector3.zero, Vector3.right));
        }

        [Test]
        public void BoundaryContactIsDeduplicatedUntilExit()
        {
            var state = new TrialBoundaryState();
            Assert.IsTrue(state.Enter("front-left", "outer-line"));
            Assert.IsFalse(state.Enter("front-left", "outer-line"));
            Assert.AreEqual(1, state.ViolationCount);
            state.Exit("front-left", "outer-line");
            Assert.IsTrue(state.Enter("front-left", "outer-line"));
            Assert.AreEqual(2, state.ViolationCount);
        }
    }
}
