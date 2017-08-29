using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	internal interface IQueueDataPersister
	{
		void Persist(
			IDictionary<int, IList<QueueInterval>> queueDataDictionary, 
			DateTime fromDateUtc, int fromIntervalIdUtc, 
			DateTime toDateUtc, int toIntervalIdUtc);
	}
}