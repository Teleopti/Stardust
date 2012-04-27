using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class RequestsPage : PortalPage, IEditTextRequestPage, IOkButton, ICancelButton
	{
		[FindBy(Id = "Requests-list")]
		public List RequestsList { get; set; }

		private Constraint RequestConstraint = Find.ByClass("request-item", false) && !Find.ByClass("template", false);
		private ListItemCollection RequestListItems { get { return Document.ListItems.Filter(RequestConstraint); } }
		public IEnumerable<ListItem> Requests { get { return RequestListItems; } }
		public ListItem FirstRequest { get { return Document.ListItem(RequestConstraint).EventualGet(); } }
		public ListItem LastRequest { get { return RequestListItems.Last(); } }

		public ListItem RequestById(Guid id)
		{
			return Document.ListItem(RequestConstraint && Find.By("data-mytime-requestid", id.ToString()));
		}

		public Button RequestDeleteButtonById(Guid Id)
		{
			var request = RequestById(Id);
			return request.Button(Find.ByClass("request-delete-button", false));
		}

		[FindBy(Id = "Requests-addTextRequest-button")]
		public Button AddTextRequestButton { get; set; }

		[FindBy(Id = "Request-detail-section")]
		public Div RequestDetailSection { get; set; }

		[FindBy(Id = "Request-detail-subject-input")]
		public TextField TextRequestDetailSubjectInput { get; set; }
		[FindBy(Id = "Request-detail-fromDate-input")]
		public TextField TextRequestDetailFromDateInput { get; set; }
		[FindBy(Id = "Request-detail-fromTime-input-input")]
		public TextField TextRequestDetailFromTimeTextField { get; set; }
		[FindBy(Id = "Request-detail-toDate-input")]
		public TextField TextRequestDetailToDateTextField { get; set; }
		[FindBy(Id = "Request-detail-toTime-input-input")]
		public TextField TextRequestDetailToTimeTextField { get; set; }
		[FindBy(Id = "Request-detail-message-input")]
		public TextField TextRequestDetailMessageTextField { get; set; }
		[FindBy(Id = "Request-detail-error")]
		public Div ValidationErrorText { get; set; }

		[FindBy(Id = "Request-detail-ok-button")]
		public Button OkButton { get; set; }
		[FindBy(Id = "Request-detail-cancel-button")]
		public Element CancelButton { get; set; }

		[FindBy(Id = "Request-detail-entityid")]
		public TextField TextRequestDetailEntityId { get; set; }
	}
}