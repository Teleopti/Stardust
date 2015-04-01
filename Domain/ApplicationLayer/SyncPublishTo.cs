using System.Linq;
using System.Reflection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class SyncPublishTo : IEventPublisher
	{
		private readonly object[] _handlers;

		public SyncPublishTo(object handler)
		{
			_handlers = new[] { handler };
		}

		public SyncPublishTo(object[] handlers)
		{
			_handlers = handlers;
		}

		public void Publish(IEvent @event)
		{
			foreach (var handler in _handlers)
			{
				var method = handler
					.GetType()
					.GetMethods()
					.FirstOrDefault(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
				if (method == null)
					continue;
				try
				{
					method.Invoke(handler, new[] { @event });
				}
				catch (TargetInvocationException e)
				{
					PreserveStack.ForInnerOf(e);
					throw e;
				}
			}
		}
		
	}
}