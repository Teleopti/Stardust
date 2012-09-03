using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PreferencesPageStepDefinitions
	{
		[When(@"I select day '(.*)'")]
		public void WhenISelectDayDate(DateTime date)
		{
			Pages.Pages.PreferencePage.SelectCalendarCellByClass(date);
		}

		[When(@"I click the add extended preference button")]
		public void WhenIClickTheAddExtendedPreferenceButton()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceButton.EventualClick();
		}

		[When(@"I click the apply extended preferences button")]
		public void WhenIClickTheApplyButton()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceApplyButton.EventualClick();
		}

		[When(@"I click the extended preference indication on '(.*)'")]
		public void WhenIClickTheExtendedPreferenceIndicationOn(DateTime date)
		{
			var indication = Pages.Pages.PreferencePage.ExtendedPreferenceIndicationForDate(date);
			indication.EventualClick();
		}

		[Then(@"I should see that I have an extended preference on '(.*)'")]
		[Then(@"I should see an extended preference indication on '(.*)'")]
		public void ThenIShouldSeeThatIHaveAnExtendedPreferenceOn(DateTime date)
		{
			var indication = Pages.Pages.PreferencePage.ExtendedPreferenceIndicationForDate(date);
			EventualAssert.That(() => indication.Exists, Is.True);
			EventualAssert.That(() => indication.DisplayVisible(), Is.True);
		}

		[Then(@"I should not see an extended preference indication on '(.*)'")]
		public void ThenIShouldNotSeeAnExtendedPreferenceIndicationOn(DateTime date)
		{
			var indication = Pages.Pages.PreferencePage.ExtendedPreferenceIndicationForDate(date);
			EventualAssert.That(() => indication.DisplayVisible(), Is.False);
		}

		[Then(@"I should see the extended preference on '(.*)'")]
		public void ThenIShouldSeeTheExtendedPreferenceOn(DateTime date)
		{
			var extendedPreference = Pages.Pages.PreferencePage.ExtendedPreferenceForDate(date);
			EventualAssert.That(() => extendedPreference.Exists, Is.True);
			EventualAssert.That(() => extendedPreference.JQueryVisible(), Is.True);
			EventualAssert.That(() => extendedPreference.DisplayVisible(), Is.True);
		}

		[Then(@"I should see the preference '(.*)' on '(.*)'")]
		public void ThenIShouldSeeThePreferenceLateOn(string preference, DateTime date)
		{
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(preference));
		}



		[When(@"I input extended preference fields with")]
		public void WhenIInputExtendedPreferenceFieldsWith(Table table)
		{
			var fields = table.CreateInstance<ExtendedPreferenceFields>();
			if (fields.Preference == null)
				ScenarioContext.Current.Pending();

			if (fields.Preference != null) Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.Select(fields.Preference);
			if (fields.StartTimeMinimum != null)
			{
				ScenarioContext.Current.Pending();
				return;
				Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMinimum.Value = fields.StartTimeMinimum;
			}
			return;
			if (fields.StartTimeMaximum != null) Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMaximum.Value = fields.StartTimeMaximum;
			if (fields.EndTimeMinimum != null) Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMinimum.Value = fields.EndTimeMinimum;
			if (fields.EndTimeMaximum != null) Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMaximum.Value = fields.EndTimeMaximum;
			if (fields.WorkTimeMinimum != null) Pages.Pages.PreferencePage.ExtendedPreferenceWorkTimeMinimum.Value = fields.WorkTimeMinimum;
			if (fields.WorkTimeMaximum != null) Pages.Pages.PreferencePage.ExtendedPreferenceWorkTimeMaximum.Value = fields.WorkTimeMaximum;

			if (fields.Activity != null) Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.Select(fields.Activity);
			if (fields.ActivityStartTimeMinimum != null) Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMinimum.Value = fields.ActivityStartTimeMinimum;
			if (fields.ActivityStartTimeMaximum != null) Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMaximum.Value = fields.ActivityStartTimeMaximum;
			if (fields.ActivityEndTimeMinimum != null) Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMinimum.Value = fields.ActivityEndTimeMinimum;
			if (fields.ActivityEndTimeMaximum != null) Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMaximum.Value = fields.ActivityEndTimeMaximum;
			if (fields.ActivityTimeMinimum != null) Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMinimum.Value = fields.ActivityTimeMinimum;
			if (fields.ActivityTimeMaximum != null) Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMaximum.Value = fields.ActivityTimeMaximum;
		}

		[Then(@"I should see extended preference with")]
		public void ThenIShouldSeeExtendedPanelWith(Table table)
		{
			var fields = table.CreateInstance<ExtendedPreferenceFields>();
			var extendedPreference = Pages.Pages.PreferencePage.ExtendedPreferenceForDate(fields.Date);

			if (fields.Preference != null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.Preference));
			if (fields.StartTimeMinimum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.StartTimeMinimum));
			if (fields.StartTimeMaximum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.StartTimeMaximum));
			if (fields.EndTimeMinimum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.EndTimeMinimum));
			if (fields.EndTimeMaximum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.EndTimeMaximum));
			if (fields.WorkTimeMinimum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.WorkTimeMinimum));
			if (fields.WorkTimeMaximum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.WorkTimeMaximum));

			if (fields.Activity != null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.Activity));
			if (fields.ActivityStartTimeMinimum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.ActivityStartTimeMinimum));
			if (fields.ActivityStartTimeMaximum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.StartTimeMaximum));
			if (fields.ActivityEndTimeMinimum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.ActivityEndTimeMinimum));
			if (fields.ActivityEndTimeMaximum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.ActivityEndTimeMaximum));
			if (fields.ActivityTimeMinimum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.ActivityTimeMinimum));
			if (fields.ActivityTimeMaximum!= null) EventualAssert.That(() => extendedPreference.InnerHtml, Is.StringContaining(fields.ActivityTimeMaximum));
		}



		private class ExtendedPreferenceFields
		{
			public DateTime Date { get; set; }
			public string Preference { get; set; }
			public string StartTimeMinimum { get; set; }
			public string StartTimeMaximum { get; set; }
			public string EndTimeMinimum { get; set; }
			public string EndTimeMaximum { get; set; }
			public string WorkTimeMinimum { get; set; }
			public string WorkTimeMaximum { get; set; }
			public string Activity { get; set; }
			public string ActivityStartTimeMinimum { get; set; }
			public string ActivityStartTimeMaximum { get; set; }
			public string ActivityEndTimeMinimum { get; set; }
			public string ActivityEndTimeMaximum { get; set; }
			public string ActivityTimeMinimum { get; set; }
			public string ActivityTimeMaximum { get; set; }
		}


	}



}