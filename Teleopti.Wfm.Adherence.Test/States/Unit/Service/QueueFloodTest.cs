using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class QueueFloodTest
	{
		public Rta Target;
		public FakeStateQueue Queue;
		public IStateQueueHealthChecker Checker;

		[Test]
		public void ShouldStopEnqueueAfter100Batches()
		{
			100.Times(() =>
			{
				Checker.Reset();
				Target.Enqueue(new BatchForTest());
			});
			
			Assert.Throws<StateQueueHealthException>(() =>
			{
				Checker.Reset();
				Target.Enqueue(new BatchForTest());
			});
		}

		[Test]
		public void ShouldNotEnqueueMoreWhenFlooded()
		{
			100.Times(() =>
			{
				Checker.Reset();
				Target.Enqueue(new BatchForTest());
			});
			try
			{
				Checker.Reset();
				Target.Enqueue(new BatchForTest());
			}
			catch (Exception)
			{
			}

			Queue.Count().Should().Be.EqualTo(100);
		}

		[Test]
		[Setting("RtaStateQueueMaxSize", 200)]
		public void ShouldStopEnqueueAfter200Batches()
		{
			200.Times(() =>
			{
				Checker.Reset();
				Target.Enqueue(new BatchForTest());
			});

			Assert.Throws<StateQueueHealthException>(() =>
			{
				Checker.Reset();
				Target.Enqueue(new BatchForTest());
			});
		}
	}
}