using System;
using System.Collections.Generic;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
    public interface IJobManager
    {
        void AssignJobToWorkerNodes();
        void CheckWorkerNodesAreAlive(TimeSpan timeSpan);
        void AddItemToJobQueue(JobQueueItem jobQueueItem);
        void CancelJobByJobId(Guid jobId);

        void UpdateResultForJob(Guid jobId,
            string result,
            string workerNodeUri,
            DateTime ended);

        void CreateJobDetail(JobDetail jobDetail, string workerNodeUri);
        Job GetJobByJobId(Guid jobId);
        IList<Job> GetAllJobs();
        IList<JobDetail> GetJobDetailsByJobId(Guid jobId);
    }
}