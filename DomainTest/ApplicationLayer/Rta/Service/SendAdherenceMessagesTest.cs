using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class SendAdherenceMessagesTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public FakeMessageSender Sender;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldSendMessageForTeam()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var teamId = Guid.NewGuid();
			Database
				.WithUser("usercode", Guid.NewGuid(), null, teamId, null);
			Now.Is("2014-10-20 9:00");

			Target.SaveState(state);

			Sender.LastMessage.DomainId.Should().Be(teamId.ToString());
		}

		[Test]
		public void ShouldUpdateTeamAdherenceAfterLoggingOutAgentMissingFromSnapshot()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode1", Guid.NewGuid(), businessUnitId, teamId, siteId)
				.WithUser("usercode2", personId, businessUnitId, teamId, siteId)
				.WithAlarm("statecode", null, 1)
				.WithAlarm("CCC Logged out", null, 0)
				;
			Now.Is("2014-10-20 10:00");
			
			Target.SaveStateSnapshot(new[]
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

			Target.SaveStateSnapshot(new[]
			{
				new ExternalUserStateForSnapshot("2014-10-20 10:05".Utc())
				{
					UserCode = "usercode1",
					StateCode = "statecode",
				}
			});
			
			var jsonResult = JsonConvert.DeserializeObject<TeamAdherenceMessage>(Sender.LastTeamMessage.BinaryData);
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
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode", phone, 1)
				;
			Now.Is("2014-10-20 9:00");
			
			Target.SaveState(state);
			Target.SaveState(state);

			Sender.AllNotifications.Where(x => x.DomainType == typeof(TeamAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
			Sender.AllNotifications.Where(x => x.DomainType == typeof(SiteAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
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
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, 1)
				.WithAlarm("phone", phone, 1)
				;
			Now.Is("2014-10-20 9:00");
			
			Target.SaveState(state);
			Target.SaveState(state2);

			Sender.AllNotifications.Where(x => x.DomainType == typeof(AgentsAdherenceMessage).Name).Should().Have.Count.EqualTo(2);
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
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("ready", phone, "my first state")
				.WithAlarm("phone", phone, "my second state")
				;
			Now.Is("2014-10-20 9:00");
			
			Target.SaveState(state);
			Target.SaveState(state2);

			var jsonResult = JsonConvert.DeserializeObject<AgentsAdherenceMessage>(Sender.LastAgentsMessage.BinaryData);
			jsonResult.AgentStates.Single().State.Should().Be.EqualTo("my second state");
		}

		[Test]
		public void ShouldSetBusinessIdOnTeamMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode"
			};
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnidId, Guid.NewGuid(), null);
			Now.Is("2014-10-20 9:00");
			
			Target.SaveState(state);

			Sender.LastTeamMessage.BusinessUnitId.Should().Be.EqualTo(businessUnidId.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnTeamMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode"
			};
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnidId, Guid.NewGuid(), null);
			Now.Is("2014-10-20 9:00");

			Target.SaveState(state);

			Sender.AllNotifications.Where(x => x.DomainType == typeof(TeamAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldSetBusinessIdOnSiteMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode"
			};
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnidId, null, Guid.NewGuid());
			Now.Is("2014-10-20 9:00");
			
			Target.SaveState(state);

			Sender.LastSiteMessage.BusinessUnitId.Should().Be.EqualTo(businessUnidId.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnSiteMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode"
			};
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnidId, null, Guid.NewGuid());
			Now.Is("2014-10-20 9:00");

			Target.SaveState(state);

			Sender.AllNotifications.Where(x => x.DomainType == typeof(SiteAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotSendMessageIfPersonIdIsNotAvailable()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
			};
			Database
				.WithSource(state.SourceId);
			Now.Is("2014-10-20 9:00");
			
			Target.SaveState(state);

			Sender.AllNotifications.Should().Be.Empty();
		}
	}
}