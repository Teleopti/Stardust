using System;
using Microsoft.AspNet.SignalR;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	// makes the hub back portable to 390
	public class TestableHub : Hub
	{
	}
}