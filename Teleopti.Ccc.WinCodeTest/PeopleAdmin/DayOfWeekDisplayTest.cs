using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class DayOfWeekDisplayTest
    {
		IList<DayOfWeekDisplay> _days;

		[SetUp]
		public void Setup()
		{
			_days = DayOfWeekDisplay.ListOfDayOfWeek;
		}

		[Test, SetCulture("en-US"), SetUICulture("sv-SE")]
        public void ShouldReturnListWithSevenDays()
        {
        	Assert.That(_days.Count, Is.EqualTo(7));
        }

		[Test, SetUICulture("sv-SE")]
		public void ShouldDisplayWeekdayNamesInSwedish()
		{
			Assert.That(_days[0].DisplayName, Is.EqualTo("måndag"));
			Assert.That(_days[6].DisplayName, Is.EqualTo("söndag"));
		}

		[Test, SetUICulture("en-US")]
		public void ShouldDisplayWeekdayNamesInEnglish()
		{
			Assert.That(_days[0].DisplayName, Is.EqualTo("Sunday"));
			Assert.That(_days[6].DisplayName, Is.EqualTo("Saturday"));
		}

		[Test, SetCulture("sv-SE"), SetUICulture("en-US")]
        public void ShouldHaveEnglishNamesOnEnglishCulture()
        {
            Assert.That(_days.Count, Is.EqualTo(7));
            Assert.That((int)_days[0].DayOfWeek, Is.EqualTo(0));
            Assert.That((int)_days[1].DayOfWeek, Is.EqualTo(1));
        }

		[Test, SetUICulture("en-GB")]
		public void ShouldReturnMondayAsFirstDayOfWeek()
		{
			_days[0].DayOfWeek.Should().Be.EqualTo(DayOfWeek.Monday);
			_days[6].DayOfWeek.Should().Be.EqualTo(DayOfWeek.Sunday);
		}

		[Test, SetUICulture("en-US")]
		public void ShouldReturnSundayAsFirstDayOfWeek()
		{
			_days[0].DayOfWeek.Should().Be.EqualTo(DayOfWeek.Sunday);
			_days[6].DayOfWeek.Should().Be.EqualTo(DayOfWeek.Saturday);
		}
    }

    
}