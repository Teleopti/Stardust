using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventPublisher : IEventPublisher, IRecurringEventPublisher
	{
		private readonly IHangfireEventClient _client;
		private readonly ResolveEventHandlers _resolver;
		private readonly ICurrentDataSource _dataSource;

		public HangfireEventPublisher(
			IHangfireEventClient client,
			ResolveEventHandlers resolver,
			ICurrentDataSource dataSource)
		{
			_client = client;
			_resolver = resolver;
			_dataSource = dataSource;
		}

		public void Publish(params IEvent[] events)
		{
			events.SelectMany(jobsFor).ForEach(x => _client.Enqueue(x.Job));
		}

		public void PublishDaily(IEvent @event)
		{
			jobsFor(@event).ForEach(x =>
			{
				_client.AddOrUpdateDaily(x.Job);
			});
		}

		public void PublishHourly(IEvent @event)
		{
			jobsFor(@event).ForEach(x =>
			{
				_client.AddOrUpdateHourly(x.Job);
			});
		}

		public void PublishMinutely(IEvent @event)
		{
			jobsFor(@event).ForEach(x =>
			{

				if (x.AttemptsAttribute != null)
					throw new Exception("Retrying minutely recurring job is a bad idea");
				x.Job.Attempts = 1;

				if (x.AllowFailuresAttribute == null)
					x.Job.AllowFailures = 10;

				_client.AddOrUpdateMinutely(x.Job);
			});
		}

		private class JobInfo
		{
			public HangfireEventJob Job;
			public AttemptsAttribute AttemptsAttribute;
			public AllowFailuresAttribute AllowFailuresAttribute;
		}

		private IEnumerable<JobInfo> jobsFor(IEvent @event)
		{
			var tenant = _dataSource.CurrentName();
			var eventType = @event.GetType();
			var handlerTypes = _resolver.HandlerTypesFor<IRunOnHangfire>(@event);

			return handlerTypes.Select(handlerType =>
			{
				var method = _resolver.HandleMethodFor(handlerType, @event);
				var attemptsAttribute = _resolver.GetAttemptsAttribute(handlerType, @event);
				var allowedFailuresAttribute = method
					.GetCustomAttributes(typeof(AllowFailuresAttribute), true).Cast<AllowFailuresAttribute>()
					.SingleOrDefault();
				return new JobInfo
				{
					Job = new HangfireEventJob
					{
						DisplayName = $"{handlerType.Name} got {eventType.Name} on {(string.IsNullOrWhiteSpace(tenant) ? "ALL" : tenant)}",
						Tenant = tenant,
						Event = @event,
						HandlerTypeName = $"{handlerType.FullName}, {handlerType.Assembly.GetName().Name}",
						QueueName = _resolver.QueueTo(handlerType, @event),
						Attempts = _resolver.AttemptsFor(attemptsAttribute),
						AllowFailures = allowedFailuresAttribute?.Failures ?? 0
					},
					AttemptsAttribute = attemptsAttribute,
					AllowFailuresAttribute = allowedFailuresAttribute
				};
			});
		}

		public void StopPublishingForTenantsExcept(IEnumerable<string> excludedTenants)
		{
			var hashed = excludedTenants.Select(x => x.GenerateGuid().ToString("N")).ToList();
			foreach (var job in _client.GetRecurringJobIds())
			{
				var tenantHash = HangfireEventJob.TenantHashForRecurringId(job);
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