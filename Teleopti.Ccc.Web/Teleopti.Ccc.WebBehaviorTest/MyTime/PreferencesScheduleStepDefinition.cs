using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;


namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferencesScheduleStepDefinition
	{
		[Then(@"I should see my assigned shift for '(.*)'")]
		public void ThenIShouldSeeMyShift(DateTime date)
		{
			var data = DataMaker.Data().UserData<AssignedShift>();
			var cultureInfo = DataMaker.Data().MyCulture;
			var from = TimeHelper.GetLongHourMinuteTimeString(TimeSpan.Parse(data.StartTime), cultureInfo);
			var to = TimeHelper.GetLongHourMinuteTimeString(TimeSpan.Parse(data.EndTime), cultureInfo);
			var contractTime = TimeHelper.GetLongHourMinuteTimeString(data.GetContractTime(), cultureInfo);

			var selector = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertFirstContains(selector, data.ShiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContains(selector, from);
			Browser.Interactions.AssertFirstContains(selector, to);
			Browser.Interactions.AssertFirstContains(selector, contractTime);
		}

		[Then(@"I should see the assigned dayoff for '(.*)'")]
		public void ThenIShouldSeeTheDayoff(DateTime date)
		{
			var data = DataMaker.Data().UserData<AssignedDayOff>();
			
			var selector = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertFirstContains(selector, data.DayOff.Description.Name);
		}

		[Then(@"I should see the assigned absence for '(.*)'")]
		public void ThenIShouldSeeTheAbsence(DateTime date)
		{
			var data = DataMaker.Data().UserData<AssignedAbsence>();
			var selector = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertFirstContains(selector, data.Absence.Description.Name);
		}

		[Then(@"I should not be able to add preference for '(.*)'")]
		public void ThenIShouldNotBeAbleToAddPreferenceForDate(DateTime date)
		{
			var selector = CalendarCells.DateSelector(date);
			
			Browser.Interactions.AssertNotExists(selector, string.Format("{0} .ui-selectee",selector));
			Browser.Interactions.Click(selector);
			Browser.Interactions.AssertNotExists(selector, string.Format("{0} .ui-selected", selector));
		}
	}
}