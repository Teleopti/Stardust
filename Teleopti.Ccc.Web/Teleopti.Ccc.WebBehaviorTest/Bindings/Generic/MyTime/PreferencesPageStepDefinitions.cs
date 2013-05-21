using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
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
			Browser.Interactions.Javascript("$('#Preference-add-extended-button').mousedown();");
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
		public void ThenIShouldSeeThatIHaveAnExtendedPreferenceOn(DateTime date)
		{
			var indication = Pages.Pages.PreferencePage.ExtendedPreferenceIndicationForDate(date);
			EventualAssert.That(() => indication.Exists, Is.True);
			EventualAssert.That(() => indication.DisplayVisible(), Is.True);
		}

		[Then(@"I should see that I have a pre-scheduled personal shift on '(.*)'")]
		[Then(@"I should see that I have a pre-scheduled meeting on '(.*)'")]
		public void ThenIShouldSeeThatIHaveAPreScheduledMeetingOn(DateTime date)
		{
			var indication = Pages.Pages.PreferencePage.MeetingAndPersonalShiftIndicationForDate(date);
			EventualAssert.That(() => indication.Exists, Is.True);
			EventualAssert.That(() => indication.DisplayVisible(), Is.True);
		}

		[Then(@"I should have a tooltip for meeting details with")]
		public void ThenIShouldHaveATooltipForMeetingDetailsWith(Table table)
		{
			var fields = table.CreateInstance<MeetingConfigurable>();
			var meetings = Pages.Pages.PreferencePage.MeetingAndPersonalShiftTooltipForDate(fields.StartTime);

			EventualAssert.That(() => meetings.InnerHtml, Is.StringContaining(fields.StartTime.ToShortTimeString().Split(' ').First()));
			EventualAssert.That(() => meetings.InnerHtml, Is.StringContaining(fields.EndTime.ToShortTimeString().Split(' ').First()));
			EventualAssert.That(() => meetings.InnerHtml, Is.StringContaining(fields.Subject));
		}

		[Then(@"I should have a tooltip for personal shift details with")]
		public void ThenIShouldHaveATooltipForPersonalShiftDetailsWith(Table table)
		{
			var fields = table.CreateInstance<PersonalShiftConfigurable>();
			var personalshifts = Pages.Pages.PreferencePage.MeetingAndPersonalShiftTooltipForDate(fields.StartTime);

			EventualAssert.That(() => personalshifts.InnerHtml, Is.StringContaining(fields.StartTime.ToShortTimeString().Split(' ').First()));
			EventualAssert.That(() => personalshifts.InnerHtml, Is.StringContaining(fields.EndTime.ToShortTimeString().Split(' ').First()));
			EventualAssert.That(() => personalshifts.InnerHtml, Is.StringContaining(fields.Activity));
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

		[Then(@"I should see the preference (.*) on '(.*)'")]
		public void ThenIShouldSeeThePreferenceLateOn(string preference, DateTime date)
		{
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(preference));
		}

		[Then(@"I should see preference")]
		public void ThenIShouldSeePreference(Table table)
		{
			var fields = table.CreateInstance<PreferenceConfigurable>();
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(fields.Date);
			var mustHave = Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "preference-must-have");

			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(">" + fields.Date.Day.ToString(CultureInfo.CurrentCulture) + "<"));

			if (fields.MustHave)
				EventualAssert.That(() => mustHave.ClassName, Is.StringContaining("icon"));
			else
				EventualAssert.That(() => mustHave.ClassName, Is.Not.StringContaining("icon"));
		}

		[Then(@"I should see I have (\d) available must haves")]
		public void ThenIShouldSeeIHave1AvailableMustHaves(int mustHave)
		{
            Browser.Interactions.AssertContains(".musthave-max",mustHave.ToString());
		}

		[Then(@"I should see I have (\d) must haves")]
		public void ThenIShouldSeeIHave1MustHaves(int mustHave)
		{
            Browser.Interactions.AssertContains(".musthave-current", mustHave.ToString());
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
			
			if (fields.Preference != null) Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.SelectWait(fields.Preference);

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

			if (fields.Activity != null) Pages.Pages.PreferencePage.ExtendedPreferenceActivity.SelectWait(fields.Activity);
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

		[Then(@"I should see extended preference fields filled with")]
		public void ThenIShouldSeeExtendedPreferenceFieldsFilledWith(Table table)
		{
			var inputs = table.CreateInstance<ExtendedPreferenceInput>();
			if (inputs.PreferenceId != null) EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.SelectedText, Is.StringContaining(inputs.PreferenceId));
			if (inputs.EarliestStartTime != null) EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceStartTimeMinimum.TextField.Value, Is.StringContaining(inputs.EarliestStartTime));
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
			Pages.Pages.PreferencePage.MustHaveButton.EventualClick();
		}

		[When(@"I click remove must have button")]
		public void WhenIClickOnRemoveMustHaveButton()
		{
            Browser.Interactions.Click(".icon-minus");
		}


		[Given(@"I have a preference template with")]
		public void GivenIHaveAPreferenceTemplateWith(Table table)
		{
			var preferenceTemplate = table.CreateInstance<PreferenceTemplateConfigurable>();
			UserFactory.User().Setup(preferenceTemplate);
		}

		[When(@"I select preference template with '(.*)'")]
		public void WhenISelectPreferenceTemplateWith(string name)
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceTemplateSelectBox.SelectWait(name);
		}

		[When(@"I click the templates select box")]
		public void WhenIClickTheTemplatesSelectBox()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceTemplateSelectBox.Open();
		}

		[When(@"I click delete button for '(.*)'")]
		public void WhenIClickDeleteButtonFor(string templateName)
		{
			Pages.Pages.PreferencePage.DeleteSpanForTemplate(templateName).EventualClick();
		}

		[When(@"I click Save as new template")]
		public void WhenIClickSaveAsNewTemplate()
		{
			Pages.Pages.PreferencePage.TemplateSaveDiv.EventualClick();
		}


		[When(@"I input new template name '(.*)'")]
		public void WhenIInputNewTemplateName(string name)
		{
			var page = Pages.Pages.PreferencePage;
			page.TemplateNameTextField.Value = name;
			Browser.Current.Eval("$('#" + Pages.Pages.PreferencePage.TemplateNameTextField.Id + "').blur()");
		}

		[When(@"I click save template button")]
		public void WhenIClickSaveTemplateButton()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceSaveTemplateButton.EventualClick();
		}

		[Then(@"I should see these available templates")]
		public void ThenIShouldSeeTheseAvailableTemplates(Table table)
		{
			var templates = table.CreateSet<SingleValue>();
			templates.ForEach(preference => EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceTemplateSelectBox.Menu.Text, Is.StringContaining(preference.Value)));
		}

		[Then(@"I should not see '(.*)' in templates list")]
		public void ThenIShouldNotSeeInTemplatesList(string name)
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.ExtendedPreferenceTemplateSelectBox.Menu.Text, Is.Not.StringContaining(name));
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

		private class ExtendedPreferenceInput
		{
			public string PreferenceId { get; set; }
			public string EarliestStartTime { get; set; }
		}

		[StepArgumentTransformation]
		public PreferenceFeedbackFields PreferenceFeedbackFieldsTransform(Table table)
		{
			return table.CreateInstance<PreferenceFeedbackFields>();
		}

		public class PreferenceFeedbackFields
		{
			public DateTime Date { get; set; }
			public string StartTimeBoundry { get; set; }
			public string EndTimeBoundry { get; set; }
			public string ContractTimeBoundry { get; set; }
			public string FeedbackError { get; set; }
		}

		[Then(@"I should see preference feedback with")]
		public void ThenIShouldSeePreferenceFeedbackWith(PreferenceFeedbackFields fields)
		{
			if (fields.StartTimeBoundry != null)
				EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "possible-start-times").InnerHtml, Is.StringMatching(fields.StartTimeBoundry));
			if (fields.EndTimeBoundry != null)
				EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "possible-end-times").InnerHtml, Is.StringMatching(fields.EndTimeBoundry));
			if (fields.ContractTimeBoundry != null)
				EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "possible-contract-times").InnerHtml, Is.StringMatching(fields.ContractTimeBoundry));
			if (fields.FeedbackError != null)
				EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(fields.Date, "feedback-error").InnerHtml, Is.StringMatching(Resources.NoAvailableShifts));
		}

		[Then(@"I should see no preference feedback on '(.*)'")]
		public void ThenIShouldSeeNoFeedback(DateTime date)
		{
			EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(date, "feedback-error").Exists, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(date, "possible-start-times").Exists, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(date, "possible-end-times").Exists, Is.False);
			EventualAssert.That(() => Pages.Pages.PreferencePage.CalendarCellDataForDate(date, "possible-contract-times").Exists, Is.False);
		}

	}
}