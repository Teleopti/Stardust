using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[DomainTest]
	[AddDatasourceId]
	public class DeletedPersonTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;
		public FakePersonRepository Persons;

		[Test]
		public void ShouldPublishWithoutAssociationWhenPersonDeleted()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data
				.WithBusinessUnit(businessUnitId)
				.WithSite(siteId, "site")
				.WithTeam(teamId, "team")
				.WithAgent(personId, "pierre boldi", teamId, siteId, businessUnitId, 1234)
				.WithDataSource(7, "7")
				.WithExternalLogon("usercode");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			((IDeleteTag) Persons.Get(personId)).SetDeleted();
			Target.Handle(new PersonDeletedEvent {PersonId = personId});

			var result = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == personId);
			result.PersonId.Should().Be(personId);
			result.BusinessUnitId.Should().Be.EqualTo(null);
			result.SiteId.Should().Be.EqualTo(null);
			result.SiteName.Should().Be.EqualTo(null);
			result.TeamId.Should().Be.EqualTo(null);
			result.TeamName.Should().Be.EqualTo(null);
			result.ExternalLogons.Should().Be.Empty();
			result.FirstName.Should().Be.EqualTo("pierre");
			result.LastName.Should().Be.EqualTo("boldi");
			result.EmploymentNumber.Should().Be("1234");
		}

		[Test]
		public void ShouldPublishWithoutAssociationOnHourTickWhenPersonDeleted()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data
				.WithBusinessUnit(businessUnitId)
				.WithSite(siteId, "site")
				.WithTeam(teamId, "team")
				.WithAgent(personId, "pierre boldi", teamId, siteId, businessUnitId, 1234)
				.WithDataSource(7, "7")
				.WithExternalLogon("usercode");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			((IDeleteTag) Persons.Get(personId)).SetDeleted();
			Target.Handle(new TenantHourTickEvent());

			var result = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == personId);
			result.PersonId.Should().Be(personId);
			result.TeamId.Should().Be.EqualTo(null);
			result.ExternalLogons.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotThrowIfPersonDoesntExistAnymore()
		{
			var person = Guid.NewGuid();
			Data.WithAgent(person, "pierre");
			Now.Is("2017-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Data.RemovePerson(person);
			Now.Is("2017-03-20 08:00");
			Assert.DoesNotThrow(() => { Target.Handle(new PersonTeamChangedEvent {PersonId = person}); });
		}
	}
}