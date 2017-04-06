using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class ExecuteBatchesInParallelTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public FakeAgentStateReadModelPersister Persister;
		
		[Test]
		public void ShouldPersistStateWhenMultiplePersonsAreUnknown()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("user1", personId)
				.WithStateCode("phone");

			Assert.Throws<AggregateException>(() => Target.SaveStateBatch(new BatchForTest
			{
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "unknown1",
						StateCode = "phone"
					},
					new BatchStateForTest
					{
						UserCode = "user1",
						StateCode = "phone"
					},
					new BatchStateForTest
					{
						UserCode = "unknown2",
						StateCode = "phone"
					}
				}
			}));

			Persister.Load(personId).StateName.Should().Be("phone");
		}

		[Test]
		public void ShouldUpdateAgentInSnapshotAndLogOutAgentNotInSnapshot()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Database
				.WithAgent("user1", personId1)
				.WithAgent("user2", personId2)
				.WithStateGroup(null, Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot, false, true)
				.WithStateCode(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.WithMappedRule(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.WithMappedRule("ready")
				.WithMappedRule("phone")
				;
			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-07-11 08:00".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "user1",
						StateCode = "ready"
					},
					new BatchStateForTest
					{
						UserCode = "user2",
						StateCode = "ready"
					}
				}
			});

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-07-11 08:10".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "user2",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-07-11 08:10".Utc()
			});

			Persister.Load(personId1).StateName.Should().Be(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot);
			Persister.Load(personId2).StateName.Should().Be("phone");
		}

		[Test]
		public void ShouldThrowForAllMissingPersons()
		{
			var users = (
				from n in Enumerable.Range(0, 50)
				select $"unknown{n}"
				).ToArray();

			var states = (
				from u in users
				select new BatchStateForTest
				{
					UserCode = u,
					StateCode = "phone"
				}
				).ToArray();

			100.Times(() =>
			{
				var result = Assert.Throws<AggregateException>(() => Target.SaveStateBatch(new BatchForTest
				{
					States = states
				}));
				result.InnerExceptions.Count.Should().Be(50);
				result.InnerExceptions.OfType<InvalidUserCodeException>().Count().Should().Be(50);
			});
		}

	}
}