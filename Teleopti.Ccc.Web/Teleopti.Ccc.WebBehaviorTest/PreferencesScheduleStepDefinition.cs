using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class PreferencesScheduleStepDefinition
	{
		[Then(@"I should see my shift")]
		public void ThenIShouldSeeMyShift()
		{
			ScenarioContext.Current.Pending();

			var data = UserFactory.User().UserData<ShiftToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);
			var from = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, UserFactory.User().Culture);
			var to = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, UserFactory.User().Culture);
			var contractTime = TimeHelper.GetLongHourMinuteTimeString(data.GetContractTime(), UserFactory.User().Culture);

			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(data.ShiftCategory.Description.Name));
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(from));
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(to));
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(contractTime));
		}

		[Then(@"I should see the dayoff")]
		public void ThenIShouldSeeTheDayoff()
		{
			ScenarioContext.Current.Pending();

			var data = UserFactory.User().UserData<DayOffToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);

			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(data.DayOff.Description.Name));
		}

		[Then(@"I should not see my existing preference")]
		public void ThenIShouldNotSeeMyExistingPreference()
		{
			ScenarioContext.Current.Pending();

			var data = UserFactory.User().UserData<ExistingPreferenceToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);

			EventualAssert.That(() => cell.InnerHtml, Is.Not.StringContaining(data.Preference));
		}

		[Then(@"I should not be able to add preference today")]
		public void ThenIShouldNotBeAbleToAddPreferenceToday()
		{
			ScenarioContext.Current.Pending();

			var data = UserFactory.User().UserData<ShiftToday>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(data.Date);

			cell.ClassName.Should().Not.Contain("ui-selectee");
			Pages.Pages.PreferencePage.SelectCalendarCellByClick(cell);
			cell.ClassName.Should().Not.Contain("ui-selected");
		}

	}
}