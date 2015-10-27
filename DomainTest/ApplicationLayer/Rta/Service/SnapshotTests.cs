using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class SnapshotTests
	{
		public FakeRtaDatabase Database;
		public FakeMessageSender Sender;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldLogOutPersonsNotInSnapshot()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithAlarm("statecode", Guid.Empty)
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
			Sender.AllNotifications.Clear();

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode"
				},
			});

			Sender.NotificationsOfType<AgentStateReadModel>()
				.Select(x => x.DeseralizeActualAgentState())
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be("CCC Logged out");
		}


		[Test]
		public void ShouldOnlyLogOutPersonsInSnapshotConnectedToPlatform()
		{
			var personId = Guid.NewGuid();
			Database
				.WithSource("source1")
				.WithSource("source2")
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithAlarm("statecode1", Guid.Empty)
				;
			Now.Is("2014-10-20 10:00");
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode1"
				}
			});
			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode2",
					StateCode = "statecode1",
					SourceId = "source2",
				}
			});

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode1"
				}
			});

			Sender.NotificationsOfType<AgentStateReadModel>()
				.Select(x => x.DeseralizeActualAgentState())
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be("statecode1");
		}

		[Test]
		public void ShouldNotLogOutAlreadyLoggedOutAgents()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithAlarm("statecode", Guid.Empty)
				.WithAlarm("statecode2", Guid.Empty, true);
			
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
					StateCode = "statecode2"
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

			Sender.NotificationsOfType<AgentStateReadModel>()
				.Select(x => x.DeseralizeActualAgentState())
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be("statecode2");
		}

		[Test]
		public void ShouldUseEmptyPlatformTypeIdWhenLoggingOutAgent()
		{
			var personId = Guid.NewGuid();
			var platformTypeId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest {PlatformTypeId = platformTypeId.ToString()})
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithAlarm("statecode", Guid.Empty)
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
			Sender.AllNotifications.Clear();

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
					PlatformTypeId = platformTypeId.ToString()
				},
			});

			Sender.NotificationsOfType<AgentStateReadModel>()
				.Select(x => x.DeseralizeActualAgentState())
				.Single(x => x.PersonId == personId)
				.PlatformTypeId.Should().Be.EqualTo(Guid.Empty);
		}
	}
}