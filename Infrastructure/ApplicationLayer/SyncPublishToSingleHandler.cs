using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncPublishToSingleHandler : IEventPublisher
	{
		private readonly object _handler;

		public SyncPublishToSingleHandler(object handler)
		{
			_handler = handler;
		}

		public void Publish(IEvent @event)
		{
			var method = _handler
				.GetType()
				.GetMethods()
				.FirstOrDefault(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
			if (method == null)
				return;
			try
			{
				method.Invoke(_handler, new[] { @event });
			}
			catch (TargetInvocationException e)
			{
				preserveStackTrace(e.InnerException);
				throw e;
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
	}
}