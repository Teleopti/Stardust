using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class StatesArePersisted
	{
		private readonly IAgentStatePersister _persister;
		private readonly StatesSender _sender;
		public IDataSourceForTenant DataSourceForTenant;

		public StatesArePersisted(
			IAgentStatePersister persister,
			StatesSender sender)
		{
			_persister = persister;
			_sender = sender;
		}

		[TestLogTime]
		[UnitOfWork]
		public virtual void WaitForAll()
		{
			var timeWhenLastStateWasSent = _sender.StateChanges().Max(x => x.Time);
			while (true)
			{
				var states = _persister.Find(_persister.FindAll(), DeadLockVictim.Yes);
				if (states.All(x => x.ReceivedTime == timeWhenLastStateWasSent.Utc()))
					break;
				Thread.Sleep(20);
			}
		}
	}
}