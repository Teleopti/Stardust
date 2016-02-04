using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.RTA_TerminatedPersons_36042)]
	[Toggle(Toggles.RTA_TeamChanges_36043)]
	public class PersonTeamChangedTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;

		[Test]
		public void ShouldPublishWhenTeamChanges()
		{
			Now.Is("2016-02-01 00:00");
			
			Target.Handle(new PersonTeamChangedEvent());

			Publisher.PublishedEvents.Single().Should().Be.OfType<PersonAssociationChangedEvent>();
		}


		[Test]
		public void ShouldPublishWithPropertiesWhenTeamChanges()
		{
			Now.Is("2016-02-01 00:00");
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			
			Target.Handle(new PersonTeamChangedEvent
			{
				PersonId = personId,
				CurrentBusinessUnitId = businessUnitId,
				CurrentSiteId = siteId,
				CurrentTeamId = teamId
			});

			var result = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			result.PersonId.Should().Be(personId);
			result.Timestamp.Should().Be("2016-02-01 00:00".Utc());
			result.BusinessUnitId.Should().Be(businessUnitId);
			result.SiteId.Should().Be(siteId);
			result.TeamId.Should().Be(teamId);
		}
	}
}