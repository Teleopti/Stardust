﻿using System.Linq;
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

		[FindBy(Id = "Preference-delete-button")]
		public Button DeleteButton { get; set; }

		[FindBy(Id = "Preference-period")]
		public Div PreferencePeriod;

		[FindBy(Id = "Preference-period-feedback-shouldhave")]
		public Div PreferencePeriodFeedbackShouldHave { get; set; }

		[FindBy(Id = "Preference-period-feedback-canresult")]
		public Div PreferencePeriodFeedbackCanResult { get; set; }

		public void SelectPreferenceItemByText(string text, bool wait)
		{
			if (wait)
				PreferenceButton.SelectWait(text);
			else
				PreferenceButton.Select(text);
		}
	}
}