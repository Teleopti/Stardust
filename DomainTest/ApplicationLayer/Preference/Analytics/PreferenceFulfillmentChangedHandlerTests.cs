using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Preference;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Preference.Analytics
{
	[TestFixture]
	[DomainTest]
	public class PreferenceFulfillmentChangedHandlerTests : IExtendSystem
	{
		public PreferenceFulfillmentChangedHandler Target;
		public IPreferenceDayRepository PreferenceDayRepository;
		public FakePersonRepository PersonRepository;
		public FakeEventPublisher EventPublisher;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<PreferenceFulfillmentChangedHandler>();
		}

		[Test]
		public void ShouldPublishEvent()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var person = PersonFactory.CreatePerson(timeZone).WithId();
			PersonRepository.Add(person);
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));

			EventPublisher.PublishedEvents.OfType<PreferenceChangedEvent>().Count().Should().Be.EqualTo(0);
			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = timeZone.SafeConvertTimeToUtc(DateTime.Today),
				EndDateTime = timeZone.SafeConvertTimeToUtc(DateTime.Today),
				LogOnBusinessUnitId = businessUnitId
			});

			EventPublisher.PublishedEvents.OfType<PreferenceChangedEvent>().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTwoEvents()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var zoneInfo = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var person = PersonFactory.CreatePerson(zoneInfo).WithId();
			PersonRepository.Add(person);
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today.AddDays(1), new PreferenceRestriction()));

			EventPublisher.PublishedEvents.OfType<PreferenceChangedEvent>().Count().Should().Be.EqualTo(0);
			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = zoneInfo.SafeConvertTimeToUtc(DateTime.Today),
				EndDateTime = zoneInfo.SafeConvertTimeToUtc(DateTime.Today.AddDays(1)),
				LogOnBusinessUnitId = businessUnitId
			});

			EventPublisher.PublishedEvents.OfType<PreferenceChangedEvent>().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldPublishOneEvents()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var person = PersonFactory.CreatePerson(timeZone).WithId();
			PersonRepository.Add(person);
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction()));
			PreferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today.AddDays(1), new PreferenceRestriction()));

			EventPublisher.PublishedEvents.OfType<PreferenceChangedEvent>().Count().Should().Be.EqualTo(0);
			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = timeZone.SafeConvertTimeToUtc(DateTime.Today),
				EndDateTime = timeZone.SafeConvertTimeToUtc(DateTime.Today),
				LogOnBusinessUnitId = businessUnitId
			});

			EventPublisher.PublishedEvents.OfType<PreferenceChangedEvent>().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishEvent()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var person = PersonFactory.CreatePerson(timeZone).WithId();
			PersonRepository.Add(person);

			EventPublisher.PublishedEvents.OfType<PreferenceChangedEvent>().Count().Should().Be.EqualTo(0);
			Target.Handle(new ScheduleChangedEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = timeZone.SafeConvertTimeToUtc(DateTime.Today),
				EndDateTime = timeZone.SafeConvertTimeToUtc(DateTime.Today),
				LogOnBusinessUnitId = businessUnitId
			});
			EventPublisher.PublishedEvents.OfType<PreferenceChangedEvent>().Count().Should().Be.EqualTo(0);
		}
	}
}
