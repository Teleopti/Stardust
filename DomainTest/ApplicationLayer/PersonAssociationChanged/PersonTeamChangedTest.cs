using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[DomainTest]
	[AddDatasourceId]
	public class PersonTeamChangedTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;
		
		[Test]
		public void ShouldPublishWithProperties()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data
				.WithBusinessUnit(businessUnitId)
				.WithSite(siteId, "site")
				.WithTeam(teamId, "team")
				.WithAgent(personId, "pierre", teamId, siteId, businessUnitId)
				.WithDataSource(7, "7")
				.WithExternalLogon("usercode");

			Now.Is("2016-02-01 00:00:05");
			Target.Handle(new PersonTeamChangedEvent
			{
				Timestamp = "2016-02-01 00:00:01".Utc(),
				PersonId = personId,
				CurrentBusinessUnitId = businessUnitId,
				CurrentSiteId = siteId,
				CurrentTeamId = teamId,
				CurrentSiteName = "site",
				CurrentTeamName = "team"
			});

			var result = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			result.PersonId.Should().Be(personId);
			result.Timestamp.Should().Be("2016-02-01 00:00:05".Utc());
			result.BusinessUnitId.Should().Be(businessUnitId);
			result.SiteId.Should().Be(siteId);
			result.SiteName.Should().Be("site");
			result.TeamId.Should().Be(teamId);
			result.TeamName.Should().Be("team");
		}
	}
}