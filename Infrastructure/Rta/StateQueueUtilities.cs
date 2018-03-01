using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class StateQueueUtilities
	{
		private readonly WithAnalyticsUnitOfWork _unitOfWork;

		public StateQueueUtilities(WithAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[TestLog]
		public virtual void WaitForQueue()
		{
			_unitOfWork.Do(uow =>
			{
				while (true)
				{
					var count = uow.Current()
						.Session()
						.CreateSQLQuery(@"SELECT COUNT(1) FROM Rta.StateQueue")
						.UniqueResult<int>();
					if (count == 0)
						break;
					Thread.Sleep(10);
				}
			});
		}
	}
}