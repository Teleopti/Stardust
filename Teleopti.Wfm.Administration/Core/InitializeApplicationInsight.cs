using System;
using Microsoft.ApplicationInsights.Extensibility;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Wfm.Administration.Core
{
	public class InitializeApplicationInsight
	{
		private readonly IConfigReader _configReader;

		public InitializeApplicationInsight(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public void Init()
		{
			var serverUrl = _configReader.AppConfig("Settings");
			var sharedSettingsQuerier = new SharedSettingsTenantClient(serverUrl);
			var sharedSetting = sharedSettingsQuerier.GetSharedSettings();
			if (Guid.TryParse(sharedSetting.InstrumentationKey, out var iKeyGuid) && iKeyGuid != Guid.Empty)
						TelemetryConfiguration.Active.InstrumentationKey = sharedSetting.InstrumentationKey;
		}
	}
}