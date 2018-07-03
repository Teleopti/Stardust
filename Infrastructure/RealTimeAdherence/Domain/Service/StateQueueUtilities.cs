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
		public virtual void WaitForDequeue(TimeSpan timeout)
		{
			_unitOfWork.Do(uow =>
			{
				var interval = TimeSpan.FromMilliseconds(100);
				var attempts = timeout.Milliseconds / interval.Milliseconds;
				var queueIsEmpty = Policy.HandleResult(false)
					.WaitAndRetry(attempts, attempt => interval)
					.Execute(() => uow.Current()
									   .Session()
									   .CreateSQLQuery(@"SELECT COUNT(1) FROM Rta.StateQueue")
									   .UniqueResult<int>() == 0
					);
				if (!queueIsEmpty)
					throw new WaitForDequeueException($"Batches still in state queue after waiting 5 seconds");
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