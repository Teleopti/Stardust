using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class SiteAdherenceTes
	{
		[Test]
		public void ShouldMapOutOfAdherenceBasedOnPositiveStaffingEffect()
		{
			var inAdherence = new ActualAgentState { StaffingEffect = 0 };
			var outOfAdherence = new ActualAgentState { StaffingEffect = 1 };

			var broker = new MessageSenderExposingLastNotification();
			var siteIdForPerson = MockRepository.GenerateMock<ISiteIdForPerson>();
			var target = new AdherenceAggregator(broker, null, siteIdForPerson);

			target.Invoke(inAdherence);
			target.Invoke(outOfAdherence);

			broker.LastNotification.GetOriginal<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInOneSite()
		{
			var outOfAdherence1 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };
			var outOfAdherence2 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };

			var broker = new MessageSenderExposingLastNotification();
			var siteProvider = MockRepository.GenerateMock<ISiteIdForPerson>();
			var siteId = Guid.NewGuid();
			siteProvider.Expect(x => x.GetSiteId(outOfAdherence1.PersonId)).Return(siteId);
			siteProvider.Expect(x => x.GetSiteId(outOfAdherence2.PersonId)).Return(siteId);
			var target = new AdherenceAggregator(broker, null, siteProvider);

			target.Invoke(outOfAdherence1);
			target.Invoke(outOfAdherence2);

			broker.LastNotification.GetOriginal<SiteAdherenceMessage>().OutOfAdherence.Should().Be(2);
		} 
	}
}