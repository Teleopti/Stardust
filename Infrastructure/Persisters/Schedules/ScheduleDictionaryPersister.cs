using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class ScheduleDictionaryPersister : IScheduleDictionaryPersister
	{
		private readonly IScheduleRangePersister _scheduleRangePersister;
		private readonly ITransactionHooksScope _transactionHooksScope;
		private readonly ICurrentTransactionHooks _transactionHooks;

		public ScheduleDictionaryPersister(
			IScheduleRangePersister scheduleRangePersister, 
			ITransactionHooksScope transactionHooksScope,
			ICurrentTransactionHooks transactionHooks)
		{
			_scheduleRangePersister = scheduleRangePersister;
			_transactionHooksScope = transactionHooksScope;
			_transactionHooks = transactionHooks;
		}

		[TestLog]
		public virtual IEnumerable<PersistConflict> Persist(IScheduleDictionary scheduleDictionary)
		{
			var completeResult = new SchedulePersistResult();
            foreach (var scheduleRange in scheduleDictionary.Values)
			{
				using (_transactionHooksScope.OnThisThreadExclude<ScheduleChangedMessageSender>())
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
			var aggregatedScheduleChangeMessageSender = _transactionHooks.Current().OfType<ScheduleChangedMessageSender>().SingleOrDefault();
			if (aggregatedScheduleChangeMessageSender != null)
				aggregatedScheduleChangeMessageSender.Send(completeResult.InitiatorIdentifier, completeResult.ModifiedRoots);
            
			return completeResult.PersistConflicts;
		}
	}
}