using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class DayOfWeekDisplayTest
    {
		[Test, SetCulture("en-US"), SetUICulture("sv-SE")]
        public void ShouldReturnListWithSevenDays()
        {
			var days = DayOfWeekDisplay.ListOfDayOfWeek;
			Assert.That(days.Count, Is.EqualTo(7));
        }

		[Test, SetUICulture("sv-SE")]
		public void ShouldDisplayWeekdayNamesInSwedish()
		{
			var days = DayOfWeekDisplay.ListOfDayOfWeek;
			Assert.That(days[0].DisplayName, Is.EqualTo("måndag"));
			Assert.That(days[6].DisplayName, Is.EqualTo("söndag"));
		}

		[Test, SetUICulture("en-US")]
		public void ShouldDisplayWeekdayNamesInEnglish()
		{
			var days = DayOfWeekDisplay.ListOfDayOfWeek;
			Assert.That(days[0].DisplayName, Is.EqualTo("Sunday"));
			Assert.That(days[6].DisplayName, Is.EqualTo("Saturday"));
		}

		[Test, SetCulture("sv-SE"), SetUICulture("en-US")]
        public void ShouldHaveEnglishNamesOnEnglishCulture()
        {
			var days = DayOfWeekDisplay.ListOfDayOfWeek;
			Assert.That(days.Count, Is.EqualTo(7));
            Assert.That((int)days[0].DayOfWeek, Is.EqualTo(0));
            Assert.That((int)days[1].DayOfWeek, Is.EqualTo(1));
        }

		[Test, SetUICulture("en-GB")]
		public void ShouldReturnMondayAsFirstDayOfWeek()
		{
			var days = DayOfWeekDisplay.ListOfDayOfWeek;
			days[0].DayOfWeek.Should().Be.EqualTo(DayOfWeek.Monday);
			days[6].DayOfWeek.Should().Be.EqualTo(DayOfWeek.Sunday);
		}

		[Test, SetUICulture("en-US")]
		public void ShouldReturnSundayAsFirstDayOfWeek()
		{
			var days = DayOfWeekDisplay.ListOfDayOfWeek;
			days[0].DayOfWeek.Should().Be.EqualTo(DayOfWeek.Sunday);
			days[6].DayOfWeek.Should().Be.EqualTo(DayOfWeek.Saturday);
		}

		[Test, SetUICulture("sv-SE")]
	    public void ShouldCompare()
		{
			var days = DayOfWeekDisplay.ListOfDayOfWeek;
			var monday = days[0];
			var tuesday = days[1];
			var anotherTuesday = days[1];

			var result = monday.CompareTo(tuesday);
			Assert.AreEqual(-1, result);

			result = tuesday.CompareTo(monday);
			Assert.AreEqual(1, result);

			result = tuesday.CompareTo(anotherTuesday);
			Assert.AreEqual(0, result);

			result = monday.CompareTo(null);
			Assert.AreEqual(-1, result);
		}
    }   
}