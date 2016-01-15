using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using NHibernate.Util;
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
			events.ForEach(e =>
			{
				jobsFor(e).ForEach(j =>
				{
					_client.Enqueue(j.DisplayName, j.Tenant, j.EventTypeName, j.Event, j.HandlerTypeName);
				});
			});
		}

		public void PublishHourly(IEvent @event)
		{
			jobsFor(@event).ForEach(j =>
			{
				_client.AddOrUpdateHourly(j.DisplayName, j.Tenant + ":::" + Guid.NewGuid(), j.Tenant, j.EventTypeName, j.Event, j.HandlerTypeName);
			});
		}

		private class jobInfo
		{
			public string DisplayName;
			public string Tenant;
			public string EventTypeName;
			public string Event;
			public string HandlerTypeName;
		}

		private IEnumerable<jobInfo> jobsFor(IEvent @event)
		{
			var tenant = _dataSource.CurrentName();
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
				yield return new jobInfo
				{
					DisplayName = displayName,
					Tenant = tenant,
					EventTypeName = eventTypeName,
					Event = serialized,
					HandlerTypeName = handlerTypeName
				};
			}
		}

		public void StopPublishingForCurrentTenant()
		{
			var tenant = _dataSource.CurrentName();
			var jobsToRemove =
				_client.GetRecurringJobIds()
				.Where(x => x.StartsWith(tenant + ":::"));
			jobsToRemove.ForEach(x => _client.RemoveIfExists(x));
		}

		public IEnumerable<string> TenantsWithRecurringJobs()
		{
			return _client.GetRecurringJobIds()
				.Select(x => x.Substring(0, x.IndexOf(":::")))
				.Distinct();
		}
	}

}