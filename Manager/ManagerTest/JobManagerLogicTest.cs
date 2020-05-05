using System.Linq;
using ManagerTest.Attributes;
using ManagerTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest
{
    [ManagerControllerValidationTest]
    public class JobManagerLogicTest
    {
        public IJobManager JobManager;
        public FakeJobRepository JobRepository;
        public IWorkerNodeRepository WorkerNodeRepository;
        
        [Test]
        public void ShouldNotCallAssignJobWhenJobQueueIsEmpty()
        {
            JobManager.AssignJobToWorkerNodes();
            JobRepository.HasCalledAssignJobToNode.Should().Be.Empty();
        }
        
        [Test]
        public void ShouldCallAssignJobOnlyIfJobQueueItemIsAvailable()
        {
            foreach(var i in Enumerable.Range(1,2))
            {
                WorkerNodeRepository.AddWorkerNode(new WorkerNode());
            }
            var jobQueueItem = new JobQueueItem
            {
                Name = "Name",
                Serialized = "Serialized",
                Type = "Type",
                CreatedBy = string.Empty
            };
            JobRepository.AddItemToJobQueue(jobQueueItem);
            JobManager.AssignJobToWorkerNodes();
            
            JobRepository.HasCalledAssignJobToNode.Count.Should().Be(1);
        }
    }
}