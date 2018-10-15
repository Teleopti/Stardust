using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class ApplicationInsightsFilterSSO : ITelemetryProcessor
	{
		private ITelemetryProcessor Next { get; }

		public ApplicationInsightsFilterSSO(ITelemetryProcessor next)
		{
			Next = next;
		}

		public void Process(ITelemetry item)
		{
			if (item is DependencyTelemetry dependency && dependency.Name.Contains("OpenId/AskUser"))
			{
				var index = dependency.Name.LastIndexOf("/", StringComparison.Ordinal);
				if (index > 0)
					dependency.Name = dependency.Name.Substring(0, index) + "/collection_username";
			}

			Next.Process(item);
		}
	}
}
