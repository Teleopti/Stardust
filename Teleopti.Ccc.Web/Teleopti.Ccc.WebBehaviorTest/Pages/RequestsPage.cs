using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class RequestsPage : PortalPage, IEditRequestPage, IOkButton, ICancelButton
	{
		private Constraint RequestConstraint = Find.BySelector(".request-body");
		public DivCollection RequestListItems { get { return Document.Divs.Filter(RequestConstraint); } }
		public IEnumerable<Div> Requests { get { return RequestListItems; } }
		public Div FirstRequest { get { return Document.Div(RequestConstraint).EventualGet(); } }
		public Div LastRequest { get { return RequestListItems.Last(); } }

		public Div RequestById(Guid id)
		{
			return Document.Div(RequestConstraint && Find.By("data-mytime-requestid", id.ToString()));
		}

		public Button RequestDeleteButtonById(Guid Id)
		{
			var request = RequestById(Id);
			return request.Button(QuicklyFind.ByClass("close"));
		}

		public Button RequestsDeleteButton()
		{
			return Document.Button(QuicklyFind.ByClass("close"));
		}

		public Span AddRequestDropDown
		{
			get { return Document.Span(QuicklyFind.ByClass("toolbar-addRequest")); }
		}

		[FindBy(Id = "Requests-addTextRequest-menuItem")]
		public Link AddTextRequestMenuItem { get; set; }

		[FindBy(Id = "Request-detail-section")]
		public Div RequestDetailSection { get; set; }

		[FindBy(Id = "Text-request-tab")]
		public Span TextRequestTab { get; set; }

		[FindBy(Id = "Absence-request-tab")]
		public Span AbsenceRequestTab { get; set; }

		[FindBy(Id = "Absence-type-input")]
		public TextField AbsenceTypesTextField { get; set; }

		[FindBy(Id = "Absence-type")]
		public SelectList AbsenceTypesSelectList { get; set; }

		[FindBy(Id = "Fullday-check")]
		public CheckBox FulldayCheck { get; set; }

		[FindBy(Id = "Request-detail-subject-input")]
		public TextField RequestDetailSubjectInput { get; set; }

		[FindBy(Id = "Deny-shift-trade")]
		public Button DenyShiftTradeButton { get; set; }

		[FindBy(Class = "request-detail-title")]
		public Div RequestDetailTitle { get; set; }
		
		[FindBy(Id = "Request-detail-fromDate-input")]
		public TextField RequestDetailFromDateTextField { get; set; }
		
		[FindBy(Id = "Request-detail-fromTime-input-input")]
		public TextField RequestDetailFromTimeTextField { get; set; }
		
		[FindBy(Id = "Request-detail-toDate-input")]
		public TextField RequestDetailToDateTextField { get; set; }
		
		[FindBy(Id = "Request-detail-toTime-input-input")]
		public TextField RequestDetailToTimeTextField { get; set; }
		
		[FindBy(Id = "Request-detail-message-input")]
		public TextField RequestDetailMessageTextField { get; set; }
		
		[FindBy(Id = "Request-detail-error")]
		public Div ValidationErrorText { get; set; }

		[FindBy(Id = "Request-detail-ok-button")]
		public Button OkButton { get; set; }

		[FindBy(Id = "Request-detail-cancel-button")]
		public Element CancelButton { get; set; }

		[FindBy(Id = "Request-detail-entityid")]
		public TextField RequestDetailEntityId { get; set; }

		[FindBy(Id = "Request-detail-deny-reason")]
		public Span RequestDetailDenyReason { get; set; }

		[FindBy(Id = "Request-add-shift-trade-missing-wcs-message")]
		public Div AddShiftTradeMissingWorkflowControlsSetMessage { get; set; }

		[FindBy(Id = "Request-add-shift-trade-no-possible-trades")]
		public Div AddShiftTradeNoPossibleShiftTradesMessage { get; set; }

		public SpanCollection MyScheduleLayers
		{
			get { return Document.Div(QuicklyFind.ByClass("shift-trade-my-schedule")).Spans.Filter(QuicklyFind.ByClass("shift-trade-layer")); }
		}

		public SpanCollection ShiftTradeScheduleLayers
		{
			get { return Document.Div(QuicklyFind.ByClass("shift-trade-possible-trade-schedule")).Spans.Filter(QuicklyFind.ByClass("shift-trade-layer")); }
		}

		public SpanCollection ShiftTradeDetailsFromScheduleLayers
		{
			get { return Document.Divs.Filter(QuicklyFind.ByClass("shift-trade-swap-detail-schedule")).First().Spans.Filter(QuicklyFind.ByClass("shift-trade-layer")); }
		}

		public SpanCollection ShiftTradeDetailsToScheduleLayers
		{
			get { return Document.Divs.Filter(QuicklyFind.ByClass("shift-trade-swap-detail-schedule-to")).First().Spans.Filter(QuicklyFind.ByClass("shift-trade-layer")); }
		}

		public TextField AddShiftTradeDatePicker
		{
			get { return Document.TextField(QuicklyFind.ByClass("shift-trade-add-datepicker")); }
		}

		public SpanCollection AddShiftTradeTimeLineItems
		{
			get { return Document.Div(QuicklyFind.ByClass("shift-trade-timeline")).Spans.Filter(QuicklyFind.ByClass("shift-trade-timeline-line")); }
		}

		[FindBy(Id = "Request-shift-trade-sender")]
		public Span ShiftTradeSender { get; set; }

		[FindBy(Id = "Request-shift-trade-reciever")]
		public Span ShiftTradeReciever { get; set; }

		[FindBy(Id = "Request-shift-trade-date-from")]
		public Element ShiftTradeDateFrom { get; set; }

		[FindBy(Id = "Request-shift-trade-date-to")]
		public Element ShiftTradeDateTo { get; set; }

		[FindBy(Id = "Request-shift-trade-detail-info")]
		public Div  ShiftTradeRequestDetailInfo { get; set; }

		[FindBy(Id = "Request-add-shift-trade-subject-input")]
		public TextField AddShiftTradeSubject { get; set; }

		[FindBy(Id = "Request-add-shift-trade-message-input")]
		public TextField AddShiftTradeMessage { get; set; }

		[FindBy(Id = "Request-add-shift-trade")]
		public Div AddShiftTradeContainer { get; set; }

		public Span MyScheduleDayOff 
		{
			get { return Document.Spans.Filter(QuicklyFind.ByClass("shift-trade-dayoff-name")).First();  }
		}

		public Span OtherScheduleDayOff
		{
			get { return Document.Spans.Filter(QuicklyFind.ByClass("shift-trade-dayoff-name")).Skip(1).First(); }
		}


		public DivCollection Timelines
		{
			get
			{
				return Document.Divs.Filter(QuicklyFind.ByClass("shift-trade-swap-detail-timeline"));
			}
		}
	
	}
}