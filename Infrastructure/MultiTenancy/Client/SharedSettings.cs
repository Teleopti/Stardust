using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class SharedSettings
	{
		public string Hangfire { get; set; }
		public string MessageBroker { get; set; }
		public string MessageBrokerLongPolling { get; set; }
		public string RtaPollingInterval { get; set; }
		public string PasswordPolicy { get; set; }
		public string InstrumentationKey { get; set; } = Guid.Empty.ToString();
		public int NumberOfDaysToShowNonPendingRequests { get; set; }
		public int MessageBrokerMailboxPollingIntervalInSeconds { get; set; }
		public int MessageBrokerMailboxExpirationInSeconds { get; set; }

		public IDictionary<string, string> AddToAppSettings(IDictionary<string, string> appSettings)
		{
			var ret = new Dictionary<string, string>
			{
				{
					"Hangfire", Hangfire
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
				},
				{
					"NumberOfDaysToShowNonPendingRequests", NumberOfDaysToShowNonPendingRequests.ToString()
				},
				{
					"MessageBrokerMailboxPollingIntervalInSeconds", MessageBrokerMailboxPollingIntervalInSeconds.ToString()
				},
				{
					"MessageBrokerMailboxExpirationInSeconds", MessageBrokerMailboxExpirationInSeconds.ToString()
				},
				{
					"InstrumentationKey", InstrumentationKey
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