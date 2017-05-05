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
				.ForEach(b => b.ForEach(j => _client.Enqueue(j.Job)));
		}

		public void PublishDaily(IEvent @event)
		{
			jobsFor(@event).ForEach(x => _client.AddOrUpdateDaily(x.Job));
		}

		public void PublishHourly(IEvent @event)
		{
			jobsFor(@event).ForEach(x => _client.AddOrUpdateHourly(x.Job));
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

		private IEnumerable<JobInfo> jobsFor(IEvent @event)
		{
			return jobsFor(new[] {@event});
		}

		private IEnumerable<JobInfo> jobsFor(IEnumerable<IEvent> events)
		{
			var tenant = _dataSource.CurrentName();
			var jobInfos = _resolver.JobsFor<IRunOnHangfire>(events);
			return jobInfos
				.Select(x =>
					{
						var @event = x.Event ?? x.Package.First();
						return new JobInfo
						{
							Job = new HangfireEventJob
							{
								DisplayName = $"{x.HandlerType.Name} got {@event.GetType().Name} on {(string.IsNullOrWhiteSpace(tenant) ? "ALL" : tenant)}",
								Tenant = tenant,
								Event = x.Event,
								Package = x.Package,
								HandlerTypeName = $"{x.HandlerType.FullName}, {x.HandlerType.Assembly.GetName().Name}",
								QueueName = x.Queue,
								Attempts = x.Attempts,
								AllowFailures = x.AllowFailures
							},
							AttemptsAttribute = x.AttemptsAttribute,
							AllowFailuresAttribute = x.AllowFailuresAttribute
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