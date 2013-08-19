using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using System.Drawing;

namespace Teleopti.Ccc.DomainTest.Time
{
    /// <summary>
    /// Tests AnchorDateTimePeriod struct
    /// </summary>
    [TestFixture]
    public class DayOffTest
    {
        private DateTime anchor = DateTime.UtcNow.Date.AddHours(12);
        private TimeSpan flexibility = TimeSpan.FromHours(8);
        private Description description = new Description("test");
        private Color displayColor = Color.Yellow;
    	private string payrollCode = "payrollCode007";

    	/// <summary>
        /// Verifies that start time and end time are set using create.
        /// </summary>
        [Test]
        public void VerifyPropertiesAreSetUsingCreate()
        {
            var duration = TimeSpan.FromHours(24);

			var per = new DayOff(anchor, duration, flexibility, description, displayColor, payrollCode);
            Assert.AreEqual(anchor, per.Anchor);
            Assert.AreEqual(duration, per.TargetLength);
            Assert.AreEqual(flexibility, per.Flexibility);
			Assert.AreEqual(payrollCode, per.PayrollCode);
			
            flexibility = TimeSpan.FromHours(12);
			per = new DayOff(anchor, duration, TimeSpan.FromHours(20), description, displayColor, payrollCode);
            Assert.AreEqual(flexibility, per.Flexibility);
	        Assert.AreEqual(payrollCode, per.PayrollCode);

        }

        /// <summary>
        /// Verifies that start time and end time are set using setter.
        /// </summary>
        [Test]
        public void VerifyPropertiesAreSetUsingSetter()
        {
            TimeSpan duration = TimeSpan.FromHours(24);
            //Percent flexibility = new Percent(0.66d);
            DayOff per = new DayOff(anchor, duration, flexibility, description, displayColor, payrollCode);
            //per.Anchor = anchor;

            Assert.AreEqual(anchor, per.Anchor);
            Assert.AreEqual(duration, per.TargetLength);
            Assert.AreEqual(flexibility, per.Flexibility);
            Assert.AreEqual(description, per.Description);
			Assert.AreEqual(payrollCode, per.PayrollCode);
        }

        /// <summary>
        /// Verifies the equals method works.
        /// </summary>
        [Test]
        public void VerifyEqualsWork()
        {
            DateTime anchorDateTime = anchor;
            TimeSpan durationHours = new TimeSpan(24, 0, 0);
            //Percent flexibility = new Percent(0.66d);

			DayOff testAnchorPeriod = new DayOff(anchorDateTime, durationHours, flexibility, description, displayColor, payrollCode);

			Assert.IsTrue(testAnchorPeriod.Equals(new DayOff(anchorDateTime, durationHours, flexibility, description, displayColor, payrollCode)));
            Assert.IsFalse(
                testAnchorPeriod.Equals(
					new DayOff(anchorDateTime, durationHours.Add(TimeSpan.FromHours(1)), flexibility, description, displayColor, payrollCode)));

						Assert.IsFalse(new DayOff(new DateTime(), new TimeSpan(), new TimeSpan(), new Description(), Color.Red, string.Empty).Equals(null));
            Assert.AreEqual(testAnchorPeriod, testAnchorPeriod);
        }

        /// <summary>
        /// Verifies that overloaded operators work.
        /// </summary>
        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            TimeSpan duration = TimeSpan.FromHours(24);
            //Percent flexibility = new Percent(0.66d);
			DayOff per = new DayOff(anchor, duration, flexibility, description, displayColor, payrollCode);
			DayOff per2 = new DayOff(new DateTime(), new TimeSpan(), new TimeSpan(), new Description(), Color.Red, string.Empty);
			Assert.IsTrue(per == new DayOff(anchor, duration, flexibility, description, displayColor, payrollCode));
            Assert.IsTrue(per != per2);
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            TimeSpan duration = TimeSpan.FromHours(4);
            //Percent flexibility = new Percent(0.66);
			DayOff per = new DayOff(anchor, duration, flexibility, description, displayColor, payrollCode);
            IDictionary<DayOff, int> dic = new Dictionary<DayOff, int>();
            dic[per] = 5;
            Assert.AreEqual(5, dic[per]);
        }

        ///// <summary>
        ///// Verifies flexibility limit (less than one) works.
        ///// </summary>
        //[Test]
        //[ExpectedException(typeof (ArgumentOutOfRangeException))]
        //public void VerifyFlexibilityLimitlessThanOneWorks()
        //{
        //    TimeSpan duration = new TimeSpan(4, 0, 0);
        //    //Percent flexibility = new Percent(-6.66d);
        //    DayOff per = new DayOff(anchor, duration, flexibility);
        //    Assert.AreEqual(per,per);
        //}

        ///// <summary>
        ///// Verifies flexibility limit (above hundred) works.
        ///// </summary>
        //[Test]
        //[ExpectedException(typeof (ArgumentOutOfRangeException))]
        //public void VerifyFlexibilityLimitAboveHundredWorks()
        //{
        //    TimeSpan duration = new TimeSpan(4, 0, 0);
        //    //Percent flexibility = new Percent(6.66d);
        //    DayOff per = new DayOff(anchor, duration, flexibility);
        //    Assert.AreEqual(per, per);
        //}

        /// <summary>
        /// Verifies correct anchor placed in date and time is of kind UTC
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void VerifyCorrectAnchorDateTimePeriodIsUtc()
        {

            TimeSpan duration = TimeSpan.FromHours(4);
            //Percent flexibility = new Percent(0.66);
			DayOff per = new DayOff(DateTime.SpecifyKind(anchor, DateTimeKind.Local), duration, flexibility, description, displayColor, payrollCode);
            Assert.AreEqual(per,per);
        }

        /// <summary>
        /// Verifies correct anchor placed in date and time is of kind UTC when using setter
        /// </summary>
        //[Test]
        //[ExpectedException(typeof (ArgumentException))]
        //public void VerifyCorrectAnchorDateTimePeriodIsUtcUsingSetter()
        //{
        //    TimeSpan duration = TimeSpan.FromHours(4);
        //    //Percent flexibility = new Percent(0.66d);
        //    DayOff per = new DayOff(anchor, duration, flexibility);

        //    per.Anchor = DateTime.Today.AddHours(12);
        //}

        /// <summary>
        /// Verifies that local anchor works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-22
        /// </remarks>
        [Test]
        public void VerifyLocalAnchorWorks()
        {
            var duration = new TimeSpan(4, 0, 0);
            //Percent flexibility = new Percent(0.10d);
			var per = new DayOff(anchor, duration, flexibility, description, displayColor, payrollCode);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var expectedLocalAnchor = TimeZoneInfo.ConvertTimeFromUtc(anchor, timeZoneInfo);
            // StateHolder.Instance.StateReader.SessionScopeData.TimeZone.ConvertTimeFromUtc(anchor, StateHolder.Instance.StateReader.SessionScopeData.TimeZone);

            Assert.AreEqual(expectedLocalAnchor, per.AnchorLocal(timeZoneInfo));
        }

        /// <summary>
        /// Verifies the get boundary works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        [Test]
        public void VerifyGetBoundaryWorks()
        {
            TimeSpan duration = new TimeSpan(24, 0, 0);
            //Percent flexibility = new Percent(0.10d);
			DayOff per = new DayOff(anchor, duration, TimeSpan.FromMinutes(12), description, displayColor, payrollCode);

            DateTimePeriod expectedBoundary = new DateTimePeriod(
                anchor.AddHours(-12).AddMinutes(-12),
                anchor.AddHours(12).AddMinutes(12));

            Assert.AreEqual(expectedBoundary, per.Boundary);
        }

        /// <summary>
        /// Verifies the get inner boundary works.
        /// </summary>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-06-12    
        /// /// </remarks>
        [Test]
        public void VerifyGetInnerBoundaryWorks()
        {
            TimeSpan duration = new TimeSpan(24, 0, 0);
            //Percent flexibility = new Percent(0.50d);
			DayOff per = new DayOff(anchor, duration, TimeSpan.FromHours(8), description, displayColor, payrollCode);

            DateTimePeriod expectedBoundary = new DateTimePeriod(
                anchor.AddHours(-12).AddHours(8),
                anchor.AddHours(12).AddHours(-8));

            Assert.AreEqual(expectedBoundary, per.InnerBoundary);
        }

        [Test]
        public void VerifyGetInnerBoundaryWorksWhenTooLongFlex()
        {
            TimeSpan duration = new TimeSpan(24, 0, 0);
			DayOff per = new DayOff(anchor, duration, TimeSpan.FromHours(20), description, displayColor, payrollCode);

            DateTimePeriod expectedBoundary = new DateTimePeriod(
                anchor.AddHours(-12).AddHours(12),
                anchor.AddHours(12).AddHours(-12));

            Assert.AreEqual(expectedBoundary, per.InnerBoundary);
        }
    }
}