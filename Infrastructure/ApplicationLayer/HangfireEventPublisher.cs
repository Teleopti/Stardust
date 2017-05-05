using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using batchRef = Teleopti.Ccc.Domain.Collection.Extensions;

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
			batchRef.Batch(events, 50)
				.Select(jobsFor)
				.ForEach(b => b.ForEach(j => _client.Enqueue(j)));
		}

		public void PublishDaily(IEvent @event)
		{
			jobsFor(@event).ForEach(x => _client.AddOrUpdateDaily(x));
		}

		public void PublishHourly(IEvent @event)
		{
			jobsFor(@event).ForEach(x => _client.AddOrUpdateHourly(x));
		}

		public void PublishMinutely(IEvent @event)
		{
			var jobs = _resolver.MinutelyRecurringJobsFor<IRunOnHangfire>(@event);
			jobsFor(jobs).ForEach(x => _client.AddOrUpdateMinutely(x));
		}

		private IEnumerable<HangfireEventJob> jobsFor(IEvent @event)
		{
			return jobsFor(new[] {@event});
		}

		private IEnumerable<HangfireEventJob> jobsFor(IEnumerable<IEvent> events)
		{
			return jobsFor(_resolver.JobsFor<IRunOnHangfire>(events));
		}

		private IEnumerable<HangfireEventJob> jobsFor(IEnumerable<IJobInfo> jobInfos)
		{
			var tenant = _dataSource.CurrentName();
			return jobInfos
				.Select(x =>
					{
						var eventDisplayName = x.Event?.GetType().Name ?? "a package";
						var tenantDisplayName = (string.IsNullOrWhiteSpace(tenant) ? "ALL" : tenant);
						return new HangfireEventJob
						{
							DisplayName = $"{x.HandlerType.Name} got {eventDisplayName} on {tenantDisplayName}",
							Tenant = tenant,
							Event = x.Event,
							Package = x.Package,
							HandlerTypeName = $"{x.HandlerType.FullName}, {x.HandlerType.Assembly.GetName().Name}",
							QueueName = x.QueueName,
							Attempts = x.Attempts,
							AllowFailures = x.AllowFailures
						};
					}
				);
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

	public class JobInfo
	{
		public HangfireEventJob Job;
		public AttemptsAttribute AttemptsAttribute;
		public AllowFailuresAttribute AllowFailuresAttribute;
	}
}