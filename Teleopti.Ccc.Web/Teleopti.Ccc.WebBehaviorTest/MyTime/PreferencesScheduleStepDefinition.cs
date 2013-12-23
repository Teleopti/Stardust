using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
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
			var from = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, DataMaker.Data().MyCulture);
			var to = TimeHelper.GetLongHourMinuteTimeString(data.StartTime, DataMaker.Data().MyCulture);
			var contractTime = TimeHelper.GetLongHourMinuteTimeString(data.GetContractTime(), DataMaker.Data().MyCulture);

			var selector = CalendarCells.DateSelector(data.Date);
			Browser.Interactions.AssertFirstContains(selector, data.ShiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContains(selector, from);
			Browser.Interactions.AssertFirstContains(selector, to);
			Browser.Interactions.AssertFirstContains(selector, contractTime);
		}

		[Then(@"I should see the dayoff")]
		public void ThenIShouldSeeTheDayoff()
		{
			var data = DataMaker.Data().UserData<DayOffToday>();
			
			var selector = CalendarCells.DateSelector(data.Date);
			Browser.Interactions.AssertFirstContains(selector, data.DayOff.Description.Name);
		}

		[Then(@"I should see the absence")]
		public void ThenIShouldSeeTheAbsence()
		{
			var data = DataMaker.Data().UserData<AbsenceToday>();
			var selector = CalendarCells.DateSelector(data.Date);
			Browser.Interactions.AssertFirstContains(selector, data.Absence.Description.Name);
		}

		[Then(@"I should not be able to add preference today")]
		public void ThenIShouldNotBeAbleToAddPreferenceToday()
		{
			var data = DataMaker.Data().UserData<ShiftToday>();
			var selector = CalendarCells.DateSelector(data.Date);
			
			Browser.Interactions.AssertNotExists(selector, string.Format("{0} .ui-selectee",selector));
			Browser.Interactions.Click(selector);
			Browser.Interactions.AssertNotExists(selector, string.Format("{0} .ui-selected", selector));
		}
	}
}