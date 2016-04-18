using System;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Node.Entities;
using Stardust.Node.Workers;

namespace Stardust.Node.Interfaces
{
	public interface IWorkerWrapper : IDisposable
	{
		string WhoamI { get; }

		CancellationTokenSource CancellationTokenSource { get; set; }

		bool IsCancellationRequested { get; }

		bool IsTaskExecuting { get; }
		Task Task { get; }

		JobQueueItemEntity GetCurrentMessageToProcess();

		void CancelJob(Guid id);

		void StartJob(JobQueueItemEntity jobQueueItemEntity);

		ObjectValidationResult ValidateStartJob(JobQueueItemEntity jobQueueItemEntity);

		void CancelTimeoutCurrentMessageTask();

		Task CreateTimeoutCurrentMessageTask(JobQueueItemEntity jobQueueItemEntity);
	}
}