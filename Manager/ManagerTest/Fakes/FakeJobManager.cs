using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Stardust.Manager;
using Stardust.Manager.Models;

namespace ManagerTest.Fakes
{
    public class FakeJobManager : IJobManager
    {
        public FakeJobManager()
        {
            Reset();
        }
        
        public enum BehaviourSelector
        {
            Nothing,
            ThrowException,
            Timeout
        };

        public BehaviourSelector Behaviour;
        public TimeSpan Timeout;

        public void Reset()
        {
            Behaviour = BehaviourSelector.Nothing;
            Timeout = TimeSpan.Zero;
        }
        
        public void SetFakeBehaviour(BehaviourSelector behaviour, TimeSpan? timeout = null)
        {
            Behaviour = behaviour;
            if(timeout.HasValue)
                Timeout = timeout.Value;
        }

        private void SelectFunction()
        {
            switch (Behaviour)
            {
                case BehaviourSelector.Nothing:
                    break;

                case BehaviourSelector.ThrowException:
                    var st = new StackTrace();
                    var sf = st.GetFrame(1);
                    throw new NotImplementedException(sf.GetMethod().Name);

                case BehaviourSelector.Timeout:
                    Thread.Sleep(Timeout);
                    break;
            }
        }

        public void AssignJobToWorkerNodes()
        {
            SelectFunction();
        }

        public void CheckWorkerNodesAreAlive(TimeSpan timeSpan)
        {
            SelectFunction();
        }

        public void AddItemToJobQueue(JobQueueItem jobQueueItem)
        {
            SelectFunction();
        }

        public void CancelJobByJobId(Guid jobId)
        {
            SelectFunction();
        }

        public void UpdateResultForJob(Guid jobId, string result, DateTime ended)
        {
            SelectFunction();
        }

        public void CreateJobDetail(JobDetail jobDetail)
        {
            SelectFunction();
        }

        public Job GetJobByJobId(Guid jobId)
        {
            throw new NotImplementedException();
        }

        public IList<Job> GetAllJobs()
        {
            SelectFunction();
            return new List<Job>();
        }

        public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
        {
            SelectFunction();
            return new List<JobDetail>();
        }
    }
}
