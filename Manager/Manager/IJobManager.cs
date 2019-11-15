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
            DateTime ended);

        void CreateJobDetail(JobDetail jobDetail);
        Job GetJobByJobId(Guid jobId);
        IList<Job> GetAllJobs();
        IList<JobDetail> GetJobDetailsByJobId(Guid jobId);
    }
}