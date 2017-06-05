
using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	[DomainTest]
	public class DeletedPersonTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;


		[Test]
		public void ShouldPublishWhenPersonDeleted()
		{
			var person = Guid.NewGuid();
			Data.WithAgent(person, "pierre");
			Now.Is("2017-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Data.RemovePerson(person);
			Now.Is("2017-03-20 08:00");
			Target.Handle(new PersonDeletedEvent { PersonId = person });

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
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
			Assert.DoesNotThrow(() =>
			{
				Target.Handle(new PersonTeamChangedEvent { PersonId = person });
			});
		}
		
	}
}