namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class CheckLegacySystemStatus : IMonitorHealth
	{
		private readonly ICallLegacySystemStatus _callLegacySystemStatus;
		public const string FailureOutput = "SystemStatus is failing!";
		public const string SuccessOutput = "SystemStatus OK!";

		public CheckLegacySystemStatus(ICallLegacySystemStatus callLegacySystemStatus)
		{
			_callLegacySystemStatus = callLegacySystemStatus;
		}

		public MonitorResult Execute()
		{
			var result = _callLegacySystemStatus.Execute();
			return result ? 
				new MonitorResult(true, new[]{SuccessOutput}) : 
				new MonitorResult(false, new[]{FailureOutput});
		}
	}
}