using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class StartTimeLimitationTest
    {
        private StartTimeLimitation target;

        [SetUp]
        public void Setup()
        {
            target = new StartTimeLimitation();

        }

        [Test]
        public void VerifyHashCode()
        {
            Assert.AreEqual(0, target.GetHashCode());
        }

        [Test]
        public void VerifyOperators()
        {
            Assert.IsFalse(target != new StartTimeLimitation());
            Assert.IsFalse(target == new StartTimeLimitation(new TimeSpan(1, 1, 1),null));
        }

        [Test]
        public void VerifyEquals()
        {
            Assert.IsTrue(target.Equals(new StartTimeLimitation()));
            Assert.IsFalse(target.Equals(new EndTimeLimitation()));
            Assert.IsTrue(target.Equals((object)new StartTimeLimitation()));
        }

        [Test]
        public void VerifyStartAndEnd()
        {
            Assert.IsNull(target.StartTime);
            Assert.IsNull(target.EndTime);
            target = new StartTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(10, 0, 0));

            Assert.IsNotNull(target.StartTime);
            Assert.IsNotNull(target.EndTime);
            Assert.IsNotNull(target.EndTimeString);
            Assert.IsNotNull(target.StartTimeString);
        }

        [Test, SetCulture("sv-SE")]
        public void ShouldHandleStartAndEndInSwedishCulture()
        {
            target = new StartTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(10, 0, 0));

            Assert.AreEqual("10:00", target.EndTimeString);
            Assert.AreEqual("05:00", target.StartTimeString);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyStartToBig()
        {
            target = new StartTimeLimitation(new TimeSpan(1, 0, 0, 0), null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyEndToBig()
        {
			  target = new StartTimeLimitation(null, new TimeSpan(1, 0, 0, 0));
        }

        [Test]
        public void VerifyEndCannotBeBiggerThanStart()
        {
        	Assert.Throws<ArgumentOutOfRangeException>(
        		() => new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(7, 0, 0)));
        }

        [Test]
        public void VerifyHasValue()
        {
            new StartTimeLimitation(null, null).HasValue().Should().Be.False();
            new StartTimeLimitation(TimeSpan.FromHours(1), null).HasValue().Should().Be.True();
				new StartTimeLimitation(null, TimeSpan.FromHours(1)).HasValue().Should().Be.True();
				new StartTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(1)).HasValue().Should().Be.True();
        }


		 [Test]
		 public void ShouldBeValidForTimeSpanWithinDayWhenBothNull()
		 {
		 	target.IsValidFor(TimeSpan.FromHours(12)).Should().Be.True();
		 }

		 [Test]
		 public void ShouldBeInvalidForTimeSpanOutsideDayWhenBothNull()
		 {
		 	target.IsValidFor(TimeSpan.FromHours(25)).Should().Be.False();
		 }

		 [Test]
		 public void ShouldBeValidForTimeSpanAfterStartTime()
		 {
		 	target = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			 target.IsValidFor(TimeSpan.FromHours(9)).Should().Be.True();
		 }

		 [Test]
		 public void ShouldBeInvalidForTimeSpanBeforeStartTime()
		 {
			 target = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			 target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.False();
		 }

		 [Test]
		 public void ShouldBeValidForTimeSpanBeforeEndTime()
		 {
			 target = new StartTimeLimitation(null, TimeSpan.FromHours(8));
			 target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.True();
		 }

		 [Test]
		 public void ShouldBeValidForTimeSpanAtEndTime()
		 {
			 target = new StartTimeLimitation(null, TimeSpan.FromHours(8));
			 target.IsValidFor(TimeSpan.FromHours(8)).Should().Be.True();
		 }

		 [Test]
		 public void ShouldBeInvalidForTimeSpanAfterEndTime()
		 {
			 target = new StartTimeLimitation(null, TimeSpan.FromHours(8));
			 target.IsValidFor(TimeSpan.FromHours(9)).Should().Be.False();
		 }


		 [Test]
		 public void ShouldBeInvalidForTimeSpanBeforePeriod()
		 {
			 target = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
		 	target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.False();
		 }

		 [Test]
		 public void ShouldBeInvalidForTimeSpanAfterPeriod()
		 {
			 target = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
			 target.IsValidFor(TimeSpan.FromHours(11)).Should().Be.False();
		 }

		 [Test]
		 public void ShouldToString()
		 {
			 target = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
			 target.ToString().Should().Be.EqualTo(target.StartTimeString + "-" + target.EndTimeString);
		 }
    }
}
