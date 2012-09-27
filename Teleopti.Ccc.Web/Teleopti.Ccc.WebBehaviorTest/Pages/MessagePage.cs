using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Core;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MessagePage : PortalPage
	{
        [FindBy(Id = "friendly-message")] 
        public Div FriendlyMessage;

        [FindBy(Id = "Communications-list")]
        public List MessageList { get; set; }

        private Constraint MessageConstraint = Find.ByClass("message-item", false) && !Find.ByClass("template", false);
        private ListItemCollection MessageListItems { get { return Document.ListItems.Filter(MessageConstraint); } }
        public IEnumerable<ListItem> Messages { get { return MessageListItems; } }

        public ListItem FirstMessage { get { return Document.ListItem(MessageConstraint).EventualGet(); } }
        public ListItem LastMessage { get { return MessageListItems.Last(); } }

	}
}