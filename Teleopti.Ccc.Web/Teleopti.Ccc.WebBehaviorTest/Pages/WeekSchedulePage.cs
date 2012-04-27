using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using WatiN.Core.Constraints;
using List = WatiN.Core.List;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class WeekSchedulePage : PortalPage, IOkButton, ICancelButton, IEditTextRequestPage, IDateRangeSelector
	{
		private Constraint DayConstraint = Find.By("data-mytime-date", v => v != null);
		private ListCollection DayLists { get { return Document.Lists.Filter(DayConstraint); } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IEnumerable<List> DayElements
		{
			get
			{
				if (DayLists.Count == 7)
					return DayLists;
				throw new Exception(DayLists.Count + " day elements found. Should be 7.");
			}
		}

		public List FirstDay { get { return Document.List(DayConstraint).EventualGet(); } }
		public List SecondDay { get { return DayElements.ElementAt(1); } }
		public List ThirdDay { get { return DayElements.ElementAt(2); } }
		public List FourthDay { get { return DayElements.ElementAt(3); } }
		public List Fifith { get { return DayElements.ElementAt(4); } }
		public List SixthDay { get { return DayElements.ElementAt(5); } }
		public List SeventhDay { get { return DayElements.ElementAt(6); } }

		public string FirstDate { get { return FirstDay.GetAttributeValue("data-mytime-date"); } }

		public List DayElementForDate(string formattedDate) { return Document.List(Find.By("data-mytime-date", v => v == formattedDate)).EventualGet(); }
		public List DayElementForDate(DateTime date) { return DayElementForDate(date.ToString("yyyy-MM-dd")); }

		public Div SecondDayComment { get { return SecondDay.Div(Find.ByClass("comment-day")); } }

		public void ClickThirdDayOfOtherWeekInWeekPicker(CultureInfo culture)
		{
			DatePicker.ClickDay(new DateOnly(TestDataSetup.ThirdDayOfOtherThanCurrentWeekInCurrentMonth(culture)));
		}

		public Div TextRequestForDate(DateTime date)
		{
			return DayElementForDate(date).Div(Find.ByClass("text-request", false));
		}

		[FindBy(Id = "Schedule-addRequest-button")]
		public Button AddTextRequestButton { get; set; }

		[FindBy(Id = "Schedule-addRequest-section")]
		public Div RequestDetailSection { get; set; }

		[FindBy(Id = "Schedule-addRequest-subject-input")]
		public TextField TextRequestDetailSubjectInput { get; set; }
		[FindBy(Id = "Schedule-addRequest-fromDate-input")]
		public TextField TextRequestDetailFromDateInput { get; set; }
		[FindBy(Id = "Schedule-addRequest-fromTime-input-input")]
		public TextField TextRequestDetailFromTimeTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-toDate-input")]
		public TextField TextRequestDetailToDateTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-toTime-input-input")]
		public TextField TextRequestDetailToTimeTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-message-input")]
		public TextField TextRequestDetailMessageTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-error")]
		public Div ValidationErrorText { get; set; }

		[FindBy(Id = "Schedule-addRequest-ok-button")]
		public Button OkButton { get; set; }

		public Element CancelButton {
			get { return Document.Element(Find.ByClass("ui-tooltip-close", false)); }
		}

		//not yet used
		public TextField TextRequestDetailEntityId { get; private set; }


		[FindBy(Id = "WeekScheduleDateRangeSelector")] public Div DateRangeSelectorContainer { get; set; }
		[FindBy(Id = "ScheduleDatePicker")] public DatePicker DatePicker { get; set; }
		public Button NextPeriodButton { get { return DateRangeSelectorContainer.Buttons.Last(); } }
		public Button PreviousPeriodButton { get { return DateRangeSelectorContainer.Buttons.First(); } }
	}
}