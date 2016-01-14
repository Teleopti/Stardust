using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventPublisher : IHangfireEventPublisher, IRecurringEventPublisher
	{
		private readonly IHangfireEventClient _client;
		private readonly IJsonEventSerializer _serializer;
		private readonly IResolveEventHandlers _resolveEventHandlers;
		private readonly ICurrentDataSource _dataSource;
		private readonly bool _displayNames;

		public HangfireEventPublisher(
			IHangfireEventClient client, 
			IJsonEventSerializer serializer, 
			IResolveEventHandlers resolveEventHandlers,
			IConfigReader config,
			ICurrentDataSource dataSource)
		{
			_client = client;
			_serializer = serializer;
			_resolveEventHandlers = resolveEventHandlers;
			_dataSource = dataSource;
			_displayNames = config.ReadValue("HangfireDashboardDisplayNames", false);
		}

		public void Publish(params IEvent[] events)
		{
			var tenant = _dataSource.CurrentName();

			foreach (var @event in events)
			{
				var eventType = @event.GetType();
				var serialized = _serializer.SerializeEvent(@event);
				var eventTypeName = eventType.FullName + ", " + eventType.Assembly.GetName().Name;
				var handlers = _resolveEventHandlers.ResolveHandlersForEvent(@event).OfType<IRunOnHangfire>();

				foreach (var handler in handlers)
				{
					var handlerType = ProxyUtil.GetUnproxiedType(handler);
					var handlerTypeName = handlerType.FullName + ", " + handlerType.Assembly.GetName().Name;
					string displayName = null;
					if (_displayNames)
						displayName = eventType.Name + " to " + handlerType.Name;
					_client.Enqueue(displayName, tenant, eventTypeName, serialized, handlerTypeName);
				}
			}
		}

		public void PublishHourly(string id, string tenant, IEvent @event)
		{
			var eventType = @event.GetType();
			var serialized = _serializer.SerializeEvent(@event);
			var eventTypeName = eventType.FullName + ", " + eventType.Assembly.GetName().Name;
			var handlers = _resolveEventHandlers.ResolveHandlersForEvent(@event).OfType<IRunOnHangfire>();

			foreach (var handler in handlers)
			{
				var handlerType = ProxyUtil.GetUnproxiedType(handler);
				var handlerTypeName = handlerType.FullName + ", " + handlerType.Assembly.GetName().Name;
				string displayName = null;
				if (_displayNames)
					displayName = eventType.Name + " to " + handlerType.Name;
				_client.AddOrUpdate(displayName, id + ":::" + Guid.NewGuid(), tenant, eventTypeName, serialized, handlerTypeName);
			}
		}

		public void StopPublishing(string id)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> AllPublishings()
		{
			throw new System.NotImplementedException();
		}
	}

}