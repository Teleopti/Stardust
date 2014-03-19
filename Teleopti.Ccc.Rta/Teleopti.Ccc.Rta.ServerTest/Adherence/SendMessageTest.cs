using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
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

			var broker = new MessageSenderExposingLastNotification();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPerson>();
			var target = new AdherenceAggregator(broker, teamProvider, null);

			teamProvider.Stub(x => x.GetTeamId(agentState.PersonId)).Return(teamId);

			target.Invoke(agentState);

			broker.LastNotification.DomainId.Should().Be(teamId.ToString());
		}

		[Test]
		public void ShouldNotSendMessageIfAdherenceHasNotChanged()
		{
			var oldState = new ActualAgentState { StaffingEffect = 1 };
			var newState = new ActualAgentState { StaffingEffect = 1 };

			var broker = MockRepository.GenerateMock<IMessageSender>();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPerson>();
			var target = new AdherenceAggregator(broker, teamProvider, null);

			target.Invoke(oldState);
			target.Invoke(newState);

			broker.AssertWasCalled(x => x.SendNotification(null), a => a.IgnoreArguments().Repeat.Once());
		}

		[Test]
		public void ShouldSetBusinessIdOnTeamMessage()
		{
			var agentState = new ActualAgentState{BusinessUnit = Guid.NewGuid()};

			var broker = new MessageSenderExposingLastNotification();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPerson>();
			var target = new AdherenceAggregator(broker, teamProvider, null);

			teamProvider.Stub(x => x.GetTeamId(agentState.PersonId)).Return(Guid.NewGuid());

			target.Invoke(agentState);

			broker.LastNotification.BusinessUnitId.Should().Be.EqualTo(agentState.BusinessUnit.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnTeamMessage()
		{
			var agentState = new ActualAgentState();

			var broker = new MessageSenderExposingLastNotification();
			var teamProvider = MockRepository.GenerateMock<ITeamIdForPerson>();
			var target = new AdherenceAggregator(broker, teamProvider, null);

			teamProvider.Stub(x => x.GetTeamId(agentState.PersonId)).Return(Guid.NewGuid());

			target.Invoke(agentState);

			broker.LastNotification.DomainType.Should().Be.EqualTo(typeof(TeamAdherenceMessage).Name);
		}

		[Test]
		public void ShouldSetBusinessIdOnSiteMessage()
		{
			var agentState = new ActualAgentState { BusinessUnit = Guid.NewGuid() };

			var broker = new MessageSenderExposingLastNotification();
			var siteProvider = MockRepository.GenerateMock<ISiteIdForPerson>();
			var target = new AdherenceAggregator(broker, null, siteProvider);

			siteProvider.Stub(x => x.GetSiteId(agentState.PersonId)).Return(Guid.NewGuid());

			target.Invoke(agentState);

			broker.LastNotification.BusinessUnitId.Should().Be.EqualTo(agentState.BusinessUnit.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnSiteMessage()
		{
			var agentState = new ActualAgentState();

			var broker = new MessageSenderExposingLastNotification();
			var siteProvider = MockRepository.GenerateMock<ISiteIdForPerson>();
			var target = new AdherenceAggregator(broker, null, siteProvider);

			siteProvider.Stub(x => x.GetSiteId(agentState.PersonId)).Return(Guid.NewGuid());

			target.Invoke(agentState);

			broker.LastNotification.DomainType.Should().Be.EqualTo(typeof(SiteAdherenceMessage).Name);
		}
	}
}