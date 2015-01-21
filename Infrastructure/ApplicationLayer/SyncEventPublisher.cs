using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncEventPublisher : ISyncEventPublisher, ICurrentEventPublisher
	{
		private readonly IResolveEventHandlers _resolver;

		public SyncEventPublisher(IResolveEventHandlers resolver)
		{
			_resolver = resolver;
		}

		public void Publish(IEvent @event)
		{
			var handlers = _resolver.ResolveHandlersForEvent(@event);
			if (handlers == null) return;

			foreach (var handler in handlers)
			{
				var method = handler.GetType().GetMethods()
					.Single(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
				try
				{
					method.Invoke(handler, new[] { @event });
				}
				catch (TargetInvocationException e)
				{
					preserveStackTrace (e.InnerException) ;
					throw e;
				}
			}
		}

		private static void preserveStackTrace(Exception e)
		{
			var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var mgr = new ObjectManager(null, ctx);
			var si = new SerializationInfo(e.GetType(), new FormatterConverter());

			e.GetObjectData(si, ctx);
			mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
			mgr.DoFixups(); // ObjectManager calls SetObjectData

			// voila, e is unmodified save for _remoteStackTraceString
		}

		public IEventPublisher Current()
		{
			return this;
		}
	}
}