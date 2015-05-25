using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class SharedSettings
	{
		public string Queue { get; set; }
		public string MessageBroker { get; set; }
		public string MessageBrokerLongPolling { get; set; }
		public string RtaPollingInterval { get; set; }
		public string PasswordPolicy { get; set; }

		public IDictionary<string, string> AddToAppSettings(IDictionary<string, string> appSettings)
		{
			var ret = new Dictionary<string, string>
			{
				{
					"Queue", Queue
				},
				{
					"MessageBroker", MessageBroker
				},
				{
					"MessageBrokerLongPolling", MessageBrokerLongPolling
				},
				{
					"RtaPollingInterval", RtaPollingInterval
				},
				{
					"PasswordPolicy", PasswordPolicy
				}
			};
			foreach (var key in appSettings.Keys)
			{
				ret[key] = appSettings[key];
			}
			return ret;
		}
	}
}