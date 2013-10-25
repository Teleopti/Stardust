using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	
	[Binding]
	public class PreferencesPageStepDefinitions
	{
		public static string Cell(DateTime date)
		{
			return string.Format("li[data-mytime-date='{0}']", date.ToString("yyyy-MM-dd"));
		}

		public static string ExtendedTooltip(DateTime date)
		{
			return Cell(date) + " .extended-tooltip";
		}

		public static string ExtendedIndication(DateTime date)
		{
			return Cell(date) + " .extended-indication";
		}

		public static void SelectCalendarCellByClass(DateTime date)
		{
			Browser.Interactions.AssertExists(Cell(date) + ".ui-selectee");
			Browser.Interactions.AddClassUsingJQuery("ui-selected", Cell(date) + ".ui-selectee");
		}

		[When(@"I select day '(.*)'")]
		public void WhenISelectDayDate(DateTime date)
		{
			SelectCalendarCellByClass(date);
		}

		[When(@"I click the add extended preference button")]
		public void WhenIClickTheAddExtendedPreferenceButton()
		{
			Browser.Interactions.Click("#Preference-add-extended-button");
		}

		[When(@"I click the apply extended preferences button")]
		public void WhenIClickTheApplyButton()
		{
			Browser.Interactions.Click("#Preference-extended-apply");
		}

		[When(@"I click the reset extended preference button")]
		public void WhenIClickTheResetExtendedPreferenceButton()
		{
			Browser.Interactions.Click("#Preference-extended-reset");
		}

		[When(@"I click the extended preference indication on '(.*)'")]
		public void WhenIClickTheExtendedPreferenceIndicationOn(DateTime date)
		{
			OpenExtendedTooltip(date);
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
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format("li[data-mytime-date='{0}'] .meeting-icon", date.ToString("yyyy-MM-dd")));
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
			EventualAssert.That(() => indication.Exists, Is.False);
		}

		[Then(@"I should see the extended preference on '(.*)'")]
		public void ThenIShouldSeeTheExtendedPreferenceOn(DateTime date)
		{
			Browser.Interactions.AssertExists(ExtendedTooltip(date));
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
            Browser.Interactions.AssertFirstContains(".musthave-max",mustHave.ToString());
		}

		[Then(@"I should see I have (\d) must haves")]
		public void ThenIShouldSeeIHave1MustHaves(int mustHave)
		{
            Browser.Interactions.AssertFirstContains(".musthave-current", mustHave.ToString());
		}

		[Then(@"I should not see the extended preference button")]
		public void ThenIShouldNotSeeTheExtendedPreferenceButton()
		{
			Browser.Interactions.AssertNotExists("#preference-split-button", "#Preference-add-extended-button");
		}

		[When(@"I click to open thee extended preference drop down list")]
		public void WhenIClickToOpenTheeExtendedPreferenceDropDownList()
		{
			Select2Box.OpenWhenOptionsAreLoaded("Preference-Picker");
		}

		[Then(@"I should see these available preferences")]
		public void ThenIShouldSeeTheseAvailablePreferences(Table table)
		{
			var preferences = table.CreateSet<SingleValue>();

			preferences.ForEach(preference => Select2Box.AssertOptionExist("Preference-Picker", preference.Value));
		}

		[When(@"I click to open thee extended activity drop down list")]
		public void WhenIClickToOpenTheeExtendedActivityDropDownList()
		{
			Select2Box.OpenWhenOptionsAreLoaded("Preference-extended-activity-Picker");
		}

		[Then(@"I should see these available activities")]
		public void ThenIShouldSeeTheseAvailableActivities(Table table)
		{
			var activities = table.CreateSet<SingleValue>();
			activities.ForEach(activity => Select2Box.AssertOptionExist("Preference-extended-activity-Picker", activity.Value));
		}

		private class SingleValue
		{
			public string Value { get; set; }
		}

		[Then(@"I should see add extended preferences panel with error 'Invalid time startTime'")]
		public void ThenIShouldSeeAddExtendedPreferencesPanelWithError()
		{
			Browser.Interactions.AssertAnyContains(".preference-error-panel", string.Format(Resources.InvalidTimeValue, Resources.StartTime));
		}

		[When(@"I input extended preference fields with")]
		public void WhenIInputExtendedPreferenceFieldsWith(Table table)
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Preference-add-extended-form"); //needed

			var fields = table.CreateInstance<ExtendedPreferenceFields>();

			if (fields.Preference != null)
			{
				Select2Box.OpenWhenOptionsAreLoaded("Preference-Picker");
				Select2Box.SelectItemByText("Preference-Picker", fields.Preference);
			}

			if (fields.StartTimeMinimum != null)
				Browser.Interactions.Javascript(string.Format("$('.preference-start-time-min').timepicker('setTime', '{0}');", fields.StartTimeMinimum));
			if (fields.StartTimeMaximum != null)
				Browser.Interactions.Javascript(string.Format("$('.preference-start-time-max').timepicker('setTime', '{0}');", fields.StartTimeMaximum));
			if (fields.EndTimeMinimum != null)
				Browser.Interactions.Javascript(string.Format("$('.preference-end-time-min').timepicker('setTime', '{0}');", fields.EndTimeMinimum));
			if (fields.EndTimeMinimumNextDay)
				Browser.Interactions.Click(".preference-end-time-min-next-day");
			if (fields.EndTimeMaximum != null)
				Browser.Interactions.Javascript(string.Format("$('.preference-end-time-max').timepicker('setTime', '{0}');", fields.EndTimeMaximum));
			if (fields.EndTimeMaximumNextDay)
				Browser.Interactions.Click(".preference-end-time-max-next-day");
			if (fields.WorkTimeMinimum != null)
				Browser.Interactions.SelectOptionByTextUsingJQuery(".preference-extended-work-time-min", fields.WorkTimeMinimum);
			if (fields.WorkTimeMaximum != null)
				Browser.Interactions.SelectOptionByTextUsingJQuery(".preference-extended-work-time-max", fields.WorkTimeMaximum);

			if (fields.Activity != null)
			{
				Select2Box.OpenWhenOptionsAreLoaded("Preference-extended-activity-Picker");
				Select2Box.SelectItemByText("Preference-extended-activity-Picker", fields.Activity);
			}
				
			if (fields.ActivityStartTimeMinimum != null)
				Browser.Interactions.Javascript(string.Format("$('.preference-activity-start-time-min').timepicker('setTime', '{0}');", fields.ActivityStartTimeMinimum));
			if (fields.ActivityStartTimeMaximum != null)
				Browser.Interactions.Javascript(string.Format("$('.preference-activity-start-time-max').timepicker('setTime', '{0}');", fields.ActivityStartTimeMaximum));
			if (fields.ActivityEndTimeMinimum != null)
				Browser.Interactions.Javascript(string.Format("$('.preference-activity-end-time-min').timepicker('setTime', '{0}');", fields.ActivityEndTimeMinimum));
			if (fields.ActivityEndTimeMaximum != null)
				Browser.Interactions.Javascript(string.Format("$('.preference-activity-end-time-max').timepicker('setTime', '{0}');", fields.ActivityEndTimeMaximum));
			if (fields.ActivityTimeMinimum != null)
				Browser.Interactions.SelectOptionByTextUsingJQuery(".preference-activity-extended-work-time-min", fields.ActivityTimeMinimum);
			if (fields.ActivityTimeMaximum != null)
				Browser.Interactions.SelectOptionByTextUsingJQuery(".preference-activity-extended-work-time-max", fields.ActivityTimeMaximum);
		}

		[Then(@"I should not see the edit time fields")]
		public void ThenIShouldNotSeeTheEditTimeFields()
		{
			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-start-time-min");
			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-start-time-max");
			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-end-time-min");
			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-end-time-min-next-day");
			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-end-time-max");
			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-end-time-max-next-day");
			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-extended-work-time-min");
			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-extended-work-time-max");

			Browser.Interactions.AssertNotExists("#Preference-Picker", ".preference-activity-section");
		}

		[Then(@"all the time fields should be reset")]
		public void ThenAllTheTimeFieldsShouldBeReset()
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-start-time-min').val() === '';", "true");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-start-time-max').val() === '';", "true");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-end-time-min').val() === '';", "true");
			Browser.Interactions.AssertExists(".preference-end-time-min-next-day:not(:enabled):not(.active)");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-end-time-max').val() === '';", "true");
			Browser.Interactions.AssertExists(".preference-end-time-max-next-day:not(:enabled):not(.active)");
			Browser.Interactions.AssertExists(".preference-extended-work-time-min option:checked[value='']");
			Browser.Interactions.AssertExists(".preference-extended-work-time-max option:checked[value='']");

			AssertExtendedActivityTimeFieldsAreReset();
		}

		[Then(@"I should see extended preference fields filled with")]
		public void ThenIShouldSeeExtendedPreferenceFieldsFilledWith(Table table)
		{
			var inputs = table.CreateInstance<ExtendedPreferenceInput>();
			if (inputs.PreferenceId != null)
				Select2Box.AssertSelectedOptionText("Preference-Picker", inputs.PreferenceId);
			
			if (inputs.EarliestStartTime != null)
				Browser.Interactions.AssertInputValueUsingJQuery(".preference-start-time-min", inputs.EarliestStartTime);
		}

		[Then(@"I should see extended preference with")]
		public void ThenIShouldSeeExtendedPanelWith(Table table)
		{
			var fields = table.CreateInstance<ExtendedPreferenceFields>();

			OpenExtendedTooltip(fields.Date);

			var tooltip = ExtendedTooltip(fields.Date);
			Browser.Interactions.AssertExists(tooltip);

			if (fields.Preference != null) Browser.Interactions.AssertAnyContains(tooltip, fields.Preference);
			if (fields.StartTimeMinimum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.StartTimeMinimum);
			if (fields.StartTimeMaximum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.StartTimeMaximum);
			if (fields.EndTimeMinimum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.EndTimeMinimum);
			if (fields.EndTimeMaximum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.EndTimeMaximum);
			if (fields.WorkTimeMinimum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.WorkTimeMinimum);
			if (fields.WorkTimeMaximum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.WorkTimeMaximum);

			if (fields.Activity != null) Browser.Interactions.AssertAnyContains(tooltip, fields.Activity);
			if (fields.ActivityStartTimeMinimum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.ActivityStartTimeMinimum);
			if (fields.ActivityStartTimeMaximum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.StartTimeMaximum);
			if (fields.ActivityEndTimeMinimum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.ActivityEndTimeMinimum);
			if (fields.ActivityEndTimeMaximum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.ActivityEndTimeMaximum);
			if (fields.ActivityTimeMinimum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.ActivityTimeMinimum);
			if (fields.ActivityTimeMaximum != null) Browser.Interactions.AssertAnyContains(tooltip, fields.ActivityTimeMaximum);
		}

		private static void OpenExtendedTooltip(DateTime date)
		{
			var extendedIndication = ExtendedIndication(date);
			Browser.Interactions.AssertExists(extendedIndication);
			Browser.Interactions.Javascript("$('{0}').trigger('mouseleave')", extendedIndication.JSEncode());
			Browser.Interactions.Javascript("$('{0}').trigger('mouseenter')", extendedIndication.JSEncode());
		}

		private void AssertExtendedActivityTimeFieldsAreReset()
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-start-time-min').val() === '';", "true");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-start-time-max').val() === '';", "true");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-end-time-min').val() === '';", "true");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-end-time-max').val() === '';", "true");
			Browser.Interactions.AssertExists(".preference-activity-extended-work-time-min option:checked[value='']");
			Browser.Interactions.AssertExists(".preference-activity-extended-work-time-max option:checked[value='']");
		}

		[Then(@"I should see the activity minimum and maximum fields")]
		public void ThenIShouldSeeTheActivityMinimumAndMaximumFields()
		{
			AssertExtendedActivityTimeFieldsAreReset();
		}

		[Then(@"I should not see the edit activity minimum and maximum fields")]
		public void ThenIShouldNotSeeTheEditActivityMinimumAndMaximumFields()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".preference-activity-start-time-min");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".preference-activity-start-time-max");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".preference-activity-end-time-min");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".preference-activity-end-time-max");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".preference-activity-extended-work-time-min");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".preference-activity-extended-work-time-max");
		}

		[Then(@"I should see nothing selected in the preference dropdown")]
		public void ThenIShouldSeeNothingSelectedInThePreferenceDropdown()
		{
			Browser.Interactions.AssertExists("#Preference-Picker option:checked[value='']");
		}

		[Then(@"I should see nothing selected in the activity dropdown")]
		public void ThenIShouldSeeNothingSelectedInTheActivityDropdown()
		{
			Browser.Interactions.AssertExists("#Preference-extended-activity-Picker option:checked[value='']");
		}

		[When(@"I click set must have button")]
		public void WhenIClickOnMustHaveButton()
		{
			Browser.Interactions.Click(".add-musthave");
		}

		[When(@"I click remove must have button")]
		public void WhenIClickOnRemoveMustHaveButton()
		{
            Browser.Interactions.Click(".icon-minus");
		}

		[When(@"I select preference template with '(.*)'")]
		public void WhenISelectPreferenceTemplateWith(string name)
		{
			Browser.Interactions.SelectOptionByTextUsingJQuery(".preference-template-list", name);
		}

		[When(@"I click to open the templates dropdown")]
		public void WhenIClickToOpenTheTemplatesDropdown()
		{
			Browser.Interactions.Click(".preference-template-list");
		}

		[When(@"I click delete button for '(.*)'")]
		public void WhenIClickDeleteButtonFor(string templateName)
		{
			Browser.Interactions.Click(".preference-template-delete");
		}

		[When(@"I click Save as new template")]
		public void WhenIClickSaveAsNewTemplate()
		{
			Browser.Interactions.Click(".preference-toggle-save-template");
		}


		[When(@"I input new template name '(.*)'")]
		public void WhenIInputNewTemplateName(string name)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".preference-template-name", name);
		}

		[When(@"I click save template button")]
		public void WhenIClickSaveTemplateButton()
		{
			Browser.Interactions.Click("#Preference-extended-save-template");
		}

		[Then(@"I should see these available templates")]
		public void ThenIShouldSeeTheseAvailableTemplates(Table table)
		{
			var templates = table.CreateSet<SingleValue>();
			templates.ForEach(
				preference =>
				Browser.Interactions.AssertJavascriptResultContains(
					string.Format("return $('.preference-template-list option:contains(\"{0}\")').length > 0;", preference.Value), "true"));
		}

		[Then(@"I should not see '(.*)' in templates list")]
		public void ThenIShouldNotSeeInTemplatesList(string name)
		{
			Browser.Interactions.AssertJavascriptResultContains(
				string.Format("return $('.preference-template-list option:contains(\"{0}\")').length === 0;", name), "true");
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