using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferencesScheduleStepDefinition
	{
		[Then(@"I should see my shift")]
		public void ThenIShouldSeeMyShift()
		{
			var data = DataMaker.Data().UserData<ShiftToday>();
			var from = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, DataMaker.Data().MyCulture);
			var to = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, DataMaker.Data().MyCulture);
			var contractTime = TimeHelper.GetLongHourMinuteTimeString(data.GetContractTime(), DataMaker.Data().MyCulture);

			var selector = CalendarCellsPage.DateSelector(data.Date);
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, data.ShiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, from);
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, to);
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, contractTime);
		}

		[Then(@"I should see the dayoff")]
		public void ThenIShouldSeeTheDayoff()
		{
			var data = DataMaker.Data().UserData<DayOffToday>();
			
			var selector = CalendarCellsPage.DateSelector(data.Date);
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, data.DayOff.Description.Name);
		}

		[Then(@"I should see the absence")]
		public void ThenIShouldSeeTheAbsence()
		{
			var data = DataMaker.Data().UserData<AbsenceToday>();
			var selector = CalendarCellsPage.DateSelector(data.Date);
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, data.Absence.Description.Name);
		}

		[Then(@"I should not be able to add preference today")]
		public void ThenIShouldNotBeAbleToAddPreferenceToday()
		{
			var data = DataMaker.Data().UserData<ShiftToday>();
			var selector = CalendarCellsPage.DateSelector(data.Date);
			
			Browser.Interactions.AssertNotExistsUsingJQuery(selector, string.Format("{0} .ui-selectee",selector));
			Browser.Interactions.ClickUsingJQuery(selector);
			Browser.Interactions.AssertNotExistsUsingJQuery(selector, string.Format("{0} .ui-selected", selector));
		}
	}
}