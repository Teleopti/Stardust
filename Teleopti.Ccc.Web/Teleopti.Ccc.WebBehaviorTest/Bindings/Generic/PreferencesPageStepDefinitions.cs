using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
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
			Pages.Pages.PreferencePage.ExtendedPreferenceButton.Focus();
			Pages.Pages.PreferencePage.ExtendedPreferenceButton.EventualClick();
		}

		[When(@"I click the apply extended preferences button")]
		public void WhenIClickTheApplyButton()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceApplyButton.EventualClick();
		}

		[When(@"I click the reset extended preference button")]
		public void WhenIClickTheResetExtendedPreferenceButton()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceResetButton.EventualClick();
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

		[Then(@"I should see preference")]
		public void ThenIShouldSeePreference(Table table)
		{

			var fields = table.CreateInstance<PreferenceConfigurable>();

			//I should see the correct date on the cell header: the right day
			DateTime date = fields.Date;
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(date);
			//var mustHave = Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "preference-must-have");
			var mustHave = cell.Div(Find.ByClass("preference-must-have", false));

			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(">" + date.Day.ToString(CultureInfo.CurrentCulture) +"<"));

			//I should see on heart icon on the current calendar cell, accorning the the must have settings
			//todo: add an icon and test code here
			EventualAssert.That(() => mustHave.Exists, Is.EqualTo(fields.MustHave));

		}

		[Then(@"I should see I have (\d) available must haves")]
		public void ThenIShouldSeeIHave1AvailableMustHaves(int mustHave)
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.MustHaveNumbersText.Text, Is.StringContaining("(" + mustHave.ToString(CultureInfo.CurrentCulture) + ")"));
		}

		[Then(@"I should see I have (\d) must haves")]
		public void ThenIShouldSeeIHave1MustHaves(int mustHave)
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.MustHaveNumbersText.Text, Is.StringContaining(mustHave.ToString(CultureInfo.CurrentCulture) + "("));
		}


		[Then(@"I should not see the extended preference button")]
		public void ThenIShouldNotSeeTheExtendedPreferenceButton()
		{
			EventualAssert.That(()=>Pages.Pages.PreferencePage.ExtendedPreferenceButton.Exists, Is.False);
		}

		[When(@"I click the extended preference select-box")]
		public void WhenIClickTheExtendedPreferenceSelecBox()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.Open();
		}

		[Then(@"I should see these available preferences")]
		public void ThenIShouldSeeTheseAvailablePreferences(Table table)
		{
			var preferences = table.CreateSet<SingleValue>();
			preferences.ForEach(preference => EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.Menu.Text, Is.StringContaining(preference.Value)));
		}

		[When(@"I click the extended preference activity select-box")]
		public void WhenIClickTheExtendedPreferenceActivitySelecBox()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceActivity.Open();
		}

		[Then(@"I should see these available activities")]
		public void ThenIShouldSeeTheseAvailableActivities(Table table)
		{
			var activities = table.CreateSet<SingleValue>();
			activities.ForEach(activity => EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivity.Menu.Text, Is.StringContaining(activity.Value)));
		}

		private class SingleValue
		{
			public string Value { get; set; }
		}
		[Then(@"I should see add extended preferences panel with error 'Invalid time startTime'")]
		public void ThenIShouldSeeAddExtendedPreferencesPanelWithError()
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferencePanel.Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferencePanelError.Text, Is.StringContaining(string.Format(Resources.InvalidTimeValue, Resources.StartTime)));
		}

		[When(@"I input extended preference fields with")]
		public void WhenIInputExtendedPreferenceFieldsWith(Table table)
		{
			var fields = table.CreateInstance<ExtendedPreferenceFields>();
			Pages.Pages.PreferencePage.ExtendedPreferencePanel.WaitUntilDisplayed(); //needed
			
			if (fields.Preference != null) Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.Select(fields.Preference);

			if (fields.StartTimeMinimum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMinimum.Value = fields.StartTimeMinimum;
			if (fields.StartTimeMaximum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMaximum.Value = fields.StartTimeMaximum;
			if (fields.EndTimeMinimum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMinimum.Value = fields.EndTimeMinimum;
			if (fields.EndTimeMinimumNextDay)
				Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMinimumNextDay.Checked = true;
			if (fields.EndTimeMaximum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMaximum.Value = fields.EndTimeMaximum;
			if (fields.EndTimeMaximumNextDay)
				Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMaximumNextDay.Checked = true;
			if (fields.WorkTimeMinimum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceWorkTimeMinimum.Value = fields.WorkTimeMinimum;
			if (fields.WorkTimeMaximum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceWorkTimeMaximum.Value = fields.WorkTimeMaximum;

			if (fields.Activity != null) Pages.Pages.PreferencePage.ExtendedPreferenceActivity.Select(fields.Activity);
			if (fields.ActivityStartTimeMinimum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMinimum.Value = fields.ActivityStartTimeMinimum;
			if (fields.ActivityStartTimeMaximum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMaximum.Value = fields.ActivityStartTimeMaximum;
			if (fields.ActivityEndTimeMinimum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMinimum.Value = fields.ActivityEndTimeMinimum;
			if (fields.ActivityEndTimeMaximum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMaximum.Value = fields.ActivityEndTimeMaximum;
			if (fields.ActivityTimeMinimum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMinimum.Value = fields.ActivityTimeMinimum;
			if (fields.ActivityTimeMaximum != null)
				Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMaximum.Value = fields.ActivityTimeMaximum;
		}

		[Then(@"I should not be able to edit time fields")]
		public void ThenIShouldNotBeAbleToEditTimeFields()
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMaximum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMaximum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceWorkTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceWorkTimeMaximum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMaximumNextDay.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMinimumNextDay.Enabled, Is.False);

			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivity.Button.Enabled, Is.False,"");
			
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMaximum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMaximum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMaximum.Enabled, Is.False);
		}

		[Then(@"all the time fields should be reset")]
		public void ThenAllTheTimeFieldsShouldBeReset()
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMinimum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMaximum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMinimum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMaximum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceWorkTimeMinimum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceWorkTimeMaximum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMaximumNextDay.Checked, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceEndTimeMinimumNextDay.Checked, Is.False);

			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMinimum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMaximum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMinimum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMaximum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMinimum.TextField.Value, Is.Null);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMaximum.TextField.Value, Is.Null);
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

		[Then(@"I should be able to edit activity minimum and maximum fields")]
		public void ThenIShouldBeAbleToEditActivityMinimumAndMaximumFields()
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMinimum.Enabled, Is.True);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMaximum.Enabled, Is.True);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMinimum.Enabled, Is.True);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMaximum.Enabled, Is.True);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMinimum.Enabled, Is.True);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMaximum.Enabled, Is.True);
		}

		[Then(@"I should not be able to edit activity minimum and maximum fields")]
		public void ThenIShouldNotBeAbleToEditActivityMinimumAndMaximumFields()
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityStartTimeMaximum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityEndTimeMaximum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMinimum.Enabled, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceActivityTimeMaximum.Enabled, Is.False);
		}

		[Then(@"I should see preference dropdown list selected to ""(.*)""")]
		public void ThenIShouldSeePreferenceDropdownListSelectedTo(string selectedText)
		{
			EventualAssert.That(() =>
				Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.Button.OuterText, Is.EqualTo(selectedText));
		}


		[Then(@"I should see activity dropdown list selected to ""(.*)""")]
		public void ThenIShouldSeeActivityDropdownListSelected(string selectedText)
		{
			EventualAssert.That(() => 
				Pages.Pages.PreferencePage.ExtendedPreferenceActivity.Button.OuterText, Is.EqualTo(selectedText));
		}

		[When(@"I click set must have button")]
		public void WhenIClickOnMustHaveButton()
		{
			// I have a must have button on the menu bar
			// todo: imitate that I click on the button
			Pages.Pages.PreferencePage.MustHaveButton.Focus();
			Pages.Pages.PreferencePage.MustHaveButton.EventualClick();
		}

		[When(@"I click remove must have button")]
		public void WhenIClickOnRemoveMustHaveButton()
		{
			Pages.Pages.PreferencePage.MustHaveDeleteButton.Focus();
			Pages.Pages.PreferencePage.MustHaveDeleteButton.EventualClick();
		}


		private class ExtendedPreferenceFields
		{
			public DateTime Date { get; set; }
			public string Preference { get; set; }
			public string StartTimeMinimum { get; set; }
			public string StartTimeMaximum { get; set; }
			public string EndTimeMinimum { get; set; }
			public bool EndTimeMinimumNextDay { get; set; }
			public string EndTimeMaximum { get; set; }
			public bool EndTimeMaximumNextDay { get; set; }
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



		[StepArgumentTransformation]
		public PreferenceFeedbackFields PreferenceFeedbackFieldsTransform(Table table)
		{
			return table.CreateInstance<PreferenceFeedbackFields>();
		}

		public class PreferenceFeedbackFields
		{
			public DateTime Date { get; set; }
			public string ContractTimeBoundry { get; set; }
		}

		[Then(@"I should see preference feedback with")]
		public void ThenIShouldSeePreferenceFeedbackWith(PreferenceFeedbackFields fields)
		{
			ScenarioContext.Current.Pending();
			EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "possible-contract-times").InnerHtml, Is.StringMatching(fields.ContractTimeBoundry));
		}

	}
}