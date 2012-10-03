using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MessagePage : PortalPage, IMessageReplyPage
	{
        [FindBy(Id = "friendly-message")] 
        public Div FriendlyMessage;

        [FindBy(Id = "Communications-list")]
        public List MessageList { get; set; }

        private readonly Constraint _messageConstraint = Find.ByClass("communication-item", false) && !Find.ByClass("template", false);
		public ListItemCollection MessageListItems { get { return Document.ListItems.Filter(_messageConstraint); } }
        public IEnumerable<ListItem> Messages { get { return MessageListItems; } }

        [FindBy(Id = "Message-detail-section")]
        public Div MessageDetailSection { get; set; }

		[FindBy(Id = "Message-detail-title")]
		public Label Title { get; set; }

		[FindBy(Id = "Message-detail-message")]
		public Label Message { get; set; }
	}
}