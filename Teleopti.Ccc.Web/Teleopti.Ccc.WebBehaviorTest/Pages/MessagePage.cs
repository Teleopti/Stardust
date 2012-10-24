using System.Collections.Generic;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MessagePage : PortalPage, IMessageReplyPage
	{
		public Div FriendlyMessage
		{
			get { return Document.Div(Find.ByClass("friendly-message", false)); }
		}

		[FindBy(Id = "AsmMessages-list")]
		public List MessageList { get; set; }

		public ListItemCollection MessageListItems { get { return Document.ListItems.Filter(Find.ByClass("asmMessage-item", false)); } }
		public IEnumerable<ListItem> Messages { get { return MessageListItems; } }

		public Div MessageDetailSection
		{
			get { return Document.Div(Find.ByClass("asmMessage-edit-section", false)); }
		}

		[FindBy(Id = "AsmMessage-detail-title")]
		public Label Title { get; set; }

		[FindBy(Id = "AsmMessage-detail-message")]
		public Label Message { get; set; }

		[FindBy(Id = "AsmMessage-detail-dialogueMessages")]
		public Div DialogueMessages { get; set; }

		[FindBy(Id = "AsmMessage-detail-reply")]
		public TextField Reply { get; set; }

		[FindBy(Id = "AsmMessage-detail-ok-button")]
		public Button OkButton { get; set; }
	}
}