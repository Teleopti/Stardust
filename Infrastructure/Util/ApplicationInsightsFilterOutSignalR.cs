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