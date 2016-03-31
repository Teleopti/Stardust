using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class QueueInfoProviderFake : IQueueInfoProvider
	{
		public IList<QueueInfo> Provide()
		{
			return new List<QueueInfo>()
			{
				new QueueInfo(){QueueId = 1, HasDataToday = true}, 
				new QueueInfo(){QueueId = 2, HasDataToday = false}, 
				new QueueInfo(){QueueId = 3, HasDataToday = false}, 
			};
		}
	}
}