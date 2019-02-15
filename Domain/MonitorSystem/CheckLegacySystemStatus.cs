namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class CheckLegacySystemStatus : IMonitorStep
	{
		private readonly ICallLegacySystemStatus _callLegacySystemStatus;
		public const string FailureOutput = "SystemStatus is failing!";
		public const string SuccessOutput = "SystemStatus OK!";

		public CheckLegacySystemStatus(ICallLegacySystemStatus callLegacySystemStatus)
		{
			_callLegacySystemStatus = callLegacySystemStatus;
		}

		public MonitorStepResult Execute()
		{
			var result = _callLegacySystemStatus.Execute();
			return result ? 
				new MonitorStepResult(true, SuccessOutput) : 
				new MonitorStepResult(false, FailureOutput);
		}
	}
}