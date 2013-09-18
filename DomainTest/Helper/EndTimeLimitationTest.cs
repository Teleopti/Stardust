using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class EndTimeLimitationTest
    {
        private EndTimeLimitation target;

        [SetUp]
        public void Setup()
        {
            target = new EndTimeLimitation();
        }

        [Test]
        public void VerifyHashCode()
        {
            Assert.AreEqual(1,target.GetHashCode());
        }

        [Test]
        public void VerifyOperators()
        {
            Assert.IsFalse(target != new EndTimeLimitation());
            Assert.IsFalse(target == new EndTimeLimitation(new TimeSpan(1,1,1), null));
        }
        [Test]
        public void VerifyEquals()
        {
            Assert.IsTrue(target.Equals(new EndTimeLimitation()));
            Assert.IsFalse(target.Equals(new StartTimeLimitation()));
            Assert.IsTrue(target.Equals((object)new EndTimeLimitation()));
        }

        [Test]
        public void VerifyStartAndEnd()
        {
            Assert.IsNull(target.StartTime);
            Assert.IsNull(target.EndTime);
            target = new EndTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(10, 0, 0));
            Assert.IsNotNull(target.StartTime);
            Assert.IsNotNull(target.EndTime);
            Assert.IsNotNull(target.StartTimeString);
        }

        [Test,SetCulture("sv-SE")]
        public void ShouldHaveCorrectStringRepresentationInSwedishCulture()
        {
            target = new EndTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(10, 0, 0));
            Assert.AreEqual("10:00", target.EndTimeString);
            Assert.AreEqual("05:00", target.StartTimeString);

            target = new EndTimeLimitation(new TimeSpan(1, 5, 0, 0), new TimeSpan(1, 10, 0, 0));
            Assert.AreEqual("10:00 +1", target.EndTimeString);
            Assert.AreEqual("05:00 +1", target.StartTimeString);
        }

        [Test, SetCulture("en-US")]
        public void ShouldHaveCorrectStringRepresentationInAmericanCulture()
        {
            target = new EndTimeLimitation(new TimeSpan(1, 5, 0, 0), new TimeSpan(1, 10, 0, 0));
            Assert.AreEqual("10:00 AM +1", target.EndTimeString);
            Assert.AreEqual("5:00 AM +1", target.StartTimeString);
            target = new EndTimeLimitation(new TimeSpan(1, 5, 0, 0), new TimeSpan(1, 17, 0, 0));
            Assert.AreEqual("5:00 PM +1", target.EndTimeString);
        }

        [Test]
        public void VerifyEndCannotBeBiggerThanStart()
        {
			  Assert.Throws<ArgumentOutOfRangeException>(() => target = new EndTimeLimitation(TimeSpan.FromHours(2), TimeSpan.FromHours(1)));
        }

        [Test]
        public void VerifyHasValue()
        {
				target = new EndTimeLimitation(null, null);
            Assert.IsFalse(target.HasValue());
				target = new EndTimeLimitation(TimeSpan.FromHours(1), null);
            Assert.IsTrue(target.HasValue());
				target = new EndTimeLimitation(null, TimeSpan.FromHours(1));
            Assert.IsTrue(target.HasValue());
				target = new EndTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(1));
				Assert.IsTrue(target.HasValue());
        }

		  [Test]
		  public void ShouldBeValidForTimeSpanWithinPeriodWhenBothNull()
		  {
			  target.IsValidFor(TimeSpan.FromHours(25)).Should().Be.True();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimeSpanOutsideDayWhenBothNull()
		  {
			  target.IsValidFor(TimeSpan.FromHours(49)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldBeValidForTimeSpanAfterStartTime()
		  {
			  target = new EndTimeLimitation(TimeSpan.FromHours(8), null);
			  target.IsValidFor(TimeSpan.FromHours(9)).Should().Be.True();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimeSpanBeforeStartTime()
		  {
			  target = new EndTimeLimitation(TimeSpan.FromHours(8), null);
			  target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldBeValidForTimeSpanBeforeEndTime()
		  {
			  target = new EndTimeLimitation(null, TimeSpan.FromHours(8));
			  target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.True();
		  }

		  [Test]
		  public void ShouldBeValidForTimeSpanAtEndTime()
		  {
			  target = new EndTimeLimitation(null, TimeSpan.FromHours(8));
			  target.IsValidFor(TimeSpan.FromHours(8)).Should().Be.True();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimeSpanAfterEndTime()
		  {
			  target = new EndTimeLimitation(null, TimeSpan.FromHours(8));
			  target.IsValidFor(TimeSpan.FromHours(9)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimeSpanBeforePeriod()
		  {
			  target = new EndTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
			  target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimeSpanAfterPeriod()
		  {
			  target = new EndTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
			  target.IsValidFor(TimeSpan.FromHours(11)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldToString()
		  {
			  target = new EndTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10));
			  target.ToString().Should().Be.EqualTo(target.StartTimeString + "-" + target.EndTimeString);
		  }
    }
}
