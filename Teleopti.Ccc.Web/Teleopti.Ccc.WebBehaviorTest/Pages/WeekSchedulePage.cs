using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using WatiN.Core.Constraints;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using List = WatiN.Core.List;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class WeekSchedulePage : PortalPage, IOkButton, ICancelButton, IEditRequestPage
	{
		private readonly Constraint DayConstraint = Find.By("data-mytime-date", v => v != null);
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

		private string DateSelector(DateTime date) { return CalendarCells.DateSelector(date); }

		public List DayElementForDate(DateTime date)
		{
			return Document.List(Find.BySelector(DateSelector(date))).EventualGet();
		}

		public Div DayComment(DateTime date) 
		{
			return Document.Div(Find.BySelector(DateSelector(date) + " .icon.comment-day")).EventualGet();
		}

		public void ClickThirdDayOfOtherWeekInWeekPicker(CultureInfo culture)
		{
			DatePicker.ClickDay(new DateOnly(TestDataSetup.ThirdDayOfOtherThanCurrentWeekInCurrentMonth(culture)));
		}

		public Div RequestForDate(DateTime date)
		{
			return Document.Div(Find.BySelector(DateSelector(date) + " .text-request")).EventualGet();
		}

		public Div HolidayAgentsForDate(DateTime date)
		{
			return Document.Div(Find.BySelector(DateSelector(date) + " .holiday-agents")).EventualGet();
		}
		public Span AddRequestDropDown { get; set; }
		public Link AddTextRequestMenuItem { get; set; }
		public Link AddAbsenceRequestMenuItem { get; set; }
		public Link AddShiftTradeRequestMenuItem { get; set; }

		
		[FindBy(Id = "Schedule-addRequest-section")]
		public Div RequestDetailSection { get; set; }

		[FindBy(Id = "Text-request-tab")]
		public Span TextRequestTab { get; set; }

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

		public Div TimeLineDiv
		{
			get { return Document.Div(QuicklyFind.ByClass("weekview-timeline")); }
		}
		
		public DivCollection TimelineLabels
		{
			get
			{
				return TimeLineDiv.Divs.Filter(QuicklyFind.ByClass("weekview-timeline-label")); 
			}
		}

		public Div AnyTimelineLabel
		{
			get
			{
				return Document.Div(QuicklyFind.ByClass("weekview-timeline-label"));
			}
		}

		public Element CancelButton
		{
			get { return Document.Element(QuicklyFind.ByClass("qtip-close")); }
		}
		
		//not yet used
		public Span RequestDetailDenyReason { get; private set; }

		//not yet used
		public TextField RequestDetailEntityId { get; private set; }


		[FindBy(Id = "ScheduleDateRangeSelector")] public Div DateRangeSelectorContainer { get; set; }
		[FindBy(Id = "ScheduleDatePicker")] public DatePicker DatePicker { get; set; }
		public Button NextPeriodButton { get { return DateRangeSelectorContainer.Buttons.Last(); } }
		public Button PreviousPeriodButton { get { return DateRangeSelectorContainer.Buttons.First(); } }
        
		public DivCollection DayLayers(DateTime date)
        {
			return Document.Divs.Filter(Find.BySelector(DateSelector(date) + " .weekview-day-schedule-layer"));
        }

		public Div DayLayerTooltipElement(DateTime date, string tooltipContent)
		{
			return Document.Div(Find.BySelector(DateSelector(date) + " .weekview-day-schedule-layer[tooltip-text*='" + tooltipContent + "']"));
		}
	}
}