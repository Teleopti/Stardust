using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class MessagePage : PortalPage, IMessageReplyPage
	{
		public Div FriendlyMessage
		{
			get { return Document.Div(Find.BySelector(".bdd-show-message")); }
		}

		public DivCollection MessageBodyDivs
		{
			get { return Document.Divs.Filter(Find.BySelector(".bdd-asm-message-body")); }
		}

		public Button ConfirmButton(int messagePositionInList)
		{
			return MessageDetailSection(messagePositionInList).Button(Find.BySelector(".bdd-asm-message-confirm-button"));
		}

		public Div MessageDetailSection(int messagePositionInList)
		{
			return Document.Divs.Filter(Find.BySelector(".bdd-asm-message-detail"))[messagePositionInList - 1];
		}

		public Span Message(int messagePositionInList)
		{
			return MessageDetailSection(messagePositionInList).Span(Find.BySelector(".bdd-asm-message-detail-message"));
		}

		public TextField Reply(int messagePositionInList)
		{
			return ReplyOptionsDiv(messagePositionInList).TextField(Find.First());
		}

		public Div DialogueMessages(int messagePositionInList)
		{
			return MessageDetailSection(messagePositionInList).Div(Find.BySelector(".bdd-asm-message-detail-dialogue"));
		}

		public Button OkButton { get; set; }

		public Div ReplyOptionsDiv(int messagePositionInList)
		{
			return MessageDetailSection(messagePositionInList).Div(Find.BySelector(".bdd-asm-message-detail-reply"));
		}
	}
}