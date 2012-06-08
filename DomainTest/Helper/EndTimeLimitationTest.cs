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
            Assert.IsFalse(target == new EndTimeLimitation(new TimeSpan(1,1,1), new TimeSpan()));
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

            target.StartTimeString = "";
            Assert.IsNull(target.StartTime);
            target.EndTimeString = "";
            Assert.IsNull(target.EndTime);
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
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyEndNotTooBig()
        {
            target.EndTimeString = "25:00";
        }

        [Test]
        public void VerifyEnterTwentyFour()
        {
            target.EndTimeString = "24:15";
            Assert.AreEqual(new TimeSpan(1,0,0,0),target.EndTime);
            target.StartTimeString = "24:15";
            Assert.AreEqual(new TimeSpan(1, 0, 0, 0), target.StartTime);
        }

        [Test, SetCulture("sv-SE")]
        public void ShouldHandleTwentyFourInSwedishCulture()
        {
            target.EndTimeString = "24:15";
            target.StartTimeString = "24:15";

            Assert.AreEqual("00:00 +1", target.EndTimeString);
        }

        [Test]
        public void VerifyEndCannotBeBiggerThanStart()
        {
            target.EndTime = new TimeSpan(7, 0, 0);
			Assert.Throws<ArgumentOutOfRangeException>(delegate { target.StartTime = new TimeSpan(8, 0, 0); });

			target.StartTime = new TimeSpan(6, 0, 0);
			Assert.Throws<ArgumentOutOfRangeException>(delegate { target.EndTime = new TimeSpan(5, 0, 0); });
        }

        [Test]
        public void VerifySetStartDateWithString()
        {
            target.StartTimeString = "5 AM";
            Assert.AreEqual(new TimeSpan(5, 0, 0), target.StartTime);
            target.StartTimeString = "5 PM";
            Assert.AreEqual(new TimeSpan(17, 0, 0), target.StartTime);
            target.StartTimeString = "1:00";
            Assert.AreEqual(new TimeSpan(1, 0, 0), target.StartTime);
            target.StartTimeString = "16:33";
            Assert.AreEqual(new TimeSpan(16, 33, 0), target.StartTime);
            target.StartTimeString = "1:16:33";
            Assert.AreEqual(new TimeSpan(1, 16, 33), target.StartTime);
            target.StartTimeString = "1.01:16:33";
            Assert.AreEqual(new TimeSpan(1,1, 16, 33), target.StartTime);

            target.StartTimeString = "10:16 +1";
            Assert.AreEqual(new TimeSpan(1, 10, 16, 0), target.StartTime);

            target.StartTimeString = "10:16 AM +1";
            Assert.AreEqual(new TimeSpan(1, 10, 16, 0), target.StartTime);
        }

        [Test]
        public void VerifySetEndDateWithString()
        {
            target.EndTimeString = "5 AM";
            Assert.AreEqual(new TimeSpan(5, 0, 0), target.EndTime);
            target.EndTimeString = "5 PM";
            Assert.AreEqual(new TimeSpan(17, 0, 0), target.EndTime);
            target.EndTimeString = "1:00";
            Assert.AreEqual(new TimeSpan(1, 0, 0), target.EndTime);
            target.EndTimeString = "16:33";
            Assert.AreEqual(new TimeSpan(16, 33, 0), target.EndTime);
            target.EndTimeString = "1:16:33";
            Assert.AreEqual(new TimeSpan(1, 16, 33), target.EndTime);
            target.EndTimeString = "1.01:16:33";
            Assert.AreEqual(new TimeSpan(1, 1, 16, 33), target.EndTime);

            target.EndTimeString = "01:16:33 +1";
            Assert.AreEqual(new TimeSpan(1, 1, 16, 33), target.EndTime);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyExceptionStartTime()
        {
            target.StartTimeString = "öalsfaslö";
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyExceptionEndTime()
        {
            target.EndTimeString = "lajfia ";
        }

        [Test]
        public void VerifyHasValue()
        {
            target.StartTime = null;
            target.EndTime = null;
            Assert.IsFalse(target.HasValue());
            target.StartTime = TimeSpan.FromHours(1);
            Assert.IsTrue(target.HasValue());
            target.EndTime = TimeSpan.FromHours(2);
            Assert.IsTrue(target.HasValue());
            target.StartTime = null;
            Assert.IsTrue(target.HasValue());
        }



		  [Test]
		  public void ShouldBeValidForTimespanWithinPeriodWhenBothNull()
		  {
			  target.IsValidFor(TimeSpan.FromHours(25)).Should().Be.True();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimespanOutsideDayWhenBothNull()
		  {
			  target.IsValidFor(TimeSpan.FromHours(49)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldBeValidForTimespanAfterStartTime()
		  {
			  target.StartTime = TimeSpan.FromHours(8);
			  target.IsValidFor(TimeSpan.FromHours(9)).Should().Be.True();
		  }

		  [Test]
		  public void ShouldBeInValidForTimespanBeforeStartTime()
		  {
			  target.StartTime = TimeSpan.FromHours(8);
			  target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldBeValidForTimespanBeforeEndtime()
		  {
			  target.EndTime = TimeSpan.FromHours(8);
			  target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.True();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimespanAfterEndtime()
		  {
			  target.EndTime = TimeSpan.FromHours(8);
			  target.IsValidFor(TimeSpan.FromHours(9)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimepanBeforePeriod()
		  {
			  target.StartTime = TimeSpan.FromHours(8);
			  target.EndTime = TimeSpan.FromHours(10);
			  target.IsValidFor(TimeSpan.FromHours(7)).Should().Be.False();
		  }

		  [Test]
		  public void ShouldBeInvalidForTimepanAfterPeriod()
		  {
			  target.StartTime = TimeSpan.FromHours(8);
			  target.EndTime = TimeSpan.FromHours(10);
			  target.IsValidFor(TimeSpan.FromHours(11)).Should().Be.False();
		  }
    }
}
