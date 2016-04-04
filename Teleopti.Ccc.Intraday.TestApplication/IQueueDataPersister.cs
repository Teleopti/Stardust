﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	internal interface IQueueDataPersister
	{
		void Persist(IDictionary<int, IList<QueueInterval>> queueData, bool doReplace);
	}
}