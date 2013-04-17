using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MessagePage : PortalPage, IMessageReplyPage
	{
		public Div FriendlyMessage
		{
			get { return Document.Div(QuicklyFind.ByClass("friendly-message")); }
		}

		public DivCollection MessageBodyDivs
		{
			get { return Document.Divs.Filter(QuicklyFind.ByClass("bdd-asm-message-body")); }
		}

		public DivCollection MessageDetailDivs
		{
			get { return Document.Divs.Filter(QuicklyFind.ByClass("bdd-asm-message-detail")); }
		}

		public Div MessageDetailSection
		{
			get { return Document.Div(QuicklyFind.ByClass("asmMessage-edit-section")); }
		}

		[FindBy(Id = "AsmMessage-detail-title")]
		public Label Title { get; set; }

		[FindBy(Id = "AsmMessage-detail-message")]
		public Label Message { get; set; }

		[FindBy(Id = "AsmMessage-detail-reply")]
		public TextField Reply { get; set; }

		[FindBy(Id = "AsmMessage-detail-dialogueMessages")]
		public Div DialogueMessages { get; set; }

		public Button OkButton { get; set; }

		[FindBy(Id = "AsmMessage-detail-replyOption")]
		public Div ReplyOptions { get; set; }
	}
}