using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using AggregateException = System.AggregateException;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class HangfireEventPublisher : IEventPublisher, IRecurringEventPublisher
	{
		private readonly IHangfireEventClient _client;
		private readonly ResolveEventHandlers _resolver;
		private readonly ICurrentDataSource _dataSource;
		private readonly PersistedTypeMapper _typeMapper;

		public HangfireEventPublisher(
			IHangfireEventClient client,
			ResolveEventHandlers resolver,
			ICurrentDataSource dataSource,
			PersistedTypeMapper typeMapper)
		{
			_client = client;
			_resolver = resolver;
			_dataSource = dataSource;
			_typeMapper = typeMapper;
		}

		public void Publish(params IEvent[] events)
		{
			try
			{
				var hangfireEventJobs = jobsFor(events).ToArray();
				Parallel.ForEach(hangfireEventJobs, j => _client.Enqueue(j));
			}
			catch (AggregateException e)
			{
				throw e.Flatten().InnerExceptions.FirstOrDefault();
			}
		}

		public void PublishDaily(IEvent @event) =>
			jobsFor(new[] {@event}).ForEach(x => _client.AddOrUpdateDaily(x));

		public void PublishHourly(IEvent @event) =>
			jobsFor(new[] {@event}).ForEach(x => _client.AddOrUpdateHourly(x));

		public void PublishMinutely(IEvent @event) =>
			jobsFor(_resolver.MinutelyRecurringJobsFor<IRunOnHangfire>(@event))
				.ForEach(x => _client.AddOrUpdateMinutely(x));

		private IEnumerable<HangfireEventJob> jobsFor(IEnumerable<IEvent> events) =>
			jobsFor(_resolver.JobsFor<IRunOnHangfire>(events));

		private IEnumerable<HangfireEventJob> jobsFor(IEnumerable<IJobInfo> jobInfos)
		{
			var tenant = _dataSource.CurrentName();
			var tenantDisplayName = (string.IsNullOrWhiteSpace(tenant) ? "ALL" : tenant);
			return jobInfos
				.Select(x =>
					{
						var eventDisplayName = x.Event?.GetType().Name ?? "a package";
						var handlerTypeName = _typeMapper.NameForPersistence(x.HandlerType);
						return new HangfireEventJob
						{
							DisplayName = $"{handlerTypeName} got {eventDisplayName} on {tenantDisplayName}",
							Tenant = tenant,
							Event = x.Event,
							Package = x.Package,
							HandlerTypeName = handlerTypeName,
							QueueName = x.QueueName,
							Attempts = x.Attempts,
							AllowFailures = x.AllowFailures,
							RunInterval = x.RunInterval
						};
					}
				);
		}

		public void StopPublishingForTenantsExcept(IEnumerable<string> excludedTenants)
		{
			var prefixesToKeep = excludedTenants
				.Select(HangfireEventJob.TenantPrefixForTenant)
				.ToList();

			foreach (var job in _client.GetRecurringJobIds())
			{
				var prefix = HangfireEventJob.TenantPrefixForRecurringId(job);
				if (string.IsNullOrWhiteSpace(prefix))
					continue;
				if (!prefixesToKeep.Contains(prefix))
					_client.RemoveIfExists(job);
			}
		}

		public void StopPublishingAll()
		{
			_client.GetRecurringJobIds()
				.ForEach(x => _client.RemoveIfExists(x));
		}
	}
}