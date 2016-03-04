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
		private readonly StatesSender _sender;

		public StatesArePersisted(
			IAgentStateReadModelReader agentStateReader,
			StatesSender sender)
		{
			_agentStateReader = agentStateReader;
			_sender = sender;
		}

		[LogTime]
		public virtual void WaitForAll()
		{
			var timeWhenLastStateWasSent = _sender.SentSates().Max(x => x.Time);
			while (true)
			{
				if (_agentStateReader.GetActualAgentStates().All(x => x.ReceivedTime == timeWhenLastStateWasSent.Utc()))
					break;
				Thread.Sleep(20);
			}
		}
	}
}