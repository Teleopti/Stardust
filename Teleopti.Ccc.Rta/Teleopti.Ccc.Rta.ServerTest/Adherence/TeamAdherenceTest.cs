using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class TeamAdherenceTest
	{
		[Test]
		public void ShouldMapOutOfAdherenceBasedOnPositiveStaffingEffect()
		{
			var teamId = Guid.NewGuid();
			var inAdherence = new ActualAgentState { StaffingEffect = 0 };
			var outOfAdherence = new ActualAgentState { StaffingEffect = 1 };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			organizationForPerson.Stub(x => x.GetOrganization(Guid.Empty)).IgnoreArguments()
				.Return(new PersonOrganizationData { TeamId = teamId });
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(inAdherence);
			target.Invoke(outOfAdherence);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldMapOutOfAdherenceBasedOnNegativeStaffingEffect()
		{
			var teamId = Guid.NewGuid();
			var inAdherence = new ActualAgentState { StaffingEffect = 0 };
			var outOfAdherence = new ActualAgentState { StaffingEffect = -1 };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			organizationForPerson.Stub(x => x.GetOrganization(Guid.Empty)).IgnoreArguments()
				.Return(new PersonOrganizationData { TeamId = teamId });
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(inAdherence);
			target.Invoke(outOfAdherence);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInATeam()
		{
			var outOfAdherence1 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };
			var outOfAdherence2 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var team = Guid.NewGuid();
			organizationForPerson.Expect(x => x.GetOrganization(outOfAdherence1.PersonId)).Return(new PersonOrganizationData{TeamId = team});
			organizationForPerson.Expect(x => x.GetOrganization(outOfAdherence2.PersonId)).Return(new PersonOrganizationData { TeamId = team });
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(outOfAdherence1);
			target.Invoke(outOfAdherence2);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInDifferentTeam()
		{
			var outOfAdherence1 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };
			var outOfAdherence2 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			organizationForPerson.Expect(x => x.GetOrganization(outOfAdherence1.PersonId)).Return(new PersonOrganizationData{TeamId = Guid.NewGuid()});
			organizationForPerson.Expect(x => x.GetOrganization(outOfAdherence2.PersonId)).Return(new PersonOrganizationData { TeamId = Guid.NewGuid() });
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(outOfAdherence1);
			target.Invoke(outOfAdherence2);

			broker.LastNotification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}
	}
}
