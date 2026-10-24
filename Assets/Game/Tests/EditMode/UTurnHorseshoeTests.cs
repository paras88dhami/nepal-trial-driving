using NepalTrial.Trial;
using NUnit.Framework;
using UnityEngine;

namespace NepalTrial.Tests.EditMode
{
    public sealed class UTurnHorseshoeTests
    {
        [Test]
        public void BothStraightLegsAreInsideTheCourse()
        {
            var center=new Vector2(0,8);
            Assert.IsTrue(UTurnHorseshoeMath.IsInside(new Vector2(-4,-5),center,
                2.8f,5.2f,-11));
            Assert.IsTrue(UTurnHorseshoeMath.IsInside(new Vector2(4,-5),center,
                2.8f,5.2f,-11));
        }

        [Test]
        public void CenterGrassAndFarOutsideCauseViolations()
        {
            var center=new Vector2(0,8);
            Assert.IsFalse(UTurnHorseshoeMath.IsInside(new Vector2(0,-5),center,
                2.8f,5.2f,-11));
            Assert.IsFalse(UTurnHorseshoeMath.IsInside(new Vector2(6,-5),center,
                2.8f,5.2f,-11));
            Assert.IsFalse(UTurnHorseshoeMath.IsInside(new Vector2(-4,-13),center,
                2.8f,5.2f,-11));
        }

        [Test]
        public void TurnApexOnlyAllowsLaneBetweenInnerOuterRadii()
        {
            var center=new Vector2(0,8);
            Assert.IsTrue(UTurnHorseshoeMath.IsInside(new Vector2(0,12),center,
                2.8f,5.2f,-11));
            Assert.IsFalse(UTurnHorseshoeMath.IsInside(new Vector2(0,9),center,
                2.8f,5.2f,-11));
            Assert.IsFalse(UTurnHorseshoeMath.IsInside(new Vector2(0,14),center,
                2.8f,5.2f,-11));
        }
    }
}
