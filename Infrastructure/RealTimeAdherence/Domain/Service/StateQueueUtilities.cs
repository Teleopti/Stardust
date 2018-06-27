using System;
using Polly;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.Service
{
	public class StateQueueUtilities
	{
		private readonly WithAnalyticsUnitOfWork _unitOfWork;

		public StateQueueUtilities(WithAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[TestLog]
		public virtual void WaitForDequeue()
		{
			_unitOfWork.Do(uow =>
			{
				while (true)
				{
					Policy.Handle<WaitForDequeueException>()
						.WaitAndRetry(5000, attempt => TimeSpan.FromSeconds(1))
						.Execute(() =>
						{
							var count = uow.Current()
								.Session()
								.CreateSQLQuery(@"SELECT COUNT(1) FROM Rta.StateQueue")
								.UniqueResult<int>();
							if (count > 0)
								throw new WaitForDequeueException($"{count} batches still in state queue after waiting 5000 seconds");
						});
				}
			});
		}

		public class WaitForDequeueException : Exception
		{
			public WaitForDequeueException(string message) : base(message)
			{
			}
		}
	}
}