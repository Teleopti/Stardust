namespace Teleopti.Ccc.Domain.Status
{
	public class CheckLegacySystemStatus : IStatusStep
	{
		private readonly ICallLegacySystemStatus _callLegacySystemStatus;
		public const string FailureOutput = "System is down!";
		public const string SuccessOutput = "System is up!";

		public CheckLegacySystemStatus(ICallLegacySystemStatus callLegacySystemStatus)
		{
			_callLegacySystemStatus = callLegacySystemStatus;
		}

		public StatusStepResult Execute()
		{
			var result = _callLegacySystemStatus.Execute();
			return result ? 
				new StatusStepResult(true, SuccessOutput) : 
				new StatusStepResult(false, FailureOutput);
		}

		public string Name { get; } = "SystemUp";
		public string Description { get; } = "Verifies that users can access the web and sign in.";
	}
}