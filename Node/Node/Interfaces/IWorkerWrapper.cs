using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Node.Entities;

namespace Stardust.Node.Interfaces
{
	public interface IWorkerWrapper : IDisposable
	{
		string WhoamI { get; }

		CancellationTokenSource CancellationTokenSource { get; set; }

		bool IsCancellationRequested { get; }
		bool IsWorking { get; }

		Task Task { get; }

		JobQueueItemEntity GetCurrentMessageToProcess();

		void CancelJob(Guid id);

		void StartJob(JobQueueItemEntity jobQueueItemEntity);

		HttpResponseMessage ValidateStartJob(JobQueueItemEntity jobQueueItemEntity);

		void CancelTimeoutCurrentMessageTask();

		void Init(NodeConfiguration nodeConfiguration);
	}
}