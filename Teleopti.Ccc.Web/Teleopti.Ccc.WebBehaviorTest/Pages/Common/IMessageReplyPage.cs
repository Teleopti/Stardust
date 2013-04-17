using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface IMessageReplyPage : IOkButton
	{
		Div MessageDetailSection { get; }
		Label Title { get; }
		Label Message { get; }
		TextField Reply { get; }
		Div DialogueMessages { get; }
		Div ReplyOptionsDiv(int messagePositionInList);
	}
}