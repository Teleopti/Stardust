using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public static class WTFDEBUG
	{
		public static void Log(string stuff)
		{
			try
			{
				System.IO.File.AppendAllText(@"wtfdebug.log", stuff + Environment.NewLine);
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.AppendAllText(@"C:\wtfdebug.log", stuff + Environment.NewLine);
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.AppendAllText(@"C:\inetpub\wwwroot\PBI20491-AgentPortalWeb\wtfdebug.log", stuff + Environment.NewLine);
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.AppendAllText(@"C:\Program Files (x86)\CruiseControl.NET\server\PBI20491\WorkingDirectory\wtfdebug.log", stuff + Environment.NewLine);
			}
			catch (Exception)
			{
			}
		}
	}

	public class ServiceBusEventPublisher : IEventPublisher
	{
		private readonly IServiceBusSender _sender;

		public ServiceBusEventPublisher(IServiceBusSender sender)
		{
			_sender = sender;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public void Publish(object @event)
		{
			WTFDEBUG.Log("ServiceBusEventPublisher " + @event.GetType().Name);
			if (!_sender.EnsureBus())
				throw new ApplicationException("Cant find the bus, cant publish the event!");
			WTFDEBUG.Log("ServiceBusEventPublisher Send " + @event.GetType().Name);
			_sender.Send(@event);
			WTFDEBUG.Log("/ServiceBusEventPublisher " + @event.GetType().Name);
		}
	}
}