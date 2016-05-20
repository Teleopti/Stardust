using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class SnapshotTests
	{
		public FakeAgentStateStorage Persister;
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldLogOutPersonsNotInSnapshot()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithRule("statecode", Guid.Empty)
				;

			Now.Is("2014-10-20 10:00");
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode"
				},
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode2",
					StateCode = "statecode"
				}
			});

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode"
				},
			});

			Persister.Models			
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot);
		}

		[Test]
		public void ShouldOnlyLogOutPersonsNotInSnapshotFromSameSource()
		{
			var personId = Guid.NewGuid();
			Database
				.WithSource("source1")
				.WithUser("usercode1", Guid.NewGuid())
				.WithSource("source2")
				.WithUser("usercode2", personId)
				.WithRule("statecode", Guid.Empty)
				;
			Now.Is("2014-10-20 10:00");
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
					SourceId = "source1",
				}
			});
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode2",
					StateCode = "statecode",
					SourceId = "source2",
				}
			});

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
					SourceId = "source1",
				}
			});
			
			Persister.Models
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be("statecode");
		}

		[Test]
		public void ShouldLogOutPersonNotInSnapshotWhenPreviousWasRecievedAsSingleStates()
		{
			var personId = Guid.NewGuid();
			Database
				.WithSource("source1")
				.WithUser("usercode1", "source1", Guid.NewGuid())
				.WithUser("usercode2", "source1", personId)
				.WithRule("statecode1", Guid.Empty)
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode1",
				SourceId = "source1",
				StateCode = "statecode1"
			});

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode2",
				SourceId = "source1",
				StateCode = "statecode1",
			});

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					SourceId = "source1",
					StateCode = "statecode1"
				}
			});

			Persister.Models
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot);
		}

		[Test]
		public void ShouldOnlyLogOutPersonsInSnapshotFromSameSourceWhenPreviousWasRecievedAsSingleStates()
		{
			var personId = Guid.NewGuid();
			Database
				.WithSource("source1")
				.WithUser("usercode1", "source1", Guid.NewGuid())
				.WithSource("source2")
				.WithUser("usercode2", "source2", personId)
				.WithRule("statecode1", Guid.Empty)
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode1",
				SourceId = "source1",
				StateCode = "statecode1"
			});

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode2",
				SourceId = "source2",
				StateCode = "statecode1",
			});

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					SourceId = "source1",
					StateCode = "statecode1"
				}
			});

			Persister.Models
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be("statecode1");
		}

		[Test]
		public void ShouldNotLogOutAlreadyLoggedOutAgents()
		{
			var user2 = Guid.NewGuid();
			Database
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", user2)
				.WithRule("statecode", Guid.Empty)
				.WithRule(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot, Guid.Empty);

			Now.Is("2014-10-20 10:00");
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode"
				},
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode2",
					StateCode = Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot
				}
			});

			Now.Is("2014-10-20 10:05");
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode"
				},
			});

			Persister.Models
				.Single(x => x.PersonId == user2)
				.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldUseEmptyPlatformTypeIdWhenLoggingOutAgent()
		{
			var personId = Guid.NewGuid();
			var platformTypeId = Guid.NewGuid();
			Database
				.WithPlatform(platformTypeId)
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithRule("statecode", Guid.Empty)
				;
			Now.Is("2014-10-20 10:00");
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
					PlatformTypeId = platformTypeId.ToString()
				},
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode2",
					StateCode = "statecode",
					PlatformTypeId = platformTypeId.ToString()
				}
			});

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
					PlatformTypeId = platformTypeId.ToString()
				},
			});

			Persister.Models
				.Single(x => x.PersonId == personId)
				.PlatformTypeId.Should().Be.EqualTo(Guid.Empty);
		}
	}
}