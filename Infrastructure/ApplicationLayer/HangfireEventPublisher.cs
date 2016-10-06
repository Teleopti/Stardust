using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
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
			events.SelectMany(jobsFor).ForEach(_client.Enqueue);
		}

		public void PublishHourly(IEvent @event)
		{
			jobsFor(@event).ForEach(j =>
			{
				j.Attempts = 1;
				_client.AddOrUpdateHourly(j);
			});
		}

		public void PublishMinutely(IEvent @event)
		{
			jobsFor(@event).ForEach(j =>
			{
				j.Attempts = 1;
				_client.AddOrUpdateMinutely(j);
			});
		}

		private IEnumerable<HangfireEventJob> jobsFor(IEvent @event)
		{
			var tenant = _dataSource.CurrentName();
			var eventType = @event.GetType();
			var handlerTypes = _resolver.HandlerTypesFor<IRunOnHangfire>(@event);

			return handlerTypes.Select(handlerType => new HangfireEventJob
			{
				DisplayName = $"{handlerType.Name} got {eventType.Name} on {(string.IsNullOrWhiteSpace(tenant) ? "ALL" : tenant)}",
				Tenant = tenant,
				Event = @event,
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