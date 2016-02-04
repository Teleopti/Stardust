using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.TimeLogger;
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

		[LogTime]
		public virtual IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			var completeResult = new SchedulePersistResult();
            foreach (var scheduleRange in scheduleDictionary.Values)
			{
				using (_messageSendersScope.OnThisThreadExclude<ScheduleChangedMessageSender>())
				{
					var result = _scheduleRangePersister.Persist(scheduleRange);
			
					var persistConflicts = completeResult.PersistConflicts.ToList();
					persistConflicts.AddRange(result.PersistConflicts);
					completeResult.PersistConflicts = persistConflicts;

					var modifiedRoots = completeResult.ModifiedRoots.ToList();
					modifiedRoots.AddRange(result.ModifiedRoots);
					completeResult.ModifiedRoots = modifiedRoots;
					
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