using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class StatesArePersisted
	{
		private readonly IAgentStateReadModelReader _agentStateReader;

		public StatesArePersisted(IAgentStateReadModelReader agentStateReader)
		{
			_agentStateReader = agentStateReader;
		}

		[LogTime]
		public virtual void WaitForAll()
		{
			while (true)
			{
				if (_agentStateReader.GetActualAgentStates().All(x => x.ReceivedTime == "2016-02-26 17:11".Utc()))
					break;
				Thread.Sleep(20);
			}
		}
	}
}