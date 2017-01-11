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
		private readonly TestConfiguration _stateHolder;
		public IDataSourceForTenant DataSourceForTenant;

		public StatesArePersisted(
			IAgentStatePersister persister,
			StatesSender sender, 
			TestConfiguration stateHolder)
		{
			_persister = persister;
			_sender = sender;
			_stateHolder = stateHolder;
		}

		[TestLogTime]
		[UnitOfWork]
		public virtual void WaitForAll()
		{
			var timeWhenLastStateWasSent = _sender.StateChanges().Max(x => x.Time);
			var rogersWorking = Enumerable.Range(0, _stateHolder.NumberOfAgentsWorking)
				.Select(x => new ExternalLogon
				{
					DataSourceId = _stateHolder.DataSourceId,
					UserCode = $"roger{x}"
				})
				.ToArray();
			while (true)
			{
				var states = _persister.Find(rogersWorking, DeadLockVictim.Yes);
				if (states.All(x => x.ReceivedTime == timeWhenLastStateWasSent.Utc()))
					break;
				Thread.Sleep(20);
			}
		}
	}
}