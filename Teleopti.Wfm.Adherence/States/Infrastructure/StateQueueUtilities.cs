using System;
using Polly;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
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
				.Execute(BatchesInStateQueue);
			if (batchCount > 0)
				throw new WaitForDequeueException($"{batchCount} batches still in state queue after waiting {timeout.TotalSeconds} seconds");
		}

		[TestLog]
		protected virtual int BatchesInStateQueue() => 
			_unitOfWork.Current()
				.Session()
				.CreateSQLQuery(@"SELECT COUNT(1) FROM Rta.StateQueue")
				.UniqueResult<int>();

		public class WaitForDequeueException : Exception
		{
			public WaitForDequeueException(string message) : base(message)
			{
			}
		}
	}
}