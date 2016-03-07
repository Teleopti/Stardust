using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class StatesArePersisted
	{
		private readonly IAgentStateReadModelPersister _persister;
		private readonly StatesSender _sender;

		public StatesArePersisted(
			IAgentStateReadModelPersister persister,
			StatesSender sender)
		{
			_persister = persister;
			_sender = sender;
		}

		[LogTime]
		public virtual void WaitForAll()
		{
			var timeWhenLastStateWasSent = _sender.SentSates().Max(x => x.Time);
			while (true)
			{
				if (_persister.GetActualAgentStates().All(x => x.ReceivedTime == timeWhenLastStateWasSent.Utc()))
					break;
				Thread.Sleep(20);
			}
		}
	}
}