﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server.Adherence
{
	public class SiteAdherenceTest
	{
		[Test]
		public void ShouldMapOutOfAdherenceBasedOnPositiveStaffingEffect()
		{
			var siteId = Guid.NewGuid();
			var inAdherence = new ActualAgentState { StaffingEffect = 0 };
			var outOfAdherence = new ActualAgentState { StaffingEffect = 1 };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			organizationForPerson.Stub(x => x.GetOrganization(Guid.Empty)).IgnoreArguments()
				.Return(new PersonOrganizationData { SiteId = siteId });
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(inAdherence);
			target.Invoke(outOfAdherence);

			broker.LastSiteNotification.GetOriginal<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldMapOutOfAdherenceBasedOnNegativeStaffingEffect()
		{
			var siteId = Guid.NewGuid();
			var inAdherence = new ActualAgentState { StaffingEffect = 0 };
			var outOfAdherence = new ActualAgentState { StaffingEffect = -1 };
			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			organizationForPerson.Stub(x => x.GetOrganization(Guid.Empty)).IgnoreArguments()
				.Return(new PersonOrganizationData {SiteId = siteId});

			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(inAdherence);
			target.Invoke(outOfAdherence);

			broker.LastSiteNotification.GetOriginal<SiteAdherenceMessage>().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsOnASite()
		{
			var outOfAdherence1 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };
			var outOfAdherence2 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var site = Guid.NewGuid();
			organizationForPerson.Expect(x => x.GetOrganization(outOfAdherence1.PersonId)).Return(new PersonOrganizationData { SiteId = site, PersonId = outOfAdherence1.PersonId });
			organizationForPerson.Expect(x => x.GetOrganization(outOfAdherence2.PersonId)).Return(new PersonOrganizationData { SiteId = site, PersonId = outOfAdherence2.PersonId });
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(outOfAdherence1);
			target.Invoke(outOfAdherence2);

			broker.LastSiteNotification.GetOriginal<SiteAdherenceMessage>().OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldAggregateAdherenceFor2PersonsInOneSite()
		{
			var outOfAdherence1 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };
			var outOfAdherence2 = new ActualAgentState { StaffingEffect = 1, PersonId = Guid.NewGuid() };

			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			var siteId = Guid.NewGuid();
			organizationForPerson.Expect(x => x.GetOrganization(outOfAdherence1.PersonId)).Return(new PersonOrganizationData { SiteId = siteId, PersonId = outOfAdherence1.PersonId });
			organizationForPerson.Expect(x => x.GetOrganization(outOfAdherence2.PersonId)).Return(new PersonOrganizationData { SiteId = siteId, PersonId = outOfAdherence2.PersonId });
			var target = new AdherenceAggregator(broker, organizationForPerson);

			target.Invoke(outOfAdherence1);
			target.Invoke(outOfAdherence2);

			broker.LastSiteNotification.GetOriginal<SiteAdherenceMessage>().OutOfAdherence.Should().Be(2);
		} 
	}
}