using System;
using System.Collections.Generic;
using Hangfire;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	[CLSCompliant(false)]
	public class HangfireEventClient : IHangfireEventClient
	{
		private readonly Lazy<IBackgroundJobClient> _jobClient;

		public HangfireEventClient(Lazy<IBackgroundJobClient> jobClient)
		{
			_jobClient = jobClient;
		}

		public void Enqueue(string displayName, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			_jobClient.Value.Enqueue<HangfireEventServer>(x => x.Process(displayName, tenant, eventType, serializedEvent, handlerType));
		}

		public void AddOrUpdate(string displayName, string id, string tenant, string eventType, string serializedEvent, string handlerType)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> GetRecurringJobIds()
		{
			throw new NotImplementedException();
		}

		public void RemoveIfExists(string id)
		{
			throw new NotImplementedException();
		}
	}

	//[CLSCompliant(false)]
	//public class HangfireRecurringEventPublisher : IRecurringEventPublisher
	//{
	//	private readonly RecurringJobManager _recurringJob;
	//	private readonly IJsonEventSerializer _serializer;
	//	private readonly JobStorage _jobStorage;

	//	public HangfireRecurringEventPublisher(
	//		RecurringJobManager recurringJob, 
	//		IJsonEventSerializer serializer,
	//		JobStorage jobStorage)
	//	{
	//		_recurringJob = recurringJob;
	//		_serializer = serializer;
	//		_jobStorage = jobStorage;
	//	}

	//	public void PublishHourly(string id, string tenant, IEvent @event)
	//	{
	//		var eventType = @event.GetType();
	//		var serialized = _serializer.SerializeEvent(@event);
	//		var eventTypeName = eventType.FullName + ", " + eventType.Assembly.GetName().Name;

	//		//Expression<Action<HangfireEventServer>> f = x => x.Process("displayName", tenant, eventTypeName, serialized, handlerType);

	//		//_recurringJob.AddOrUpdate(id, Job.FromExpression(f), Cron.Hourly());
	//	}

	//	public void StopPublishing(string id)
	//	{
	//		_recurringJob.RemoveIfExists(id);
	//	}

	//	public IEnumerable<string> RecurringPublishingIds()
	//	{
	//		return _jobStorage.GetConnection().GetRecurringJobs().Select(j => j.Id).ToArray();
	//	}

	//}

}