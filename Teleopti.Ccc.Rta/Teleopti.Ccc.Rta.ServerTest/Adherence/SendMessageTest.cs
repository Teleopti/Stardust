using System;
using System.Linq;
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
	public class SendMessageTest
	{
		[Test]
		public void ShouldSendMessageForTeam()
		{
			var agentState = new ActualAgentState();
			var teamId = Guid.NewGuid();

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var target = new AdherenceAggregator(broker, organizationForPerson);

			organizationForPerson.Stub(x => x.GetOrganization(agentState.PersonId)).Return(new PersonOrganizationData{TeamId = teamId});

			target.Invoke(agentState);

			broker.LastNotification.DomainId.Should().Be(teamId.ToString());
		}

		[Test]
		public void ShouldNotSendMessageIfAdherenceHasNotChanged()
		{
			var oldState = new ActualAgentState { StaffingEffect = 1 };
			var newState = new ActualAgentState { StaffingEffect = 1 };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var target = new AdherenceAggregator(broker, organizationForPerson);

			organizationForPerson.Stub(x => x.GetOrganization(Guid.Empty)).Return(new PersonOrganizationData());

			target.Invoke(oldState);
			target.Invoke(newState);

			broker.AllNotifications.Select(x => x.DomainType)
				.Should().Have.SameValuesAs("TeamAdherenceMessage", "SiteAdherenceMessage", "AgentsAdherenceMessage");
		}

		[Test]
		public void ShouldSetBusinessIdOnTeamMessage()
		{
			var agentState = new ActualAgentState{BusinessUnit = Guid.NewGuid()};

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var target = new AdherenceAggregator(broker, organizationForPerson);

			organizationForPerson.Stub(x => x.GetOrganization(agentState.PersonId)).Return(new PersonOrganizationData());

			target.Invoke(agentState);

			broker.LastTeamNotification.BusinessUnitId.Should().Be.EqualTo(agentState.BusinessUnit.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnTeamMessage()
		{
			var agentState = new ActualAgentState();

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var target = new AdherenceAggregator(broker, organizationForPerson);

			organizationForPerson.Stub(x => x.GetOrganization(agentState.PersonId)).Return(new PersonOrganizationData());

			target.Invoke(agentState);

			broker.LastNotification.DomainType.Should().Be.EqualTo(typeof(AgentsAdherenceMessage).Name);
		}

		[Test]
		public void ShouldSetBusinessIdOnSiteMessage()
		{
			var agentState = new ActualAgentState { BusinessUnit = Guid.NewGuid() };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var target = new AdherenceAggregator(broker, organizationForPerson);

			organizationForPerson.Stub(x => x.GetOrganization(agentState.PersonId)).Return(new PersonOrganizationData());

			target.Invoke(agentState);

			broker.LastNotification.BusinessUnitId.Should().Be.EqualTo(agentState.BusinessUnit.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnSiteMessage()
		{
			var agentState = new ActualAgentState();

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var target = new AdherenceAggregator(broker, organizationForPerson);

			organizationForPerson.Stub(x => x.GetOrganization(agentState.PersonId)).Return(new PersonOrganizationData());

			target.Invoke(agentState);

			broker.LastSiteNotification.DomainType.Should().Be.EqualTo(typeof(SiteAdherenceMessage).Name);
		}
	}
}