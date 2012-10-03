using System;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest
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

			//if (fields.Fulfilled != null) EventualAssert.That(() => preference.InnerHtml, Is.StringContaining(fields.Fulfilled));
		}


		[Given(@"I have a shift with")]
		public void GivenIHaveAShiftWith(Table table)
		{
			var schedule = table.CreateInstance<ScheduledShiftConfigurable>();
			UserFactory.User().Setup(schedule);
		}


		private class DayCellFields
		{
			public DateTime Date { get; set; }
			public string ShiftCategory { get; set; }
			public string Preference { get; set; }
			public bool? Fulfilled { get; set; }
		}
	}

	
}