using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor
	{
		private readonly CommonEventProcessor _processor;

		public HangfireEventProcessor(CommonEventProcessor processor)
		{
			_processor = processor;
		}

		public void Process(string displayName, string tenant, IEvent @event, IEnumerable<IEvent> package, string handlerType)
		{
			var handlerT = Type.GetType(handlerType, true);
			Process(displayName, tenant, @event, package, handlerT);
		}

		public void Process(string displayName, string tenant, IEvent @event, IEnumerable<IEvent> package, Type handlerType)
		{
			_processor.Process(tenant, @event, package, handlerType);
		}
	}
}