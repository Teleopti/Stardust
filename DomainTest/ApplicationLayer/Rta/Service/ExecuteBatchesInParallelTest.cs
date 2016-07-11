using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using System.Linq;

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

			Assert.Throws<AggregateException>(() => Target.SaveStateBatch(new[]
			{
				new ExternalUserStateForTest
				{
					UserCode = "unknown1",
					StateCode = "phone"
				},
				new ExternalUserStateForTest
				{
					UserCode = "user1",
					StateCode = "phone"
				},
				new ExternalUserStateForTest
				{
					UserCode = "unknown2",
					StateCode = "phone"
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
			Target.SaveStateBatch(new[]
			{
				new ExternalUserStateForSnapshot("2016-07-11 08:00".Utc())
				{
					UserCode = "user1",
					StateCode = "ready"
				},
				new ExternalUserStateForSnapshot("2016-07-11 08:00".Utc())
				{
					UserCode = "user2",
					StateCode = "ready"
				}
			});


			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2016-07-11 08:10".Utc())
				{
					UserCode = "user2",
					StateCode = "phone"
				}
			});

			Persister.Get(personId1).StateCode.Should().Be(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot);
			Persister.Get(personId2).StateCode.Should().Be("phone");
		}


		[Test]
		public void ShouldNotAddDuplicateStateCodes()
		{
			Database
				.WithUser("usercode1")
				.WithUser("usercode2")
				.WithUser("usercode3")
				.WithRule();

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForTest
				{
					UserCode = "usercode1",
					StateCode = "phone",
					BatchId = "2016-05-18 08:00".Utc()
				},
				new ExternalUserStateForTest
				{
					UserCode = "usercode2",
					StateCode = "phone",
					BatchId = "2016-05-18 08:00".Utc()
				}
			});

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForTest
				{
					UserCode = "usercode3",
					StateCode = "phone",
					BatchId = "2016-05-18 08:05".Utc()
				}
			});

			Database.StateCodes.Where(x => x.StateCode == Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
				.Should().Have.Count.EqualTo(1);
		}
	}
}