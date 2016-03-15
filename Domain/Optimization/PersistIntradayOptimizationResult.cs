using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PersistIntradayOptimizationResult : ISynchronizeIntradayOptimizationResult
	{
		private readonly IScheduleDictionaryPersister _scheduleDictionaryPersister;

		public PersistIntradayOptimizationResult(IScheduleDictionaryPersister scheduleDictionaryPersister)
		{
			_scheduleDictionaryPersister = scheduleDictionaryPersister;
		}

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			_scheduleDictionaryPersister.Persist(modifiedScheduleDictionary);
		}
	}
}