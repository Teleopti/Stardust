using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class TimePeriodWithSegmentTest
    {
        private TimePeriodWithSegment target;
        private TimePeriod period;
        private TimeSpan segment;

        [SetUp]
        public void Setup()
        {
            period = new TimePeriod(10, 0, 11, 0);
            segment = new TimeSpan(0, 10, 0);
            target = new TimePeriodWithSegment(period, segment);

        }

        [Test]
        public void VerifyOtherConstructor()
        {
            TimePeriodWithSegment otherTarget = new TimePeriodWithSegment(10, 0, 11, 0, 10);
            Assert.AreEqual(target, otherTarget);
        }

        [Test]
        public void VerifyPropertiesCanBeRead()
        {
            Assert.AreEqual(period, target.Period);
            Assert.AreEqual(segment, target.Segment);
        }

        [Test]
        public void VerifySegmentLengthMoreThanZero()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => target = new TimePeriodWithSegment(10,0,11,0,0));
        }

        [Test]
        public void VerifyEquals()
        {
            TimePeriodWithSegment verifier = new TimePeriodWithSegment(period, segment);
            Assert.IsTrue(target.Equals(verifier));
            Assert.IsTrue(target.Equals((object)verifier));
            Assert.IsTrue(target == verifier);
            Assert.IsFalse(target != verifier);

            verifier = new TimePeriodWithSegment(period, new TimeSpan(2));
            Assert.IsFalse(target.Equals(verifier));
            Assert.IsFalse(target.Equals((object)verifier));
            Assert.IsFalse(target == verifier);
            Assert.IsTrue(target != verifier);

            verifier = new TimePeriodWithSegment(new TimePeriod(11,0,13,0), segment);
            Assert.IsFalse(target.Equals(verifier));
            Assert.IsFalse(target.Equals((object)verifier));
            Assert.IsFalse(target == verifier);
            Assert.IsTrue(target != verifier);

            Assert.IsFalse(target.Equals(null));
            
        }

        [Test]
        public void VerifyGetHashCode()
        {
            TimePeriodWithSegment verifier = new TimePeriodWithSegment(period, segment);
            Assert.AreEqual(target.GetHashCode(), verifier.GetHashCode());
            verifier = new TimePeriodWithSegment(period, new TimeSpan(2));
            Assert.AreNotEqual(target.GetHashCode(), verifier.GetHashCode());
            verifier = new TimePeriodWithSegment(new TimePeriod(11, 0, 13, 0), segment);
            Assert.AreNotEqual(target.GetHashCode(), verifier.GetHashCode());
        }

        [Test]
        public void VerifyDoNotAllowNegativeTime()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => target = new TimePeriodWithSegment(0, -1, 1, 1, 1));
        }
    }
}