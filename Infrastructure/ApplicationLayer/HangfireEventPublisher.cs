using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventPublisher : IEventPublisher, IRecurringEventPublisher
	{
		private const string delimiter = ":::";
		private readonly IHangfireEventClient _client;
		private readonly IJsonEventSerializer _serializer;
		private readonly ResolveEventHandlers _resolver;
		private readonly ICurrentDataSource _dataSource;

		public HangfireEventPublisher(
			IHangfireEventClient client,
			IJsonEventSerializer serializer,
			ResolveEventHandlers resolver,
			ICurrentDataSource dataSource)
		{
			_client = client;
			_serializer = serializer;
			_resolver = resolver;
			_dataSource = dataSource;
		}

		public void Publish(params IEvent[] events)
		{
			events.ForEach(e =>
			{
				jobsFor(e).ForEach(j =>
				{
					_client.Enqueue(j.DisplayName, j.Tenant, j.QueueName, j.Attempts, j.EventTypeName, j.Event, j.HandlerTypeName);
				});
			});
		}

		public void PublishHourly(IEvent @event)
		{
			jobsFor(@event).ForEach(j =>
			{
				_client.AddOrUpdateHourly(j.DisplayName, j.IdForJob(@event), j.Tenant, j.EventTypeName, j.Event, j.HandlerTypeName);
			});
		}

		public void PublishMinutely(IEvent @event)
		{
			jobsFor(@event).ForEach(j =>
			{
				_client.AddOrUpdateMinutely(j.DisplayName, j.IdForJob(@event), j.Tenant, j.EventTypeName, j.Event, j.HandlerTypeName);
			});
		}

		private class jobInfo
		{
			public string DisplayName;
			public string Tenant;
			public string EventTypeName;
			public string Event;
			public Type HandlerType;
			public string HandlerTypeName;
			public string QueueName;
			public int Attempts;

			public string IdForJob(IEvent @event)
			{
				var hashedHandlerAndEvent = $"{HandlerType.Name}{delimiter}{@event.GetType().Name}".GenerateGuid().ToString("N");
				var hashedTenant = Tenant?.GenerateGuid().ToString("N") ?? "";
				return $"{hashedTenant}{delimiter}{hashedHandlerAndEvent}";
			}
		}

		private IEnumerable<jobInfo> jobsFor(IEvent @event)
		{
			var tenant = _dataSource.CurrentName();
			var eventType = @event.GetType();
			var serialized = _serializer.SerializeEvent(@event);
			var eventTypeName = $"{eventType.FullName}, {eventType.Assembly.GetName().Name}";
			var handlerTypes = _resolver.HandlerTypesFor<IRunOnHangfire>(@event);

			return handlerTypes.Select(handlerType => new jobInfo
			{
				DisplayName = $"{handlerType.Name} got {eventType.Name} on {(string.IsNullOrWhiteSpace(tenant) ? "ALL" : tenant)}",
				Tenant = tenant,
				EventTypeName = eventTypeName,
				Event = serialized,
				HandlerType = handlerType,
				HandlerTypeName = $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}",
				QueueName = _resolver.QueueTo(handlerType, @event),
				Attempts = _resolver
							   .HandleMethodFor(handlerType, @event)
							   .GetCustomAttributes(typeof(AttemptsAttribute), true)
							   .Cast<AttemptsAttribute>().SingleOrDefault()?.Attempts ?? 3
			});
		}

		public void StopPublishingForTenantsExcept(IEnumerable<string> excludedTenants)
		{
			var hashed = excludedTenants.Select(x => x.GenerateGuid().ToString("N")).ToList();
			foreach (var job in _client.GetRecurringJobIds())
			{
				var tenantHash = job.Substring(0, job.IndexOf(delimiter, StringComparison.Ordinal));
				if (!string.IsNullOrWhiteSpace(tenantHash) && !hashed.Contains(tenantHash))
				{
					_client.RemoveIfExists(job);
				}
			}
		}

		public void StopPublishingAll()
		{
			_client.GetRecurringJobIds()
				.ForEach(x => _client.RemoveIfExists(x));
		}
	}
}