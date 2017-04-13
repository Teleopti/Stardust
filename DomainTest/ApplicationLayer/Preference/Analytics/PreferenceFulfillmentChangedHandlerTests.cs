using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Preference;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using System.Collections.ObjectModel;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Preference.Analytics
{
	[TestFixture]
	[DomainTest]
	public class PreferenceFulfillmentChangedHandlerTests : ISetup
	{
		public PreferenceFulfillmentChangedHandler Target;
		public IPreferenceDayRepository PreferenceDayRepository;
		public FakePersonRepository PersonRepository;
		public FakeEventPublisher EventPublisher;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<PreferenceFulfillmentChangedHandler>();
		}

		[Test]
		public void ShouldPublishEvent()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var person = PersonFactory.CreatePerson("firstName", "lastName").WithId();
			PersonRepository.Add(person);
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));

			var list = new Collection<ProjectionChangedEventScheduleDay>
			{
				new ProjectionChangedEventScheduleDay
				{
					Date = DateTime.Today
				}
			};

			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
			Target.Handle(new ProjectionChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDays = list,
				LogOnBusinessUnitId = businessUnitId
			});

			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTwoEvents()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var person = PersonFactory.CreatePerson("firstName", "lastName").WithId();
			PersonRepository.Add(person);
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today.AddDays(1), new PreferenceRestriction()));

			var list = new Collection<ProjectionChangedEventScheduleDay>
			{
				new ProjectionChangedEventScheduleDay
				{
					Date = DateTime.Today
				},
				new ProjectionChangedEventScheduleDay
				{
					Date = DateTime.Today.AddDays(1)
				}
			};

			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
			Target.Handle(new ProjectionChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDays = list,
				LogOnBusinessUnitId = businessUnitId
			});

			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldPublishOneEvents()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var person = PersonFactory.CreatePerson("firstName", "lastName").WithId();
			PersonRepository.Add(person);
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today.AddDays(1), new PreferenceRestriction()));

			var list = new Collection<ProjectionChangedEventScheduleDay>
			{
				new ProjectionChangedEventScheduleDay
				{
					Date = DateTime.Today
				}
			};

			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
			Target.Handle(new ProjectionChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDays = list,
				LogOnBusinessUnitId = businessUnitId
			});

			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishEvent()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var person = PersonFactory.CreatePerson("firstName", "lastName").WithId();
			PersonRepository.Add(person);

			var list = new Collection<ProjectionChangedEventScheduleDay>
			{
				new ProjectionChangedEventScheduleDay
				{
					Date = DateTime.Today
				}
			};

			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
			Target.Handle(new ProjectionChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDays = list,
				LogOnBusinessUnitId = businessUnitId
			});
			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
		}
	}
}
