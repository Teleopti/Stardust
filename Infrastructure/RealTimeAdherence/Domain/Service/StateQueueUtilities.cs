using System;
using Polly;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.Service
{
	public class StateQueueUtilities
	{
		private readonly ICurrentAnalyticsUnitOfWork _unitOfWork;

		public StateQueueUtilities(ICurrentAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[TestLog]
		[AnalyticsUnitOfWork]
		public virtual void WaitForDequeue(TimeSpan timeout)
		{
			var interval = TimeSpan.FromMilliseconds(100);
			var attempts = (int) (timeout.TotalMilliseconds / interval.TotalMilliseconds);
			var batchCount = Policy.HandleResult<int>(t => t > 0)
				.WaitAndRetry(attempts, attempt => interval)
				.Execute(() => _unitOfWork.Current()
					.Session()
					.CreateSQLQuery(@"SELECT COUNT(1) FROM Rta.StateQueue")
					.UniqueResult<int>()
				);
			if (batchCount > 0)
				throw new WaitForDequeueException($"{batchCount} batches still in state queue after waiting {timeout.TotalSeconds} seconds");
		}

		public class WaitForDequeueException : Exception
		{
			public WaitForDequeueException(string message) : base(message)
			{
			}
		}
	}
}