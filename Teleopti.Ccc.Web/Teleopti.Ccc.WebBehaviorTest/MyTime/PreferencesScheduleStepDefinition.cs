using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferencesScheduleStepDefinition
	{
		[Then(@"I should see my shift")]
		public void ThenIShouldSeeMyShift()
		{
			var data = DataMaker.Data().UserData<ShiftToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);
			var from = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, DataMaker.Data().MyCulture);
			var to = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, DataMaker.Data().MyCulture);
			var contractTime = TimeHelper.GetLongHourMinuteTimeString(data.GetContractTime(), DataMaker.Data().MyCulture);

			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(data.ShiftCategory.Description.Name));
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(from));
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(to));
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(contractTime));
		}

		[Then(@"I should see the dayoff")]
		public void ThenIShouldSeeTheDayoff()
		{
			var data = DataMaker.Data().UserData<DayOffToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);

			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(data.DayOff.Description.Name));
		}

		[Then(@"I should see the absence")]
		public void ThenIShouldSeeTheAbsence()
		{
			var data = DataMaker.Data().UserData<AbsenceToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);

			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(data.Absence.Description.Name));
		}

		[Then(@"I should not be able to add preference today")]
		public void ThenIShouldNotBeAbleToAddPreferenceToday()
		{
			var data = DataMaker.Data().UserData<ShiftToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);

			cell.ClassName.Should().Not.Contain("ui-selectee");
			Pages.Pages.PreferencePage.SelectCalendarCellByClick(cell);
			cell.ClassName.Should().Not.Contain("ui-selected");
		}

	}
}