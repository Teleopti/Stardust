﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using WatiN.Core.Constraints;
using List = WatiN.Core.List;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class WeekSchedulePage : PortalPage, IOkButton, ICancelButton, IEditRequestPage, IDateRangeSelector
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
		public List FifthDay { get { return DayElements.ElementAt(4); } }
		public List SixthDay { get { return DayElements.ElementAt(5); } }
		public List SeventhDay { get { return DayElements.ElementAt(6); } }

		public string FirstDate { get { return FirstDay.GetAttributeValue("data-mytime-date"); } }

		public List DayElementForDate(string formattedDate) { return Document.List(Find.By("data-mytime-date", v => v == formattedDate)).EventualGet(); }
		public List DayElementForDate(DateTime date) { return DayElementForDate(date.ToString("yyyy-MM-dd")); }

		public Div SecondDayComment { get { return SecondDay.Div(Find.ByClass("icon comment-day", false)); } }

		public void ClickThirdDayOfOtherWeekInWeekPicker(CultureInfo culture)
		{
			DatePicker.ClickDay(new DateOnly(TestDataSetup.ThirdDayOfOtherThanCurrentWeekInCurrentMonth(culture)));
		}

		public Div RequestForDate(DateTime date)
		{
			return DayElementForDate(date).Div(Find.ByClass("text-request", false));
		}

		[FindBy(Id = "Schedule-addRequest-button")]
		public Button AddRequestButton { get; set; }

		[FindBy(Id = "Schedule-addRequest-section")]
		public Div RequestDetailSection { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public Span TextRequestTab
		{
			get { throw new NotImplementedException(); }
		}

		[FindBy(Id = "Absence-request-tab")]
		public Span AbsenceRequestTab { get; set; }

		[FindBy(Id = "Absence-type-element")]
		public Div AbsenceTypesElement { get; set; }

		[FindBy(Id = "Absence-type-input")]
		public TextField AbsenceTypesTextField { get; set; }
		[FindBy(Id = "Absence-type")]
		public SelectList AbsenceTypesSelectList { get; set; }
		[FindBy(Id = "Fullday-check")]
		public CheckBox FulldayCheck { get; set; }

		[FindBy(Id = "Schedule-addRequest-subject-input")]
		public TextField RequestDetailSubjectInput { get; set; }
		[FindBy(Id = "Schedule-addRequest-fromDate-input")]
		public TextField RequestDetailFromDateTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-fromTime-input-input")]
		public TextField RequestDetailFromTimeTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-toDate-input")]
		public TextField RequestDetailToDateTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-toTime-input-input")]
		public TextField RequestDetailToTimeTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-message-input")]
		public TextField RequestDetailMessageTextField { get; set; }
		[FindBy(Id = "Schedule-addRequest-error")]
		public Div ValidationErrorText { get; set; }

		[FindBy(Id = "Schedule-addRequest-ok-button")]
		public Button OkButton { get; set; }

		public DivCollection TimelineLabels
		{
			get
			{
				var timelineDiv = Document.Div(Find.ByClass("weekview-timeline", false));
				return timelineDiv.Divs.Filter(Find.ByClass("weekview-timeline-label", false)); 
			}
		}

		public Element CancelButton
		{
			get { return Document.Element(Find.ByClass("ui-tooltip-close", false)); }
		}

		public Div TimeIndicatorForDate(DateTime date)
		{
			return DayElementForDate(date).ListItems[4].Div(Find.ByClass("week-schedule-time-indicator", false));
		}

		public Div TimeIndicatorInTimeLine
		{
			get
			{
				var timelineDiv = Document.Div(Find.ByClass("weekview-timeline", false));
				return timelineDiv.Div(Find.ByClass("week-schedule-time-indicator-small", false)); 
			}
		}

		//not yet used
		public TextField RequestDetailEntityId { get; private set; }


		[FindBy(Id = "ScheduleDateRangeSelector")] public Div DateRangeSelectorContainer { get; set; }
		[FindBy(Id = "ScheduleDatePicker")] public DatePicker DatePicker { get; set; }
		public Button NextPeriodButton { get { return DateRangeSelectorContainer.Buttons.Last(); } }
		public Button PreviousPeriodButton { get { return DateRangeSelectorContainer.Buttons.First(); } }

		[FindBy(Id = "Schedule-today-button")]
		public Button TodayButton { get; set; }

		public DivCollection DayLayers(List numberOfDayInWeek)
		{
			return numberOfDayInWeek.ListItems[4].Divs.Filter(Find.ByClass("week-schedule-layer", false));
		}

        public DivCollection DayLayers(DateTime date)
        {
            return DayElementForDate(date).ListItems[4].Divs.Filter(Find.ByClass("week-schedule-layer", false));
        }
	}
}