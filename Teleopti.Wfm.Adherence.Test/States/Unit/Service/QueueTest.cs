using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class QueueTest
	{
		public Rta Target;
		public FakeDatabase Database;
		public MutableNow Now;
		public FakeStateQueue Queue;
		public FakeEventPublisher Publisher;

		[Test]
		public void ShouldEnqueueBatch()
		{
			Now.Is("2017-05-12 17:00");
			Target.Enqueue(new BatchForTest
			{
				SourceId = "source",
				SnapshotId = "2017-05-12 17:00".Utc(),
				CloseSnapshot = true,
				States = new[]
				{
					new BatchStateInputModel
					{
						UserCode = "usercode",
						StateCode = "phone"
					}
				}
			});

			var actual = Queue.Items().Single();
			actual.Model.SourceId.Should().Be("source");
			actual.Model.SnapshotId.Should().Be("2017-05-12 17:00".Utc());
			actual.Model.CloseSnapshot.Should().Be(true);
			actual.Model.States.Single().UserCode.Should().Be("usercode");
			actual.Model.States.Single().StateCode.Should().Be("phone");
		}
		
		[Test]
		public void ShouldEnqueueBatchOnTenant()
		{
			Database.WithTenant("tenant", "key");
			Target.Enqueue(new BatchForTest { AuthenticationKey = "key" });

			Queue.Items().Single().OnTenant.Should().Be("tenant");
		}

		[Test]
		public void ShouldNotEnqueueBatchWithoutTenant()
		{
			Assert.Throws<InvalidAuthenticationKeyException>(
				() => Target.Enqueue(new BatchForTest
				{
					AuthenticationKey = "key"
				}));

			Queue.Items().Should().Be.Empty();
		}

		[Test]
		public void ShouldProcessQueue()
		{
			var state = Guid.NewGuid();
			Database
				.WithAgent("usercode")
				.WithStateGroup(state, "phone")
				.WithStateCode("phone")
				;
			Target.Enqueue(new BatchForTest
			{
				States = new[]
				{
					new BatchStateInputModel
					{
						UserCode = "usercode",
						StateCode = "phone"
					}
				}
			});

			Target.QueueIteration(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().StateGroupId.Should().Be(state);
		}

		[Test]
		public void ShouldProcessEmptyQueue()
		{
			Publisher.Clear();
			Target.QueueIteration(Database.TenantName());

			Publisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldIndicateIteration()
		{
			Target.Enqueue(new BatchForTest());

			Target.QueueIteration(Database.TenantName()).Should().Be.True();
		}


		[Test]
		public void ShouldIndicateEmptyQueue()
		{
			Target.QueueIteration(Database.TenantName()).Should().Be.False();
		}
	}

}