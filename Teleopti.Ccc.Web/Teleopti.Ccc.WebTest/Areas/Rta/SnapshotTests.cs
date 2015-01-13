using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class SnapshotTests
	{
		[Test]
		public void ShouldLogOutPersonsNotInSnapshot()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithAlarm("statecode", Guid.Empty)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);
			target.SaveStateSnapshot(new[]
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
			sender.AllNotifications.Clear();

			target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode"
				},
			});

			sender.NotificationsOfType<AgentStateReadModel>()
				.Select(x => x.DeseralizeActualAgentState())
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be("CCC Logged out");
		}


		[Test]
		public void ShouldOnlyLogOutPersonsInSnapshotConnectedToPlatform()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithSource("source1")
				.WithSource("source2")
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithAlarm("statecode1", Guid.Empty)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);
			target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode1"
				}
			});
			target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode2",
					StateCode = "statecode1",
					SourceId = "source2",
				}
			});

			target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode1"
				}
			});

			sender.NotificationsOfType<AgentStateReadModel>()
				.Select(x => x.DeseralizeActualAgentState())
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be("statecode1");
		}

		[Test]
		public void ShouldNotLogOutAlreadyLoggedOutAgents()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithAlarm("statecode", Guid.Empty)
				.WithAlarm("statecode2", Guid.Empty, true)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);
			target.SaveStateSnapshot(new[]
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

			target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode"
				},
			});

			sender.NotificationsOfType<AgentStateReadModel>()
				.Select(x => x.DeseralizeActualAgentState())
				.Single(x => x.PersonId == personId)
				.StateCode.Should().Be("statecode2");
		}

		[Test]
		public void ShouldUseEmptyPlatformTypeIdWhenLoggingOutAgent()
		{
			var personId = Guid.NewGuid();
			var platformTypeId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest {PlatformTypeId = platformTypeId.ToString()})
				.WithUser("usercode1", Guid.NewGuid())
				.WithUser("usercode2", personId)
				.WithAlarm("statecode", Guid.Empty)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);
			target.SaveStateSnapshot(new[]
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
			sender.AllNotifications.Clear();

			target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
					PlatformTypeId = platformTypeId.ToString()
				},
			});

			sender.NotificationsOfType<AgentStateReadModel>()
				.Select(x => x.DeseralizeActualAgentState())
				.Single(x => x.PersonId == personId)
				.PlatformTypeId.Should().Be.EqualTo(Guid.Empty);
		}
	}
}