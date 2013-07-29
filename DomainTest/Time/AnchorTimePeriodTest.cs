using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
    /// <summary>
    /// Tests AnchorTimePeriod struct
    /// </summary>
    [TestFixture]
    public class AnchorTimePeriodTest
    {
        /// <summary>
        /// Verifies that start time and end time are set using create.
        /// </summary>
        [Test]
        public void VerifyPropertiesAreSetUsingCreate()
        {
            TimeSpan duration = TimeSpan.FromHours(4);
            TimeSpan anchor = TimeSpan.FromHours(12);
            Percent flexibility = new Percent(0.66d);
            AnchorTimePeriod per = new AnchorTimePeriod(anchor, duration, flexibility);
            Assert.AreEqual(anchor, per.Anchor);
            Assert.AreEqual(duration, per.TargetLength);
            Assert.AreEqual(flexibility, per.Flexibility);
        }

        /// <summary>
        /// Verifies the equals method works.
        /// </summary>
        [Test]
        public void VerifyEqualsWork()
        {
            TimeSpan anchorHour = new TimeSpan(10, 0, 0);
            TimeSpan durationHours = new TimeSpan(4, 0, 0);
            Percent flexibility = new Percent(0.66d);

            AnchorTimePeriod testAnchorPeriod = new AnchorTimePeriod(anchorHour, durationHours, flexibility);

            Assert.IsTrue(testAnchorPeriod.Equals(new AnchorTimePeriod(anchorHour, durationHours, flexibility)));
            Assert.IsFalse(
                testAnchorPeriod.Equals(
                    new AnchorTimePeriod(anchorHour, durationHours, new Percent(0.67d))));
            Assert.IsFalse(new AnchorTimePeriod().Equals(null));
            Assert.AreEqual(testAnchorPeriod, testAnchorPeriod);
        }

        /// <summary>
        /// Verifies that overloaded operators work.
        /// </summary>
        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            TimeSpan anchor = TimeSpan.FromHours(10);
            TimeSpan duration = TimeSpan.FromHours(4);
            Percent flexibility = new Percent(0.66d);
            AnchorTimePeriod per = new AnchorTimePeriod(anchor, duration, flexibility);
            AnchorTimePeriod per2 = new AnchorTimePeriod();
            Assert.IsTrue(per == new AnchorTimePeriod(anchor, duration, flexibility));
            Assert.IsTrue(per != per2);
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            TimeSpan anchor = TimeSpan.FromHours(10);
            TimeSpan duration = TimeSpan.FromHours(4);
            Percent flexibility = new Percent(0.66d);
            AnchorTimePeriod per = new AnchorTimePeriod(anchor, duration, flexibility);
            IDictionary<AnchorTimePeriod, int> dic = new Dictionary<AnchorTimePeriod, int>();
            dic[per] = 5;
            Assert.AreEqual(5, dic[per]);
        }

        /// <summary>
        /// Verifies flexibility limit (less than one) works.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Domain.Time.AnchorTimePeriod"), Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyFlexibilityLimitLessThanOneWorks()
        {
            TimeSpan anchor = new TimeSpan(10, 0, 0);
            TimeSpan duration = new TimeSpan(4, 0, 0);
            Percent flexibility = new Percent(-6.66d);
            AnchorTimePeriod per = new AnchorTimePeriod(anchor, duration, flexibility);
        }

        /// <summary>
        /// Verifies flexibility limit (above hundred) works.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Domain.Time.AnchorTimePeriod"), Test]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void VerifyFlexibilityLimitAboveHundredWorks()
        {
            TimeSpan anchor = new TimeSpan(10, 0, 0);
            TimeSpan duration = new TimeSpan(4, 0, 0);
            Percent flexibility = new Percent(6.66d);
            AnchorTimePeriod per = new AnchorTimePeriod(anchor, duration, flexibility);
        }

        ///// <summary>
        ///// Verifies correct anchor placed in date and time is returned
        ///// </summary>
        //[Test]
        //public void VerifyCorrectAnchorDateTimePeriodIsReturned()
        //{
        //    TimeSpan anchor = new TimeSpan(10, 0, 0);
        //    TimeSpan duration = new TimeSpan(4, 0, 0);
        //    Percent flexibility = new Percent(0.66d);
        //    AnchorTimePeriod per = new AnchorTimePeriod(anchor, duration, flexibility);

        //    DateTime intervalStart = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //    DayOff placedDateTime = per.GetAnchorDateTimePeriod(intervalStart);

        //    Assert.AreEqual(placedDateTime.Anchor, intervalStart.Add(anchor));
        //}
    }
}