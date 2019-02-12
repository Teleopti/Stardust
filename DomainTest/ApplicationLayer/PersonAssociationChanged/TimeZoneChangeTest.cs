using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[DomainTest]
	[AddDatasourceId]
	public class TimeZoneChangeTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;
		public FakePersonRepository Persons;

		[Test]
		public void ShouldPublishWithTimeZone()
		{
			Now.Is("2018-10-25 00:00");
			var person = Guid.NewGuid();
			Data.WithAgent(person, "pierre", TimeZoneInfoFactory.Kathmandu());

			Target.Handle(new TenantHourTickEvent());

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == person);
			@event.TimeZone.Should().Be(TimeZoneInfoFactory.Kathmandu().Id);
		}

		[Test]
		public void ShouldPublishWhenTimeZoneChanges()
		{
			Now.Is("2018-10-25 00:00");
			var person = Guid.NewGuid();
			Data.WithAgent(person, "pierre", TimeZoneInfoFactory.Kathmandu());

			Target.Handle(new TenantHourTickEvent());
			Persons.LoadAll().Last().PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			Target.Handle(new TenantHourTickEvent());

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Last(x => x.PersonId == person);
			@event.TimeZone.Should().Be(TimeZoneInfoFactory.ChinaTimeZoneInfo().Id);
		}
	}
}