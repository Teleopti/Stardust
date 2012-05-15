﻿using System;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class WorkTimeLimitationTest
    {
        private WorkTimeLimitation target;
        [SetUp]
        public void Setup()
        {
            target = new WorkTimeLimitation();
        }

        [Test]
        public void VerifyHashCode()
        {
            Assert.AreEqual(0, target.GetHashCode());
        }

        [Test]
        public void VerifyOperators()
        {
            Assert.IsFalse(target != new WorkTimeLimitation());
            Assert.IsFalse(target == new WorkTimeLimitation(new TimeSpan(1, 1, 1), new TimeSpan()));
        }
        [Test]
        public void VerifyEquals()
        {
            Assert.IsTrue(target.Equals(new WorkTimeLimitation()));
            Assert.IsFalse(target.Equals(new StartTimeLimitation()));
            Assert.IsTrue(target.Equals((object)new WorkTimeLimitation()));
        }

        [Test]
        public void VerifyStartAndEnd()
        {
            Assert.IsNull(target.StartTime);
            Assert.IsNull(target.EndTime);
            target = new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(10, 0, 0));

            target.StartTimeString = "";
            Assert.IsNull(target.StartTime);
            target.EndTimeString = "";
            Assert.IsNull(target.EndTime);

            target = new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(10, 0, 0));

            Assert.IsNotNull(target.StartTime);
            Assert.IsNotNull(target.EndTime);
            Assert.IsNotNull(target.EndTimeString);
            Assert.IsNotNull(target.StartTimeString);
        }

        [Test, SetCulture("sv-SE")]
        public void ShouldHaveCorrectStringRepresentationInSwedishCulture()
        {
            target = new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(10, 0, 0));

            Assert.AreEqual("10:00", target.EndTimeString);
            Assert.AreEqual("5:00", target.StartTimeString);
        }

		[Test, SetCulture("en-US")]
		public void ShouldHaveCorrectStringRepresentationInAmericanCulture()
		{
			target = new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(9, 0, 0));

			Assert.AreEqual("9:00", target.EndTimeString);
			Assert.AreEqual("5:00", target.StartTimeString);
		}

		[Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyStartToBig()
        {
            target.StartTime = new TimeSpan(1, 0, 0, 0);
        }
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyStartToBigViaString()
        {
            target.StartTimeString = "1:1:16:33";
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyEndToBig()
        {
            target.EndTime = new TimeSpan(1, 0, 0, 0);
        }
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyEndToBigViaString()
        {
            target.EndTimeString = "1:1:16:33";
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
        public void VerifyCanCheckIsCorrespondingToWorkTimeLimitation()
        {
            TimeSpan timeSpanToCheck = new TimeSpan(2,0,0);
            target.StartTime = new TimeSpan(2,0,0);
            target.EndTime = new TimeSpan(6,0,0);
            Assert.IsTrue(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));

            target.StartTime = null;
            target.EndTime = new TimeSpan(2, 0, 0);
            Assert.IsTrue(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));

            target.StartTime = null;
            target.EndTime = null;
            Assert.IsTrue(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));

            target.StartTime = null;
            target.EndTime = new TimeSpan(1, 0, 0);
            Assert.IsFalse(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));

            target.StartTime = new TimeSpan(1, 0, 0);
            target.EndTime = new TimeSpan(2, 0, 0);
            Assert.IsTrue(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));

            target.StartTime = new TimeSpan(1, 0, 0);
            target.EndTime = new TimeSpan(1, 0, 0);
            Assert.IsFalse(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));

			target.EndTime = new TimeSpan(2, 0, 0);
			target.StartTime = new TimeSpan(2, 0, 0);
            Assert.IsTrue(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));

			target.EndTime = null;
			target.StartTime = new TimeSpan(3, 0, 0);
            Assert.IsFalse(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));

			target.EndTime = null;
			target.StartTime = new TimeSpan(1, 0, 0);
            Assert.IsTrue(target.IsCorrespondingToWorkTimeLimitation(timeSpanToCheck));
        }

        [Test]
        public void VerifyValidPeriod()
        {
            target.StartTime = null;
            target.EndTime = null;
            TimePeriod expected = new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(36));
            Assert.AreEqual(expected, target.ValidPeriod());

            target.StartTime = TimeSpan.FromHours(1);
            target.EndTime = null;
            expected = new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(36));
            Assert.AreEqual(expected, target.ValidPeriod());

            target.StartTime = null;
            target.EndTime = TimeSpan.FromHours(21);
            expected = new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(21));
            Assert.AreEqual(expected, target.ValidPeriod());

            target.StartTime = TimeSpan.FromHours(1);
            target.EndTime = TimeSpan.FromHours(21);
            expected = new TimePeriod(TimeSpan.FromHours(1), TimeSpan.FromHours(21));
            Assert.AreEqual(expected, target.ValidPeriod());
        }
    }
}

