namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class TeleoptiProgressChangeMessage
	{
		public string Message { get; set; }

		public TeleoptiProgressChangeMessage(string message)
		{
			Message = message;
		}
	}
}