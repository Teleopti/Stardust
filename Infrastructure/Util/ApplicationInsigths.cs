using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class ApplicationInsigths : IApplicationInsights 
	{
		private readonly ISharedSettingsTenantClient _sharedSettingsQuerier;
		private readonly TelemetryClient _telemetryClient;

		public ApplicationInsigths(ISharedSettingsTenantClient sharedSettingsQuerier, TelemetryClient telemetryClient)
		{
			_sharedSettingsQuerier = sharedSettingsQuerier;
			_telemetryClient = telemetryClient;
		}

		public void Init()	
		{
			var iKey = _sharedSettingsQuerier.GetSharedSettings().InstrumentationKey;
			if (Guid.TryParse(iKey, out var iKeyGuid) && iKeyGuid != Guid.Empty)
				TelemetryConfiguration.Active.InstrumentationKey = iKey;
			TrackEvent("Application Insight Inititialized.");
		}

		public void TrackEvent(string description)
		{
			_telemetryClient.TrackEvent(description);
		}
	}
}
