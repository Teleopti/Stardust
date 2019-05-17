using System;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Node.Entities;
using ValidateStartJobResult = Stardust.Core.Node.Workers.ValidateStartJobResult;

namespace Stardust.Core.Node.Interfaces
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

		ValidateStartJobResult ValidateStartJob(JobQueueItemEntity jobQueueItemEntity);

		void CancelTimeoutCurrentMessageTask();

		void Init(NodeConfiguration nodeConfiguration);
	}
}