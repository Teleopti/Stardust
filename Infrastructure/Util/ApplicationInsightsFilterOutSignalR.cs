using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class ApplicationInsightsFilterOutSignalR : ITelemetryProcessor
	{
		private const string signalr = "/signalr/";
		private ITelemetryProcessor Next { get; }

		public ApplicationInsightsFilterOutSignalR(ITelemetryProcessor next)
		{
			Next = next;
		}

		public void Process(ITelemetry item)
		{
			if (item is DependencyTelemetry dependency && dependency.Name.StartsWith("PUT /SIGNALR_TOPIC"))
			{
				dependency.Name = "PUT /SIGNALR_TOPIC_collection";
			}

			var request = item as RequestTelemetry;
			if (request?.Url != null &&
				request.Url.AbsolutePath.IndexOf(signalr, StringComparison.InvariantCultureIgnoreCase) > -1)
			{
				return;
			}

			Next.Process(item);
		}
	}
}