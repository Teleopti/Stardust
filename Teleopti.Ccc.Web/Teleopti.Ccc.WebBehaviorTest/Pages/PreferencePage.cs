using System;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class PreferencePage : CalendarCellsPage, IDateRangeSelector
	{
		[FindBy(Id = "PreferenceDateRangeSelector")] public Div DateRangeSelectorContainer { get; set; }

		public void ClickNext()
		{
			Browser.Interactions.Click(".icon-arrow-right");
		}

		public void ClickPrevious()
		{
			Browser.Interactions.Click(".icon-arrow-left");
		}

		[FindBy(Id = "PreferenceDatePicker")] public DatePicker DatePicker { get; set; }
		public Button NextPeriodButton { get { return DateRangeSelectorContainer.Buttons.Last(); } }
		public Button PreviousPeriodButton { get { return DateRangeSelectorContainer.Buttons.First(); } }

		[FindBy(Id = "Preference")]
		public SplitButton PreferenceButton { get; set; }

		[FindBy(Id = "Preference-add-extended-button")]
		public Button ExtendedPreferenceButton { get; set; }

		[FindBy(Id = "Preference-period")]
		public Div PreferencePeriod;

		[FindBy(Id = "qtip-add-extended")]
		public Div ExtendedPreferencePanel;
		[FindBy(Id = "Preference-template-container")]
		public SelectBox ExtendedPreferenceTemplateSelectBox;
		[FindBy(Id = "Preference-extended-preference-container")]
		public SelectBox ExtendedPreferenceSelectBox;
		[FindBy(Id = "Preference-extended-start-time-minimum")]
		public ComboBox ExtendedPreferenceStartTimeMinimum;
		[FindBy(Id = "Preference-extended-start-time-maximum")]
		public ComboBox ExtendedPreferenceStartTimeMaximum;
		[FindBy(Id = "Preference-extended-end-time-minimum")]
		public ComboBox ExtendedPreferenceEndTimeMinimum;
		[FindBy(Id = "Preference-extended-end-time-minimum-next-day")]
		public CheckBox ExtendedPreferenceEndTimeMinimumNextDay;
		[FindBy(Id = "Preference-extended-end-time-maximum")]
		public ComboBox ExtendedPreferenceEndTimeMaximum;
		[FindBy(Id = "Preference-extended-end-time-maximum-next-day")]
		public CheckBox ExtendedPreferenceEndTimeMaximumNextDay;
		[FindBy(Id = "Preference-extended-work-time-minimum")]
		public ComboBox ExtendedPreferenceWorkTimeMinimum;
		[FindBy(Id = "Preference-extended-work-time-maximum")]
		public ComboBox ExtendedPreferenceWorkTimeMaximum;
		[FindBy(Id = "Preference-extended-activity-container")]
		public SelectBox ExtendedPreferenceActivity;
		[FindBy(Id = "Preference-extended-activity-start-time-minimum")]
		public ComboBox ExtendedPreferenceActivityStartTimeMinimum;
		[FindBy(Id = "Preference-extended-activity-start-time-maximum")]
		public ComboBox ExtendedPreferenceActivityStartTimeMaximum;
		[FindBy(Id = "Preference-extended-activity-end-time-minimum")]
		public ComboBox ExtendedPreferenceActivityEndTimeMinimum;
		[FindBy(Id = "Preference-extended-activity-end-time-maximum")]
		public ComboBox ExtendedPreferenceActivityEndTimeMaximum;
		[FindBy(Id = "Preference-extended-activity-time-minimum")]
		public ComboBox ExtendedPreferenceActivityTimeMinimum;
		[FindBy(Id = "Preference-extended-activity-time-maximum")]
		public ComboBox ExtendedPreferenceActivityTimeMaximum;
		[FindBy(Id = "Template-save")]
		public Div TemplateSaveDiv;

		[FindBy(Id = "Preference-extended-error")]
		public Div ExtendedPreferencePanelError { get; set; }

		[FindBy(Id = "Preference-period-feedback-view")]
		public Div PreferencePeriodFeedbackView { get; set; }

		[FindBy(Id = "Preference-extended-apply")]
		public Button ExtendedPreferenceApplyButton { get; set; }
		[FindBy(Id = "Preference-extended-reset")]
		public Button ExtendedPreferenceResetButton { get; set; }
		[FindBy(Id = "Preference-extended-save-template")]
		public Button ExtendedPreferenceSaveTemplateButton { get; set; }

		public void SelectPreferenceItemByText(string text, bool wait)
		{
			if (wait)
				PreferenceButton.SelectWait(text);
			else
				PreferenceButton.Select(text);
		}

		public Span DeleteSpanForTemplate(string templateName)
		{
			// in order to find fast and also it has only one delete button, templateName isn't used here
			return Document.Div("Preference-template-menu").Span(QuicklyFind.ByClass("menu-icon-delete")).EventualGet();
		}

		public Div ExtendedPreferenceIndicationForDate(DateTime date)
		{
			return CalendarCellForDate(date).Div(QuicklyFind.ByClass("extended-indication"));
		}

		public Div MeetingAndPersonalShiftIndicationForDate(DateTime date)
		{
			return CalendarCellForDate(date).Div(QuicklyFind.ByClass("icon-user")).EventualGet();
		}

		public Div MeetingAndPersonalShiftTooltipForDate(DateTime date)
		{
			return CalendarCellForDate(date).Div(QuicklyFind.ByClass("meeting-tooltip")).EventualGet();
		}
		
		public Div MeetingAndPersonalShiftForDate(DateTime date)
		{
			return Document.Div("qtip-meeting-" + date.ToString("yyyy-MM-dd")).EventualGet();
		}
	}
}