using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Stardust.Node.Entities;
using Stardust.Node.Workers;

namespace Stardust.Node.Interfaces
{
	public interface IWorkerWrapper : IDisposable
	{
		string WhoamI { get; }

		CancellationTokenSource CancellationTokenSource { get; set; }

		bool IsCancellationRequested { get; }

		Task Task { get; }

		JobQueueItemEntity GetCurrentMessageToProcess();

		void CancelJob(Guid id);

		void StartJob(JobQueueItemEntity jobQueueItemEntity);

		HttpResponseMessage ValidateStartJob(JobQueueItemEntity jobQueueItemEntity);

		void CancelTimeoutCurrentMessageTask();

		Task CreateTimeoutCurrentMessageTask(JobQueueItemEntity jobQueueItemEntity);
	}
}