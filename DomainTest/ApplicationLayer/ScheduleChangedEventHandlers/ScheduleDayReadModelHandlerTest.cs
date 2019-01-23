using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[DomainTest]
	public class ScheduleDayReadModelHandlerTest
	{
		private ScheduleReadModelWrapperHandler _target;
		private IPersonRepository _personRepository;
		private IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private IPerson _person;
		private INotificationValidationCheck _notificationValidationCheck;

		[SetUp]
		public void Setup()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_notificationValidationCheck = MockRepository.GenerateMock<INotificationValidationCheck>();
			_scheduleDayReadModelsCreator = MockRepository.GenerateMock<IScheduleDayReadModelsCreator>();
			_scheduleDayReadModelRepository = MockRepository.GenerateMock<IScheduleDayReadModelRepository>();
			var teamScheduleWeekViewChangeCheck = MockRepository.GenerateMock<ITeamScheduleWeekViewChangeCheck>();
			var persister = new ScheduleDayReadModelPersister(_personRepository,
				_notificationValidationCheck, _scheduleDayReadModelsCreator, _scheduleDayReadModelRepository, teamScheduleWeekViewChangeCheck);
			_target = new ScheduleReadModelWrapperHandler(persister, null, null, null);
			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.NewGuid());
		}

		[Test]
		public void ShouldSkipOutIfNotDefaultScenario()
		{
			_target.Handle(new ProjectionChangedEventNew { IsDefaultScenario = false });
			_personRepository.AssertWasNotCalled(x => x.Get(Arg<Guid>.Is.Anything));
		}

		[Test]
		public void ShouldCreateReadModel()
		{
			var period = new DateTimePeriod(new DateTime(2012, 12, 1, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2012, 12, 1, 17, 0, 0, DateTimeKind.Utc));
			var dateOnlyPeriod = period.ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());
			var model = new ScheduleDayReadModel();
			var denormalizedScheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = dateOnlyPeriod.StartDate.Date,
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime
				}
			};
			var message = new ProjectionChangedEventNew
			{
				PersonId = _person.Id.GetValueOrDefault(),
				IsDefaultScenario = true,
				IsInitialLoad = false,
				ScheduleDays = new[] { denormalizedScheduleDay }
			};
			_personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(_person);
			_scheduleDayReadModelsCreator.Stub(x => x.GetReadModel(denormalizedScheduleDay, _person)).Return(model);
			_target.Handle(message);
			_notificationValidationCheck.AssertWasCalled(x =>
				x.InitiateNotify(model, new DateOnly(denormalizedScheduleDay.Date), _person));
			_scheduleDayReadModelRepository.AssertWasCalled(x => x.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId));
			_scheduleDayReadModelRepository.AssertWasCalled(x => x.SaveReadModel(model));
		}

		[Test]
		public void ShouldCreateReadModelWithoutNotifying()
		{
			var period = new DateTimePeriod(new DateTime(2012, 12, 1, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2012, 12, 1, 17, 0, 0, DateTimeKind.Utc));
			var dateOnlyPeriod = period.ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());
			var model = new ScheduleDayReadModel();
			var denormalizedScheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = dateOnlyPeriod.StartDate.Date,
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime
				}
			};
			var message = new ProjectionChangedEventNew
			{
				PersonId = _person.Id.GetValueOrDefault(),
				IsDefaultScenario = true,
				IsInitialLoad = true,
				ScheduleDays = new[] { denormalizedScheduleDay }
			};
			_personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(_person);
			_scheduleDayReadModelsCreator.Stub(x => x.GetReadModel(denormalizedScheduleDay, _person)).Return(model);
			_target.Handle(message);
			_notificationValidationCheck.AssertWasNotCalled(x =>
				x.InitiateNotify(model, new DateOnly(denormalizedScheduleDay.Date), _person));
			_scheduleDayReadModelRepository.AssertWasNotCalled(x => x.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId));
			_scheduleDayReadModelRepository.AssertWasCalled(x => x.SaveReadModel(model));
		}
	}
}