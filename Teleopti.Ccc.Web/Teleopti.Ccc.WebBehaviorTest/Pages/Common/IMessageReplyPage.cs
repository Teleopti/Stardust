using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface IMessageReplyPage : IOkButton
	{
		Div MessageDetailSection(int messagePositionInList);
		Span Message(int messagePositionInList);
		Div ReplyOptionsDiv(int messagePositionInList);
		TextField Reply(int messagePositionInList);
		Div DialogueMessages(int messagePositionInList);
	}
}