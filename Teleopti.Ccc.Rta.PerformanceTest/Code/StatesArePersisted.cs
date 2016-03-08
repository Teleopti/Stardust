using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class StatesArePersisted
	{
		private readonly IAgentStateReadModelPersister _persister;
		private readonly StatesSender _sender;
		private readonly WithAnalyticsUnitOfWork _unitOfWork;

		public StatesArePersisted(
			IAgentStateReadModelPersister persister,
			StatesSender sender,
			WithAnalyticsUnitOfWork unitOfWork)
		{
			_persister = persister;
			_sender = sender;
			_unitOfWork = unitOfWork;
		}

		[LogTime]
		public virtual void WaitForAll()
		{
			var timeWhenLastStateWasSent = _sender.SentSates().Max(x => x.Time);
			while (true)
			{
				var states = Enumerable.Empty<AgentStateReadModel>();
				_unitOfWork.Do(() =>
				{
					states = _persister.GetAll();
				});
				if (states.All(x => x.ReceivedTime == timeWhenLastStateWasSent.Utc()))
					break;
				Thread.Sleep(20);
			}
		}
	}
}