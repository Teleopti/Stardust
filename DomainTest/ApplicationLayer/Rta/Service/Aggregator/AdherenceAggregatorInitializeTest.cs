using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.Aggregator
{
	[RtaTest]
	[TestFixture]
	public class AdherenceAggregatorInitializeTest
	{
		public FakeRtaDatabase Database;
		public FakeMessageSender Sender;
		public MutableNow Now;
		public IRta Target;

		[Test]
		public void ShouldReadPersistedAggregatedState()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "user2",
				StateCode = "loggedoff",
			};
			var teamId = Guid.NewGuid();
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("user1", person1, null, teamId, null)
				.WithUser("user2", person2, null, teamId, null)
				.WithSchedule(person1, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(person2, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("loggedoff", phone, -1)
				;
			Database.PersistActualAgentReadModel(new AgentStateReadModel
			{
				PersonId = person1,
				StaffingEffect = -1
			});
			Now.Is("2014-10-20 9:00");

			Target.Initialize();
			Target.SaveState(state);

			Sender.LastTeamNotification.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldNotSendMessagesWhenInitializing()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			
			var teamId = Guid.NewGuid();
			var person1 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithDataFromState(state)
				.WithUser("usercode", person1, null, teamId, null)
				.WithSchedule(person1, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode", phone, -1)
				;
			Database.PersistActualAgentReadModel(new AgentStateReadModel
			{
				PersonId = person1,
				StaffingEffect = -1
			});
			Now.Is("2014-10-20 9:00");

			Target.Initialize();

			Sender.AllNotifications.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotSendMessageWhenSomeoneHasLeftTheOrganization()
		{
			Database.PersistActualAgentReadModel(new AgentStateReadModel { PersonId = Guid.NewGuid() });

			Target.Initialize();

			Sender.AllNotifications.Should().Be.Empty();
		}
	}
}