using System;
using System.Net.Http;
using System.Web.Http.Results;
using ManagerTest.Attributes;
using ManagerTest.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Manager;
using Stardust.Manager.Models;

namespace ManagerTest
{
	[ManagerControllerValidationWithCustomJobManagerTest]
	[TestFixture]
	public class ManagerControllerValidationWithErrorTest
    {
		public ManagerController Target;
        public FakeJobManager FakeJobManager;

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

            Target.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://calabiro.com")
            };

            var httpActionResult = Target.JobFailed(jobFailed);
            var exceptionResult = httpActionResult as ExceptionResult;
            exceptionResult.Exception.InnerException.Message.Should().Contain("JobFailed");
        }

        [Test]
        public void ShouldHandleExceptionFromWithinJobSucceeded()
        {
            FakeJobManager.SetFakeBehaviour(FakeJobManager.BehaviourSelector.ThrowException);
            Target.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://calabiro.com")
            };

            var httpActionResult = Target.JobSucceed(Guid.NewGuid());
            var exceptionResult = httpActionResult as ExceptionResult;
            exceptionResult.Exception.InnerException.Message.Should().Be("UpdateResultForJob");
        }

        [Test]
        public void ShouldHandleTimeoutFromWithinJobFailed()
        {
            FakeJobManager.SetFakeBehaviour(FakeJobManager.BehaviourSelector.Timeout, TimeSpan.FromSeconds(70));
            Target.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://calabiro.com")
            };
            
            var jobFailed = new JobFailed
            {
                JobId = Guid.NewGuid(),
                Created = DateTime.Now,
                AggregateException = new AggregateException()
            };

            var httpActionResult = Target.JobFailed(jobFailed);
            var exceptionResult = httpActionResult as ExceptionResult;
            exceptionResult.Exception.InnerException.Message.Should().Be("Timeout while executing JobFailed");
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
