using System.Collections.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
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

		public ListItemCollection MessageListItems
		{
			get { return Document.ListItems.Filter(QuicklyFind.ByClass("asmMessage-item")); }
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

		[FindBy(Id = "AsmMessage-detail-ok-button")]
		public Button OkButton { get; set; }

		[FindBy(Id = "AsmMessage-detail-replyOption")]
		public Div ReplyOptions { get; set; }
	}
}