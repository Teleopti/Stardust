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

		JobToDo GetCurrentMessageToProcess();
		void CancelJob(Guid id);

		void StartJob(JobToDo jobToDo);

		ObjectValidationResult ValidateStartJob(JobToDo jobToDo);

	}
}