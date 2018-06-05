using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestCommon
{
	public interface IQueueDataPersister
	{
		void Persist(
			IDictionary<int, IList<QueueInterval>> queueDataDictionary, 
			DateTime fromDateUtc, int fromIntervalIdUtc, 
			DateTime toDateUtc, int toIntervalIdUtc);
	}
}