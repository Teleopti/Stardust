using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{

	[Binding]
	public class PreferencesPageStepDefinitions
	{
		private static string ExtendedTooltip(DateTime date)
		{
			return CalendarCells.DateSelector(date) + " .extended-tooltip";
		}

		private static string ExtendedIndication(DateTime date)
		{
			return CalendarCells.DateSelector(date) + " .extended-indication";
		}

		public static void SelectCalendarCellByClass(DateTime date)
		{
			var selector = CalendarCells.DateSelector(date) + ".ui-selectee";
			Browser.Interactions.AssertExists(selector);
			Browser.Interactions.AddClassUsingJQuery("ui-selected", selector);
		}

		[When(@"I select day '(.*)'")]
		public void WhenISelectDayDate(DateTime date)
		{
			SelectCalendarCellByClass(date);
		}

		[When(@"I click the add extended preference button")]
		public void WhenIClickTheAddExtendedPreferenceButton()
		{
			Browser.Interactions.ClickUsingJQuery(".preference-phone-navbar .Preference-add-extended-button[data-menu-loaded=true]");
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
			Browser.Interactions.AssertVisibleUsingJQuery(ExtendedIndication(date));
		}

		[Then(@"I should see that I have a pre-scheduled personal shift on '(.*)'")]
		[Then(@"I should see that I have a pre-scheduled meeting on '(.*)'")]
		public void ThenIShouldSeeThatIHaveAPreScheduledMeetingOn(DateTime date)
		{
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date.ToString("yyyy-MM-dd") + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format("li[data-mytime-date='{0}'] .meeting-icon", date.ToString("yyyy-MM-dd")));
		}

		[Then(@"I should have a tooltip for meeting details with")]
		public void ThenIShouldHaveATooltipForMeetingDetailsWith(Table table)
		{
			var fields = table.CreateInstance<MeetingConfigurable>();
			var cell = CalendarCells.DateSelector(fields.StartTime);

			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + fields.StartTime.ToString("yyyy-MM-dd") + "\"] .day-content figure.cover-me", "IsLoading()", "False");

			var selector = string.Format("{0} .{1}", cell, "meeting-tooltip");
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, fields.StartTime.ToShortTimeString().Split(' ').First());
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, fields.EndTime.ToShortTimeString().Split(' ').First());
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, fields.Subject);
		}

		[Then(@"I should have a tooltip for personal shift details with")]
		public void ThenIShouldHaveATooltipForPersonalShiftDetailsWith(Table table)
		{
			var fields = table.CreateInstance<PersonalShiftConfigurable>();
			var cell = CalendarCells.DateSelector(fields.StartTime);

			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + fields.StartTime.ToString("yyyy-MM-dd") + "\"] .day-content figure.cover-me", "IsLoading()", "False");

			var selector = string.Format("{0} .{1}", cell, "meeting-tooltip");
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, fields.StartTime.ToShortTimeString().Split(' ').First());
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, fields.EndTime.ToShortTimeString().Split(' ').First());
			Browser.Interactions.AssertFirstContainsUsingJQuery(selector, fields.Activity);
		}

		[Then(@"I should not see an extended preference indication on '(.*)'")]
		public void ThenIShouldNotSeeAnExtendedPreferenceIndicationOn(DateTime date)
		{
			var cell = CalendarCells.DateSelector(date);

			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date.ToString("yyyy-MM-dd") + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertNotExists(cell, string.Format("{0} .{1}", cell, "extended-indication"));
		}

		[Then(@"I should see the extended preference on '(.*)'")]
		public void ThenIShouldSeeTheExtendedPreferenceOn(DateTime date)
		{
			Browser.Interactions.AssertExistsUsingJQuery($"li[data-mytime-date={date:yyyy-MM-dd}] .extended-indication");
			Browser.Interactions.AssertExistsUsingJQuery(".tooltip.top.in");
		}

		[Then(@"I should see the preference (.*) on '(.*)'")]
		public void ThenIShouldSeeThePreferenceLateOn(string preference, DateTime date)
		{
			var cell = CalendarCells.DateSelector(date);

			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date.ToString("yyyy-MM-dd") + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstContains(cell, preference);
		}

		[Then(@"I should see preference")]
		public void ThenIShouldSeePreference(Table table)
		{
			var fields = table.CreateInstance<PreferenceConfigurable>();
			var cell = CalendarCells.DateSelector(fields.Date);

			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + fields.Date.ToString("yyyy-MM-dd") + "\"] .day-content figure.cover-me", "IsLoading()", "False");

			Browser.Interactions.AssertFirstContains(cell, fields.Date.Day.ToString(CultureInfo.CurrentCulture));

			var mustHaveIcon = string.Format("{0} .{1}.{2}", cell, "preference-must-have", "glyphicon-heart");
			if (fields.MustHave)
				Browser.Interactions.AssertExists(mustHaveIcon);
			else
				Browser.Interactions.AssertNotExists(cell, mustHaveIcon);
		}

		[Then(@"I should see I have (\d) available must haves")]
		public void ThenIShouldSeeIHave1AvailableMustHaves(int mustHave)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".preference-phone-navbar .musthave-max", mustHave.ToString());
		}

		[Then(@"I should see I have (\d) must haves")]
		public void ThenIShouldSeeIHave1MustHaves(int mustHave)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".preference-phone-navbar .musthave-current", mustHave.ToString());
		}

		[Then(@"I should not see the extended preference button")]
		public void ThenIShouldNotSeeTheExtendedPreferenceButton()
		{
			Browser.Interactions.AssertNotExists(".preference-split-button", ".Preference-add-extended-button");
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

				Browser.Interactions.Javascript_IsFlaky(string.Format("$('#{0}').select2('close');", "Preference-Picker")); // for IE
			}

			if (fields.StartTimeMinimum != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.preference-start-time-min').timepicker('setTime', '{0}');", fields.StartTimeMinimum));
			if (fields.StartTimeMaximum != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.preference-start-time-max').timepicker('setTime', '{0}');", fields.StartTimeMaximum));
			if (fields.EndTimeMinimum != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.preference-end-time-min').timepicker('setTime', '{0}');", fields.EndTimeMinimum));
			if (fields.EndTimeMinimumNextDay)
				Browser.Interactions.Click(".preference-end-time-min-next-day");
			if (fields.EndTimeMaximum != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.preference-end-time-max').timepicker('setTime', '{0}');", fields.EndTimeMaximum));
			if (fields.EndTimeMaximumNextDay)
				Browser.Interactions.Click(".preference-end-time-max-next-day");
			if (fields.WorkTimeMinimum != null)
				Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".preference-extended-work-time-min", fields.WorkTimeMinimum);
			if (fields.WorkTimeMaximum != null)
				Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".preference-extended-work-time-max", fields.WorkTimeMaximum);

			if (fields.Activity != null)
			{
				Select2Box.OpenWhenOptionsAreLoaded("Preference-extended-activity-Picker");
				Select2Box.SelectItemByText("Preference-extended-activity-Picker", fields.Activity);

				Browser.Interactions.Javascript_IsFlaky(string.Format("$('#{0}').select2('close');", "Preference-extended-activity-Picker")); // for IE
			}

			if (fields.ActivityStartTimeMinimum != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.preference-activity-start-time-min').timepicker('setTime', '{0}');", fields.ActivityStartTimeMinimum));
			if (fields.ActivityStartTimeMaximum != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.preference-activity-start-time-max').timepicker('setTime', '{0}');", fields.ActivityStartTimeMaximum));
			if (fields.ActivityEndTimeMinimum != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.preference-activity-end-time-min').timepicker('setTime', '{0}');", fields.ActivityEndTimeMinimum));
			if (fields.ActivityEndTimeMaximum != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.preference-activity-end-time-max').timepicker('setTime', '{0}');", fields.ActivityEndTimeMaximum));
			if (fields.ActivityTimeMinimum != null)
				Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".preference-activity-extended-work-time-min", fields.ActivityTimeMinimum);
			if (fields.ActivityTimeMaximum != null)
				Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".preference-activity-extended-work-time-max", fields.ActivityTimeMaximum);
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
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-start-time-min').val() === '';", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-start-time-max').val() === '';", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-end-time-min').val() === '';", "True");
			Browser.Interactions.AssertExists(".preference-end-time-min-next-day:not(:enabled):not(.active)");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-end-time-max').val() === '';", "True");
			Browser.Interactions.AssertExists(".preference-end-time-max-next-day:not(:enabled):not(.active)");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-extended-work-time-min').val() === '';", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-extended-work-time-max').val() === '';", "True");

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

			var tooltip = ".tooltip.top.in";
			Browser.Interactions.AssertExistsUsingJQuery(tooltip);

			if (fields.EndTimeMaximum != null) Browser.Interactions.AssertFirstContainsUsingJQuery(tooltip, fields.EndTimeMaximum);

			// sometimes the tooltip will hide, and will lose the tooltip content
			var resultHtml = Browser.Interactions.Javascript_IsFlaky("return $(\"" + tooltip + "\").html()");

			if (fields.Preference != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.Preference));
			if (fields.StartTimeMinimum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.StartTimeMinimum));
			if (fields.StartTimeMaximum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.StartTimeMaximum));
			if (fields.EndTimeMinimum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.EndTimeMinimum));
			if (fields.EndTimeMaximum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.EndTimeMaximum));
			if (fields.WorkTimeMinimum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.WorkTimeMinimum));
			if (fields.WorkTimeMaximum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.WorkTimeMaximum));

			if (fields.Activity != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.Activity));
			if (fields.ActivityStartTimeMinimum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.ActivityStartTimeMinimum));
			if (fields.ActivityStartTimeMaximum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.ActivityStartTimeMaximum));
			if (fields.ActivityEndTimeMinimum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.ActivityEndTimeMinimum));
			if (fields.ActivityEndTimeMaximum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.ActivityEndTimeMaximum));
			if (fields.ActivityTimeMinimum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.ActivityTimeMinimum));
			if (fields.ActivityTimeMaximum != null && resultHtml != null) Assert.IsTrue(resultHtml.Contains(fields.ActivityTimeMaximum));
		}

		private static void OpenExtendedTooltip(DateTime date)
		{
			var extendedIndication = ExtendedIndication(date);
			Browser.Interactions.AssertVisibleUsingJQuery(extendedIndication);
			Browser.Interactions.Javascript("$('{0}').trigger('mouseleave')", extendedIndication.JSEncode());
			Browser.Interactions.Javascript("$('{0}').trigger('mouseenter')", extendedIndication.JSEncode());
		}

		private void AssertExtendedActivityTimeFieldsAreReset()
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-start-time-min').val() === '';", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-start-time-max').val() === '';", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-end-time-min').val() === '';", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-end-time-max').val() === '';", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-extended-work-time-min').val() === '';", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.preference-activity-extended-work-time-max').val() === '';", "True");
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
			Browser.Interactions.ClickUsingJQuery(".submenu .add-musthave");
		}

		[When(@"I click remove must have button")]
		public void WhenIClickOnRemoveMustHaveButton()
		{
			Browser.Interactions.ClickUsingJQuery(".submenu .remove-musthave");
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
					string.Format("return $('.preference-template-list option:contains(\"{0}\")').length > 0;", preference.Value), "True"));
		}

		[Then(@"I should not see '(.*)' in templates list")]
		public void ThenIShouldNotSeeInTemplatesList(string name)
		{
			Browser.Interactions.AssertJavascriptResultContains(
				string.Format("return $('.preference-template-list option:contains(\"{0}\")').length === 0;", name), "True");
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
			var cell = CalendarCells.DateSelector(fields.Date);
			if (fields.StartTimeBoundry != null)
				Browser.Interactions.AssertAnyContains(string.Format("{0} .{1}", cell, "possible-start-times"), fields.StartTimeBoundry);
			if (fields.EndTimeBoundry != null)
				Browser.Interactions.AssertAnyContains(string.Format("{0} .{1}", cell, "possible-end-times"), fields.EndTimeBoundry);
			if (fields.ContractTimeBoundry != null)
				Browser.Interactions.AssertAnyContains(string.Format("{0} .{1}", cell, "possible-contract-times"), fields.ContractTimeBoundry);
			if (fields.FeedbackError != null)
				Browser.Interactions.AssertAnyContains(string.Format("{0} .{1}", cell, "feedback-error"), Resources.NoAvailableShifts);
		}

		[Then(@"I should see no preference feedback on '(.*)'")]
		public void ThenIShouldSeeNoFeedback(DateTime date)
		{
			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0}.{1}", cell, "feedback-error"));
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0}.{1}", cell, "possible-start-times"));
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0}.{1}", cell, "possible-end-times"));
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0}.{1}", cell, "possible-contract-times"));
		}
	}
}