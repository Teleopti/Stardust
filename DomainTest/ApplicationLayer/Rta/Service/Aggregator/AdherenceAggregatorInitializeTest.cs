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
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldReadPersistedAggregatedState()
		{
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

			Target.SaveState(
				new ExternalUserStateForTest
				{
					UserCode = "user2",
					StateCode = "loggedoff",
				});

			Sender.LastTeamMessage.DeserializeBindaryData<TeamAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}
	}
}