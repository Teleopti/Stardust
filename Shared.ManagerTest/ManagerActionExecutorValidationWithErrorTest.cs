using System;
using ManagerTest.Attributes;
using ManagerTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Models;

namespace ManagerTest
{
	[ManagerActionExecutorValidationWithCustomJobManagerTestAttribute]
	[TestFixture]
	public class ManagerActionExecutorValidationWithErrorTest
    {
		public ManagerActionExecutor Target;
        public FakeJobManager FakeJobManager;

        public const string Hostname = "AGreatHostname";

        [SetUp]
        public void Setup()
        {
            FakeJobManager?.Reset();
        }
        
        [Test]
        public void ShouldHandleExceptionFromWithinJobFailed()
        {
            FakeJobManager.SetFakeBehaviour(FakeJobManager.BehaviourSelector.ThrowException);
            var jobFailed = new JobFailed
            {
                JobId = Guid.NewGuid(),
                Created = DateTime.Now,
                AggregateException = new AggregateException()
            };

            var actionResult = Target.JobFailed(jobFailed, Hostname);
            actionResult.Exception.InnerException.Message.Should().Be("CreateJobDetail");
        }

        [Test]
        public void ShouldHandleExceptionFromWithinJobSucceeded()
        {
            FakeJobManager.SetFakeBehaviour(FakeJobManager.BehaviourSelector.ThrowException);
            
            var actionResult = Target.JobSucceed(Guid.NewGuid(),Hostname);
            actionResult.Exception.InnerException.Message.Should().Be("UpdateResultForJob");
        }

        [Test]
        public void ShouldHandleTimeoutFromWithinJobFailed()
        {
            FakeJobManager.SetFakeBehaviour(FakeJobManager.BehaviourSelector.Timeout, TimeSpan.FromSeconds(70));

            var jobFailed = new JobFailed
            {
                JobId = Guid.NewGuid(),
                Created = DateTime.Now,
                AggregateException = new AggregateException()
            };

            var actionResult = Target.JobFailed(jobFailed, Hostname);
            actionResult.Exception.InnerException.Message.Should().Be("Timeout while executing JobFailed");
        }

        //[Test]
        //public void ShouldHandleExceptionFromWithinJobSucceeded()
        //{
        //    Target.Request = new HttpRequestMessage
        //    {
        //        RequestUri = new Uri("http://calabiro.com")
        //    };

        //    var httpActionResult = Target.JobSucceed(Guid.NewGuid());
        //    var exceptionResult = httpActionResult as ExceptionResult;
        //    exceptionResult.Exception.InnerException.Message.Should().Be("UpdateResultForJob");
        //}
    }
}
