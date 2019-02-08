using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class DateOnlyTest
    {
        private DateOnly target;

        [Test]
        public void VerifyConstructor()
        {
            target = new DateOnly(2008, 1, 2);
            Assert.AreEqual(2008, target.Year);
            Assert.AreEqual(1, target.Month);
            Assert.AreEqual(2, target.Day);
        }

        [Test]
        public void VerifyEqualsAndStuff()
        {
            target = new DateOnly(2008, 1, 2);
            Assert.IsTrue(target.Equals(new DateOnly(2008, 1, 2)));
            Assert.IsFalse(target.Equals(new DateOnly(2008, 1, 3)));
            Assert.IsTrue(target == new DateOnly(2008, 1, 2));
            Assert.IsFalse(target == new DateOnly(2009, 1, 2));
            Assert.IsTrue(target != new DateOnly(2008, 2, 2));
            Assert.IsFalse(target != new DateOnly(2008, 1, 2));
            Assert.IsTrue((target < new DateOnly(2008, 2, 2)));
            Assert.IsFalse((target < new DateOnly(2008, 1, 1)));
            Assert.IsTrue((target > new DateOnly(2007, 2, 2)));
            Assert.IsFalse((target > new DateOnly(2009, 1, 1)));
            Assert.IsFalse(target.Equals(null));
            Assert.IsFalse(target.Equals(3));
            Assert.AreEqual(target, target);
        }

        [Test]
        public void VerifyCompareTo()
        {
            target = new DateOnly(2008, 1, 2);
            Assert.AreEqual(0, target.CompareTo(target));
            Assert.AreEqual(0, target.CompareTo((object)target));
            Assert.AreEqual(-1, target.CompareTo(new DateOnly(2008, 2, 2)));
            Assert.AreEqual(1, target.CompareTo(new DateOnly(2007, 1, 2)));
        }

        [Test]
        public void VerifyAddDays()
        {
            target = new DateOnly(2008, 1, 10);
            Assert.AreEqual(new DateOnly(2008, 2, 9), target.AddDays(30));
        }

        [Test]
        public void VerifyMinMaxValues()
        {
            target = DateOnly.MaxValue;
            Assert.AreEqual(DateHelper.MaxSmallDateTime, target.Date);
            target = DateOnly.MinValue;
            Assert.AreEqual(DateHelper.MinSmallDateTime, target.Date);
        }

        [Test]
        public void VerifyDayOfWeek()
        {
            target = new DateOnly(2008, 10, 19);
            Assert.AreEqual(DayOfWeek.Sunday,target.DayOfWeek);
        }

        [Test]
        public void VerifyDayOfWeekAsNumber()
        {
            target = new DateOnly(2008, 10, 13);
            Assert.AreEqual(DayOfWeek.Monday, target.DayOfWeek);
            Assert.AreEqual(1, (int)target.DayOfWeek);
            target = new DateOnly(2008, 10, 14);
            Assert.AreEqual(DayOfWeek.Tuesday, target.DayOfWeek);
            Assert.AreEqual(2, (int)target.DayOfWeek);
            target = new DateOnly(2008, 10, 15);
            Assert.AreEqual(DayOfWeek.Wednesday, target.DayOfWeek);
            Assert.AreEqual(3, (int)target.DayOfWeek);
            target = new DateOnly(2008, 10, 16);
            Assert.AreEqual(DayOfWeek.Thursday, target.DayOfWeek);
            Assert.AreEqual(4, (int)target.DayOfWeek);
            target = new DateOnly(2008, 10, 17);
            Assert.AreEqual(DayOfWeek.Friday, target.DayOfWeek);
            Assert.AreEqual(5, (int)target.DayOfWeek);
            target = new DateOnly(2008, 10, 18);
            Assert.AreEqual(DayOfWeek.Saturday, target.DayOfWeek);
            Assert.AreEqual(6, (int)target.DayOfWeek);
            target = new DateOnly(2008, 10, 19);
            Assert.AreEqual(DayOfWeek.Sunday, target.DayOfWeek);
            Assert.AreEqual(0, (int)target.DayOfWeek);
        }

        [Test]
        public void VerifyDate()
        {
            target = new DateOnly(2008, 10, 19);
            Assert.AreEqual(new DateTime(2008, 10, 19), target.Date);
        }

        [Test]
        public void VerifyToShortDateString()
        {
            target = new DateOnly(2008, 10, 19);
            string result = target.ToShortDateString();
            Assert.AreEqual(target.Date.ToShortDateString(), result);
        }

        [Test]
        public void VerifyToday()
        {
            target = DateOnly.Today;
            Assert.AreEqual(DateTime.Today, target.Date);
        }

        [Test]
        public void VerifyGetValidDateOnly()
        {
            target = new DateOnly(1900, 4, 29);
            Assert.AreEqual(new DateOnly(DateHelper.MinSmallDateTime), target.ValidDateOnly());

            target = new DateOnly(1901, 1, 1);
			Assert.AreEqual(target, target.ValidDateOnly());

            target = new DateOnly(DateHelper.MaxSmallDateTime).AddDays(1);
			Assert.AreEqual(new DateOnly(DateHelper.MaxSmallDateTime), target.ValidDateOnly());

            target = new DateOnly(DateHelper.MaxSmallDateTime);
			Assert.AreEqual(target, target.ValidDateOnly());
        }

        [Test, SetCulture("sv-SE")]
        public void ShouldReturnShortDateStringForEnglishUsCulture()
        {
            CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");
            target = new DateOnly(2010, 11, 23);
            const string expectedDateString = "11/23/2010";

            Assert.AreEqual(expectedDateString, target.ToShortDateString(enUs));
        }

	    [Test]
	    public void ShouldBeJsonSerializable()
	    {
		    var date = new DateOnly(2015, 2, 13);
		    var json = NewtonsoftJsonSerializer.Make().SerializeObject(date);
		    var deserialized = NewtonsoftJsonSerializer.Make().DeserializeObject<DateOnly>(json);
		    deserialized.Should().Be(date);
	    }

		[Test]
		public void ShouldJsonSerializeAsSingleValue()
		{
			var date = new DateOnly(2015, 2, 13);
			var json = NewtonsoftJsonSerializer.Make().SerializeObject(date);
			var deserialized = NewtonsoftJsonSerializer.Make().DeserializeObject<Dictionary<string, object>>(json);
			deserialized.Should().Have.Count.EqualTo(1);
		}

    }
}
