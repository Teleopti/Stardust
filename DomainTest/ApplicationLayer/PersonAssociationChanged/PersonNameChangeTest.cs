using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[DomainTest]
	[AddDatasourceId]
	public class PersonNameChangeTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public FakeDatabase Data;
		public FakePersonRepository Persons;

		[Test]
		public void ShouldPublishPersonNameChange()
		{
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "anakin skywalker");

			Target.Handle(new TenantHourTickEvent());
			Persons.LoadAll().Last().SetName(new Name("darth", "vader"));
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Last().FirstName.Should().Be("darth");
			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Last().LastName.Should().Be("vader");
		}
		
		[Test]
		public void ShouldPublishEmploymentNumberChange()
		{
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "name", 1234);

			Target.Handle(new TenantHourTickEvent());
			Persons.LoadAll().Last().SetEmploymentNumber("1235");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Last().EmploymentNumber.Should().Be("1235");
		}
	}
}