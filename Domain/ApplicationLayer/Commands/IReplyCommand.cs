namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IReplyCommand
	{
		string ReplyMessage { get; set; }
		bool IsReplySuccess { get; set; }
	}
}
