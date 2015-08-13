using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	[HubName("forecastHub")]
	[CLSCompliant(false)]
	public class ForecastHub : Hub
	{
	}
}