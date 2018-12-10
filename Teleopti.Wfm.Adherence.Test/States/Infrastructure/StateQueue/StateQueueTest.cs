using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.StateQueue
{
	[TestFixture]
	[AnalyticsUnitOfWorkTest]
	public class StateQueueTest
	{
		public IStateQueueReader Reader;
		public IStateQueueWriter Writer;

		[Test]
		public void ShouldEnqueueueueeu()
		{
			Writer.Enqueue(new BatchForTest());

			Reader.Dequeue().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldEnqueueBiiiiiiiiiiiiiiiiiiiiiiiiiiiiiigBatch()
		{
			Assert.DoesNotThrow(() =>
			{
				Writer.Enqueue(
					new BatchForTest
					{
						States = Enumerable.Range(0, 1000)
							.Select(i => new BatchStateInputModel
							{
								StateCode = "state",
								UserCode = $"user{i}"
							})
					});
			});
		}

		[Test]
		public void ShouldDequeue()
		{
			Writer.Enqueue(new BatchForTest());
			Reader.Dequeue();

			Reader.Dequeue().Should().Be.Null();
		}

		[Test]
		public void ShouldDequeue2()
		{
			Writer.Enqueue(new BatchForTest());
			Writer.Enqueue(new BatchForTest());

			Reader.Dequeue().Should().Not.Be.Null();
			Reader.Dequeue().Should().Not.Be.Null();
			Reader.Dequeue().Should().Be.Null();
		}

		[Test]
		public void ShouldDequeueWithProperties()
		{
			Writer.Enqueue(new BatchInputModel
			{
				AuthenticationKey = LegacyAuthenticationKey.TheKey,
				SourceId = "sourceId",
				CloseSnapshot = true,
				SnapshotId = "2017-05-16 08:00".Utc(),
				States = new[]
				{
					new BatchStateInputModel
					{
						UserCode = "8808",
						StateCode = "AUX2",
						StateDescription = "phone"
					}
				}
			});

			var result = Reader.Dequeue();

			result.AuthenticationKey.Should().Be(LegacyAuthenticationKey.TheKey);
			result.SourceId.Should().Be("sourceId");
			result.CloseSnapshot.Should().Be(true);
			result.SnapshotId.Should().Be("2017-05-16 08:00".Utc());
			result.States.Single().UserCode.Should().Be("8808");
			result.States.Single().StateCode.Should().Be("AUX2");
			result.States.Single().StateDescription.Should().Be("phone");
		}

		[Test]
		public void ShouldCount()
		{
			Writer.Enqueue(new BatchForTest());
			Writer.Enqueue(new BatchForTest());

			Reader.Count().Should().Be(2);
		}
	}
}