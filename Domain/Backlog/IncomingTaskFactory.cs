using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Backlog
{
	public class IncomingTaskFactory
	{
		private readonly FlatDistributionSetter _distributionSetter;

		public IncomingTaskFactory(FlatDistributionSetter distributionSetter)
		{
			_distributionSetter = distributionSetter;
		}

		public IncomingTask Create(DateOnlyPeriod spanningPeriod, int totalWorkItems, TimeSpan averageWorkTimePerItem)
		{
			return new IncomingTask(spanningPeriod, totalWorkItems, averageWorkTimePerItem, _distributionSetter);
		}
	}
}