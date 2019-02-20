namespace Teleopti.Ccc.Domain.Status
{
	public class StatusStepResult
	{
		public StatusStepResult(bool success, string output)
		{
			Success = success;
			Output = output;
		}

		public bool Success { get; }
		public string Output { get; }
	}
}