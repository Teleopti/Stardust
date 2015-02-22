namespace Teleopti.Ccc.WinCode.Scheduling
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