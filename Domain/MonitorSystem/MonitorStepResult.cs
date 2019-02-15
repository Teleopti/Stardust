namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class MonitorStepResult
	{
		public MonitorStepResult(bool success, string output)
		{
			Success = success;
			Output = output;
		}

		public bool Success { get; }
		public string Output { get; }
	}
}