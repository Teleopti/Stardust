using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class MinMaxTest
    {
        private MinMax<int> target;

        [SetUp]
        public void Setup()
        {
            target = new MinMax<int>(3, 5);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldHandleToString()
		{
			new MinMax<int>(3, 4).ToString().Should().Be("3-4");
		}

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(3, target.Minimum);
            Assert.AreEqual(5, target.Maximum);
        }

        [Test]
        public void VerifyMinimumValueIsNotTooHigh()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MinMax<int>(7, 6));
        }

        [Test]
        public void VerifyEqualsWorks()
        {
            MinMax<int> target2 = new MinMax<int>(3, 5);
            Assert.IsTrue(target == target2);
            Assert.IsFalse(target != target2);
            Assert.IsTrue(target.Equals(target2));
            Assert.IsTrue(target.Equals((object)target2));
            Assert.IsFalse(target.Equals(4));
            Assert.IsFalse(target.Equals(null));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            MinMax<int> target2 = new MinMax<int>(3, 5);
            Assert.AreEqual(target.GetHashCode(), target2.GetHashCode());
        }


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyContainsWork()
        {
            MinMax<int> target2 = new MinMax<int>(3, 5);
			Assert.IsFalse(target2.Contains(1));
			Assert.IsTrue(target2.Contains(3));
			Assert.IsTrue(target2.Contains(5));
            Assert.IsFalse(target2.Contains(7));
        }

		[Test]
		public void ShouldBeTwoDiffentMinMax()
		{
			MinMax<int> target1 = new MinMax<int>(1, 1);
			MinMax<int> target2 = new MinMax<int>(2, 2);

			Assert.That(target1.GetHashCode(), Is.Not.EqualTo(target2.GetHashCode()));
		}
    }
}
