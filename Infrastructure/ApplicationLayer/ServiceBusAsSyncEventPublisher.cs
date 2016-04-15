using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusAsSyncEventPublisher : IEventPublisher
	{
		private readonly ServiceBusEventProcessor _processor;

		public ServiceBusAsSyncEventPublisher(ServiceBusEventProcessor processor)
		{
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var thread = new Thread(() =>
				{
					ProcessLikeTheBus(@event);
				});
				thread.Start();
				thread.Join();
			}
		}

		[AsSystem]
		protected virtual void ProcessLikeTheBus(IEvent @event)
		{
			_processor.Process(@event);
		}
	}
}