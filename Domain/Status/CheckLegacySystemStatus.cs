namespace Teleopti.Ccc.Domain.Status
{
	public class CheckLegacySystemStatus : IStatusStep
	{
		private readonly ICallLegacySystemStatus _callLegacySystemStatus;
		public const string FailureOutput = "SystemStatus is failing!";
		public const string SuccessOutput = "SystemStatus OK!";

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

		public string Name { get; } = "SystemStatus";
		public string Description { get; } = "Verifies that users can access the web and sign in.";
	}
}