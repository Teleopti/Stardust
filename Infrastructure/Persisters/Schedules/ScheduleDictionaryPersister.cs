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
		private readonly ICurrentPersistCallbacks _persistCallbacks;

		public ScheduleDictionaryPersister(
			IScheduleRangePersister scheduleRangePersister, 
			IMessageSendersScope messageSendersScope,
			ICurrentPersistCallbacks persistCallbacks)
		{
			_scheduleRangePersister = scheduleRangePersister;
			_messageSendersScope = messageSendersScope;
			_persistCallbacks = persistCallbacks;
		}

		public IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			var completeResult = new SchedulePersistResult();
            foreach (var scheduleRange in scheduleDictionary.Values)
			{
				using (_messageSendersScope.OnThisThreadExclude<ScheduleChangedMessageSender>())
				{
					var result = _scheduleRangePersister.Persist(scheduleRange);
					completeResult.PersistConflicts.ToList().AddRange(result.PersistConflicts);
					completeResult.ModifiedRoots.ToList().AddRange(result.ModifiedRoots);
					completeResult.InitiatorIdentifier = result.InitiatorIdentifier;
				}				
			}
			var aggregatedScheduleChangeMessageSender = _persistCallbacks.Current().OfType<ScheduleChangedMessageSender>().SingleOrDefault();
			if (aggregatedScheduleChangeMessageSender != null)
				aggregatedScheduleChangeMessageSender.Send(completeResult.InitiatorIdentifier, completeResult.ModifiedRoots);
            
			return completeResult.PersistConflicts;
		}
	}
}