using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDictionaryPersister : IScheduleDictionaryPersister
	{
		private readonly IScheduleRangePersister _scheduleRangePersister;
		private readonly IMessageSendersScope _messageSendersScope;
		private readonly ICurrentMessageSenders _messageSenders;

		public ScheduleDictionaryPersister(
			IScheduleRangePersister scheduleRangePersister, 
			IMessageSendersScope messageSendersScope,
			ICurrentMessageSenders messageSenders)
		{
			_scheduleRangePersister = scheduleRangePersister;
			_messageSendersScope = messageSendersScope;
			_messageSenders = messageSenders;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			var completeResult = new SchedulePersistResult();
            foreach (var scheduleRange in scheduleDictionary.Values)
			{
				using (_messageSendersScope.OnThisThreadExclude<AggregatedScheduleChangeMessageSender>())
				{
					var result = _scheduleRangePersister.Persist(scheduleRange);
					completeResult.PersistConflicts = completeResult.PersistConflicts.Concat(result.PersistConflicts);
					completeResult.ModifiedRoots = completeResult.ModifiedRoots.Concat(result.ModifiedRoots);
					completeResult.InitiatorIdentifier = result.InitiatorIdentifier;
				}				
			}
			var aggregatedScheduleChangeMessageSender = _messageSenders.Current().OfType<AggregatedScheduleChangeMessageSender>().SingleOrDefault();
			if (aggregatedScheduleChangeMessageSender != null)
				aggregatedScheduleChangeMessageSender.Execute(completeResult.InitiatorIdentifier, completeResult.ModifiedRoots);
            
			return completeResult.PersistConflicts;
		}
	}
}