using System;
using System.Collections.Concurrent;
using System.Linq;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Hangfire
{
	[DomainTest]
	public class HangfireRetryEnqueuingTest : IIsolateSystem
	{
		public HangfireEventPublisher Target;
		public backgroundJobClientThatThrows BackgroundJobClient;

		[TestCase(HangfireEventClient.NumberOfRetries, ExpectedResult = true)]
		[TestCase(HangfireEventClient.NumberOfRetries + 1, ExpectedResult = false)]
		[TestCase(HangfireEventClient.NumberOfRetries -1, ExpectedResult = true)]
		public bool ShouldRetryWhenBackgroundJobClientExceptionTest(int numberOfTimesHangfireThrows)
		{
			BackgroundJobClient.NumberOfTimesToThrow = numberOfTimesHangfireThrows;

			try
			{
				Target.Publish(new ScheduleChangedEvent());			
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		[Test]
		public void NonBackgroundJobClientExceptionShouldBubbleUp()
		{
			BackgroundJobClient.NumberOfTimesToThrow = 1;
			BackgroundJobClient.ExceptionToThrow = new Exception();

			try
			{
				Target.Publish(new ScheduleChangedEvent());
			}
			catch (Exception e)
			{
				e.Should().Be.SameInstanceAs(BackgroundJobClient.ExceptionToThrow);
				return;
			}
			Assert.Fail("No exception was thrown");
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<backgroundJobClientThatThrows>().For<IBackgroundJobClient>();
		}

		public class backgroundJobClientThatThrows : IBackgroundJobClient
		{
			private readonly ConcurrentDictionary<object,ConcurrentQueue<int>> throwQueue = new ConcurrentDictionary<object, ConcurrentQueue<int>>();
			private int[] queue = new int[0];


			public int NumberOfTimesToThrow
			{
				get => queue.Length;
				set => queue = Enumerable.Repeat(1, value).ToArray();
			}

			public Exception ExceptionToThrow { get; set; } = new BackgroundJobClientException("_", null); 
			
			public string Create(Job job, IState state)
			{
				if (throwQueue.GetOrAdd(job.Args[1], job1 => new ConcurrentQueue<int>(queue.ToArray())).TryDequeue(out _))
					throw ExceptionToThrow;

				return string.Empty;
			}

			public bool ChangeState(string jobId, IState state, string expectedState)
			{
				return false;
			}
		}
	}
}