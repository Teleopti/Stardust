using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Node.Interfaces
{
    public interface IWorkerWrapper
    {
        string WhoamI { get; }

        CancellationTokenSource CancellationTokenSource { get; set; }
        bool IsCancellationRequested { get; }
        bool IsTaskExecuting { get; }
        Task Task { get; }

        JobToDo GetCurrentMessageToProcess();
        void CancelJob(Guid id);

        IHttpActionResult StartJob(JobToDo jobToDo,
            HttpRequestMessage requestMessage);
    }
}