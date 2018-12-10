using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventProcessor
	{
		private readonly CommonEventProcessor _processor;
		private readonly HandlerTypeMapper _typeMapper;

		public HangfireEventProcessor(CommonEventProcessor processor, HandlerTypeMapper typeMapper)
		{
			_processor = processor;
			_typeMapper = typeMapper;
		}

		public void Process(string displayName, string tenant, IEvent @event, IEnumerable<IEvent> package, string handlerTypeName)
		{
			var handlerT = _typeMapper.TypeForPersistedName(handlerTypeName);
			Process(displayName, tenant, @event, package, handlerT);
		}

		public void Process(string displayName, string tenant, IEvent @event, IEnumerable<IEvent> package, Type handlerType)
		{
			_processor.Process(tenant, @event, package, handlerType);
		}
	}
}