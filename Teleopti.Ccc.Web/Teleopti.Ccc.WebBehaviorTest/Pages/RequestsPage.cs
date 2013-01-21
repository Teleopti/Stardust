using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class RequestsPage : PortalPage, IEditRequestPage, IOkButton, ICancelButton
	{
		[FindBy(Id = "Requests-list")]
		public List RequestsList { get; set; }

		private Constraint RequestConstraint = Find.BySelector(".request-item");
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
			return request.Button(QuicklyFind.ByClass("request-delete-button"));
		}

		[FindBy(Id = "Requests-showRequests-button")]
		public Button ShowRequestsButton { get; set; }

		[FindBy(Id = "Requests-addShiftTradeRequest-button")]
		public Button AddShiftTradeRequestButton { get; set; }

		[FindBy(Id = "Requests-addRequest-button")]
		public Button AddRequestButton { get; set; }

		[FindBy(Id = "Request-detail-section")]
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

		[FindBy(Id = "Request-detail-subject-input")]
		public TextField RequestDetailSubjectInput { get; set; }

		[FindBy(Id = "I-am-a-shifttrade")]
		public Div IamAShiftTrade { get; set; }

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

		[FindBy(Class = "arrow-down")]
		public Div MoreToLoadArrow { get; set; }

		[FindBy(Id = "Requests-no-requests-found")]
		public Div NoRequestsFound { get; set; }
		
		[FindBy(Id = "Request-add-shift-trade-button")]
		public Button ShiftTradeRequestsButton { get; set; }

		public Div FriendlyMessage
		{
			get { return Document.Div(QuicklyFind.ByClass("friendly-message")); }
		}

		public SpanCollection MyScheduleLayers
		{
			get { return Document.Div(QuicklyFind.ByClass("shift-trade-my-schedule")).Spans; }
		}

		[FindBy(Id = "Request-add-shift-trade-datepicker")]
		public TextField AddShiftTradeDatePicker
		{
			get { return Document.TextField(QuicklyFind.ByClass("shift-trade-add-datepicker")); }
		}

		public SpanCollection AddShiftTradeTimeLineItems
		{
			get { return Document.Div(QuicklyFind.ByClass("shift-trade-timeline")).Spans; }
		}
	}
}