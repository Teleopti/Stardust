using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class SendAdherenceMessagesTest
	{
		[Test]
		public void ShouldSendMessageForTeam()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var sender = new FakeMessageSender();
			var teamId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", Guid.NewGuid(), null, teamId, null)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);

			sender.LastNotification.DomainId.Should().Be(teamId.ToString());
		}

		[Test]
		public void ShouldUpdateTeamAdherenceAfterLoggingOutAgentMissingFromSnapshot()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode1", Guid.NewGuid(), businessUnitId, teamId, siteId)
				.WithUser("usercode2", personId, businessUnitId, teamId, siteId)
				.WithAlarm("statecode", Guid.Empty, 1)
				.WithAlarm("CCC Logged out", Guid.Empty, 0)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);
			target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
				},
				new ExternalUserStateForSnapshot("2014-10-20 10:00".Utc())
				{
					UserCode = "usercode2",
					StateCode = "statecode",
				}
			});

			target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
				}
			});
			
			var jsonResult = JsonConvert.DeserializeObject<TeamAdherenceMessage>(sender.LastTeamNotification.BinaryData);
			jsonResult.OutOfAdherence.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotSendAggregatedAdherenceMessagesIfAdherenceHasNotChanged()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var sender = new FakeMessageSender();
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode", phone, 1)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);
			target.SaveState(state);

			sender.AllNotifications.Where(x => x.DomainType == typeof(TeamAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
			sender.AllNotifications.Where(x => x.DomainType == typeof(SiteAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldSendAgentAdhereneMessageIfStateGroupHasChanged()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			};
			var sender = new FakeMessageSender();
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 1)
				.WithAlarm("phone", phone, 1)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);
			target.SaveState(state2);

			sender.AllNotifications.Where(x => x.DomainType == typeof(AgentsAdherenceMessage).Name).Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldUpdateAgentStateBeforeSending()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			};
			var sender = new FakeMessageSender();
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, "my first state")
				.WithAlarm("phone", phone, "my second state")
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);
			target.SaveState(state2);

			var jsonResult = JsonConvert.DeserializeObject<AgentsAdherenceMessage>(sender.LastAgentsNotification.BinaryData);
			jsonResult.AgentStates.Single().State.Should().Be.EqualTo("my second state");
		}

		[Test]
		public void ShouldSetBusinessIdOnTeamMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode"
			};
			var sender = new FakeMessageSender();
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnidId, Guid.NewGuid(), null)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);

			sender.LastTeamNotification.BusinessUnitId.Should().Be.EqualTo(businessUnidId.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnTeamMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode"
			};
			var sender = new FakeMessageSender();
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnidId, Guid.NewGuid(), null)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);

			sender.AllNotifications.Where(x => x.DomainType == typeof(TeamAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldSetBusinessIdOnSiteMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode"
			};
			var sender = new FakeMessageSender();
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnidId, null, Guid.NewGuid())
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);

			sender.LastSiteNotification.BusinessUnitId.Should().Be.EqualTo(businessUnidId.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnSiteMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode"
			};
			var sender = new FakeMessageSender();
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnidId, null, Guid.NewGuid())
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);

			sender.AllNotifications.Where(x => x.DomainType == typeof(SiteAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotSendMessageIfPersonIdIsNotAvailable()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
			};
			var sender = new FakeMessageSender();
			var database = new FakeRtaDatabase()
				.WithSource(state.SourceId)
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(state);

			sender.AllNotifications.Should().Be.Empty();
		}
	}
}