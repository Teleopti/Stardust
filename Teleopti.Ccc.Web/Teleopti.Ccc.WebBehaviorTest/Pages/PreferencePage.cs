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

		[FindBy(Id = "Preference-delete-button")]
		public Button DeleteButton { get; set; }

		[FindBy(Id = "Preference-period")]
		public Div PreferencePeriod;

		[FindBy(Id = "Preference-extended-preference-container")]
		public SelectBox ExtendedPreferenceSelectBox;

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