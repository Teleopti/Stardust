using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferencesFollowUpStepDefinition
	{
		[Then(@"I should see the day cell with")]
		public void ThenIShouldSeeTheDayCellWith(Table table)
		{
			var fields = table.CreateInstance<DayCellFields>();
			var shift = CalendarCells.DateSelector(fields.Date);
			if (fields.ShiftCategory != null)
				Browser.Interactions.AssertFirstContains(string.Format("{0} .{1}", shift, "scheduled"), fields.ShiftCategory);

			if (fields.Preference != null)
				Browser.Interactions.AssertFirstContains(string.Format("{0} .{1}", shift, "preference-text"), fields.Preference);
		}

		private class DayCellFields
		{
			public DateTime Date { get; set; }
			public string ShiftCategory { get; set; }
			public string Preference { get; set; }
		}
	}
}