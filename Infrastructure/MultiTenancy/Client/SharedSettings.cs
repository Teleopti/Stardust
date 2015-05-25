namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class SharedSettings
	{
		public string Queue { get; set; }
		public string MessageBroker { get; set; }
		public string MessageBrokerLongPolling { get; set; }
		public string RtaPollingInterval { get; set; }
		public string PasswordPolicy { get; set; }
	}
}