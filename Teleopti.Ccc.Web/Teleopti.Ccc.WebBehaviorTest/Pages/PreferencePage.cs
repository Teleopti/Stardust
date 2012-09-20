﻿using System;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class PreferencePage : CalendarCellsPage, IDateRangeSelector, IDeleteButton
	{
		[FindBy(Id = "PreferenceDateRangeSelector")] public Div DateRangeSelectorContainer { get; set; }
		[FindBy(Id = "PreferenceDatePicker")] public DatePicker DatePicker { get; set; }
		public Button NextPeriodButton { get { return DateRangeSelectorContainer.Buttons.Last(); } }
		public Button PreviousPeriodButton { get { return DateRangeSelectorContainer.Buttons.First(); } }

		[FindBy(Id = "Preference")]
		public SplitButton PreferenceButton { get; set; }

		[FindBy(Id = "Preference-add-extended-button")]
		public Button ExtendedPreferenceButton { get; set; }

		[FindBy(Id = "Preference-must-have-button")]
		public Button MustHaveButton { get; set; }
		[FindBy(Id = "Preference-must-have-delete-button")]
		public Button MustHaveDeleteButton { get; set; }

		[FindBy(Id = "Preference-delete-button")]
		public Button DeleteButton { get; set; }

		[FindBy(Id = "Preference-period")]
		public Div PreferencePeriod;

		[FindBy(Id = "ui-tooltip-add-extended")]
		public Div ExtendedPreferencePanel;
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

		[FindBy(Id = "Preference-extended-error")]
		public Div ExtendedPreferencePanelError { get; set; }

		[FindBy(Id = "Preference-period-feedback-view")]
		public Div PreferencePeriodFeedbackView { get; set; }

		[FindBy(Id = "Preference-extended-apply")]
		public Button ExtendedPreferenceApplyButton { get; set; }

		public void SelectPreferenceItemByText(string text, bool wait)
		{
			if (wait)
				PreferenceButton.SelectWait(text);
			else
				PreferenceButton.Select(text);
		}

		public Div ExtendedPreferenceIndicationForDate(DateTime date)
		{
			return CalendarCellForDate(date).Div(Find.ByClass("extended-indication", false));
		}
		
		public Div ExtendedPreferenceForDate(DateTime date)
		{
			return Document.Div("ui-tooltip-extended-" + date.ToString("yyyy-MM-dd"));
		}
	}
}