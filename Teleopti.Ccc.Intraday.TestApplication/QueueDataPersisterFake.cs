using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class QueueDataPersisterFake : IQueueDataPersister
	{
		public void Persist(IDictionary<int, IList<QueueInterval>> queueData, bool doReplace)
		{
			foreach (var data in queueData)
			{
				var queueTextCalls = new StringBuilder();
				var queueTextHandleTime = new StringBuilder();
				foreach (var queueStats in data.Value)
				{
					queueTextCalls.Append(queueStats.OfferedCalls + ", ");
					queueTextHandleTime.Append(queueStats.HandleTime + ", ");
				}
				Console.WriteLine("Queue: " + data.Key);
				Console.WriteLine(queueTextCalls);
				Console.WriteLine(queueTextHandleTime);
			}
		}
	}
}