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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Preference.Analytics
{
	[TestFixture]
	public class PreferenceFulfillmentChangedHandlerTests
	{
		private PreferenceFulfillmentChangedHandler _target;
		private IPreferenceDayRepository _preferenceDayRepository;
		private FakePersonRepository _personRepository;
		private FakeEventPublisher _eventPublisher;

		[SetUp]
		public void Setup()
		{
			_preferenceDayRepository = new FakePreferenceDayRepository();
			_personRepository = new FakePersonRepository();
			_eventPublisher = new FakeEventPublisher();

			_target = new PreferenceFulfillmentChangedHandler(_preferenceDayRepository, _personRepository, _eventPublisher);
		}

		[Test]
		public void ShouldPublishEvent()
		{
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			person.WithId();
			_personRepository.Add(person);
			_preferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));

			var list = new Collection<ProjectionChangedEventScheduleDay>
			{
				new ProjectionChangedEventScheduleDay
				{
					Date = DateTime.Today
				}
			};

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
			_target.Handle(new ProjectionChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDays = list
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTwoEvents()
		{
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			person.WithId();
			_personRepository.Add(person);
			_preferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));
			_preferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today.AddDays(1), new PreferenceRestriction()));

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

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
			_target.Handle(new ProjectionChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDays = list
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldPublishOneEvents()
		{
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			person.WithId();
			_personRepository.Add(person);
			_preferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));
			_preferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today.AddDays(1), new PreferenceRestriction()));

			var list = new Collection<ProjectionChangedEventScheduleDay>
			{
				new ProjectionChangedEventScheduleDay
				{
					Date = DateTime.Today
				}
			};

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
			_target.Handle(new ProjectionChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDays = list
			});

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishEvent()
		{
			var person = PersonFactory.CreatePersonWithGuid("firstName", "lastName");
			person.WithId();
			_personRepository.Add(person);

			var list = new Collection<ProjectionChangedEventScheduleDay>
			{
				new ProjectionChangedEventScheduleDay
				{
					Date = DateTime.Today
				}
			};

			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
			_target.Handle(new ProjectionChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				ScheduleDays = list
			});
			_eventPublisher.PublishedEvents.Count().Should().Be.EqualTo(0);
		}
	}
}
