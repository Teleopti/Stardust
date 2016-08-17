using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	[Toggle(Toggles.RTA_Optimize_39667)]
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
				.WithUser("user1", personId);
			
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

			Persister.Get(personId).StateCode.Should().Be("phone");
		}

		[Test]
		public void ShouldUpdateAgentInSnapshotAndLogOutAgentNotInSnapshot()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Database
				.WithUser("user1", personId1)
				.WithUser("user2", personId2);
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

			Persister.Get(personId1).StateCode.Should().Be(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot);
			Persister.Get(personId2).StateCode.Should().Be("phone");
		}

		[Test]
		[ToggleOff(Toggles.RTA_RuleMappingOptimization_39812)]
		// this works for real with RTA_RuleMappingOptimization_39812, see infra test
		public void ShouldNotAddDuplicateStateCodes()
		{
			Database
				.WithUser("usercode1")
				.WithUser("usercode2")
				.WithUser("usercode3")
				.WithRule();

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-05-18 08:00".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "phone"
					},
					new BatchStateForTest
					{
						UserCode = "usercode2",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-05-18 08:00".Utc()
			});

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-05-18 08:05".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode3",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-05-18 08:05".Utc()
			});


			Database.StateCodes.Where(x => x.StateCode == Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.Should().Have.Count.EqualTo(1);
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