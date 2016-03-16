using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class StatesArePersisted
	{
		private readonly IAgentStateReadModelPersister _persister;
		private readonly StatesSender _sender;
		private readonly WithAnalyticsUnitOfWork _unitOfWork;
		private readonly IDataSourceScope _dataSource;

		public StatesArePersisted(
			IAgentStateReadModelPersister persister,
			StatesSender sender,
			WithAnalyticsUnitOfWork unitOfWork,
			IDataSourceScope dataSource)
		{
			_persister = persister;
			_sender = sender;
			_unitOfWork = unitOfWork;
			_dataSource = dataSource;
		}

		[LogTime]
		public virtual void WaitForAll()
		{
			var timeWhenLastStateWasSent = _sender.SentSates().Max(x => x.Time);
			using (_dataSource.OnThisThreadUse(DataSourceHelper.TestTenantName))
			{
				_unitOfWork.Do(() =>
				{
					while (true)
					{
						var states = _persister.GetAll();
						if (states.All(x => x.ReceivedTime == timeWhenLastStateWasSent.Utc()))
							break;
						Thread.Sleep(20);
					}
				});
			}
		}
	}
}