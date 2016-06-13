using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

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
					_client.Enqueue(j.DisplayName, j.Tenant, j.QueueName, j.EventTypeName, j.Event, j.HandlerTypeName);
				});
			});
		}

		public void PublishDaily(IEvent @event, TimeZoneInfo timeZone)
		{
			jobsFor(@event).ForEach(j =>
			{
				_client.AddOrUpdateDaily(j.DisplayName, idForJob(j, @event), j.Tenant, j.EventTypeName, j.Event, j.HandlerTypeName, timeZone);
			});
		}

		public void PublishHourly(IEvent @event)
		{
			jobsFor(@event).ForEach(j =>
			{
				_client.AddOrUpdateHourly(j.DisplayName, idForJob(j, @event), j.Tenant, j.EventTypeName, j.Event, j.HandlerTypeName);
			});
		}

		public void PublishMinutely(IEvent @event)
		{
			jobsFor(@event).ForEach(j =>
			{
				_client.AddOrUpdateMinutely(j.DisplayName, idForJob(j, @event), j.Tenant, j.EventTypeName, j.Event, j.HandlerTypeName);
			});
		}

		private static string idForJob(jobInfo j, IEvent @event)
		{
			var maxLength = 100 - "recurring-job:".Length;

			var handlerId = $"{j.HandlerType.Name}{delimiter}{@event.GetType().Name}";

			var id = $"{j.Tenant}{delimiter}{handlerId}";
			if (id.Length > maxLength)
			{
				throw new ArgumentException($"A recurring job cannot not have a long name. The maximum length is {maxLength} and now it is '{id}' with length {id.Length}. Please change class or event name.");
			}

			return id;
		}

		private class jobInfo
		{
			public string DisplayName;
			public string Tenant;
			public string EventTypeName;
			public string Event;
			public Type HandlerType;
			public string HandlerTypeName;
			public string QueueName { get; set; }
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
				DisplayName = $"{eventType.Name} to {handlerType.Name}",
				Tenant = tenant,
				EventTypeName = eventTypeName,
				Event = serialized,
				HandlerType = handlerType,
				HandlerTypeName = $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}",
				QueueName = QueueName.Default
			});
		}

		public IEnumerable<string> TenantsWithRecurringJobs()
		{
			return _client.GetRecurringJobIds()
				.Select(x => x.Substring(0, x.IndexOf(delimiter, StringComparison.Ordinal)))
				.Where(x => !string.IsNullOrEmpty(x))
				.Distinct();
		}

		public void StopPublishingForCurrentTenant()
		{
			var tenant = _dataSource.CurrentName();
			var jobsToRemove =
				_client.GetRecurringJobIds()
				.Where(x => x.StartsWith($"{tenant}{delimiter}"));
			jobsToRemove.ForEach(x => _client.RemoveIfExists(x));
		}

		public void StopPublishingAll()
		{
			_client.GetRecurringJobIds()
				.ForEach(x => _client.RemoveIfExists(x));
		}

		public void StopPublishingForEvent<T>() where T : IEvent
		{
			_client.GetRecurringJobIds()
				.Where(x => x.EndsWith($"{delimiter}{typeof(T).Name}"))
				.ForEach(x => _client.RemoveIfExists(x));
		}
	}
}