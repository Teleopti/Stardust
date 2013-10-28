using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferencesFollowUpStepDefinition
	{
		[Then(@"I should see the day cell with")]
		public void ThenIShouldSeeTheDayCellWith(Table table)
		{
			var fields = table.CreateInstance<DayCellFields>();
			var shift = Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "scheduled");
			if (fields.ShiftCategory != null) EventualAssert.That(() => shift.InnerHtml, Is.StringContaining(fields.ShiftCategory));

			var preference = Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "preference");
			if (fields.Preference != null) EventualAssert.That(() => preference.InnerHtml, Is.StringContaining(fields.Preference));
		}

		private class DayCellFields
		{
			public DateTime Date { get; set; }
			public string ShiftCategory { get; set; }
			public string Preference { get; set; }
		}
	}

	
}