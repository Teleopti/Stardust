using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Requests
{
	[TestFixture]
	public class ShiftExchangeOfferPersisterTest
	{
		[Test]
		public void ShouldStoreShiftExchangeOfferLookingForOtherShift()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var person = PersonFactory.CreatePerson();
			var date = new DateOnly(2029, 1, 1);

			var schedule = ScheduleDayFactory.Create(date, person);
			var scheduleProvider = new FakeScheduleProvider(schedule);

			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				MockRepository.GenerateMock<IMappingEngine>(),
				new ShiftExchangeOfferMapper(new FakeLoggedOnUser(person), scheduleProvider));


			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date,
						StartTime = TimeSpan.FromHours(8),
						EndTime = TimeSpan.FromHours(9.25),
						WishShiftType = ShiftExchangeLookingForDay.WorkingShift
					},
					ShiftExchangeOfferStatus.Pending);

				var matchingWantedSchedule = ScheduleDayFactory.Create(date,PersonFactory.CreatePerson("assert"));
				matchingWantedSchedule.CreateAndAddActivity(ActivityFactory.CreateActivity("Phone"),
					new DateTimePeriod(new DateTime(2029, 1, 1, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2029, 1, 1, 9, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Early"));

				personRequestRepository.AssertWasCalled(x => x.Add(null),
					o =>
						o.Constraints(
							Rhino.Mocks.Constraints.Is.Matching<IPersonRequest>(
								r => ((ShiftExchangeOffer) r.Request).IsWantedSchedule(matchingWantedSchedule))));
			}
		}

		[Test]
		public void ShouldStoreShiftExchangeOfferLookingForEmptyDay()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var person = PersonFactory.CreatePerson();
			var date = new DateOnly(2029, 1, 1);

			var schedule = ScheduleDayFactory.Create(date, person);
			var scheduleProvider = new FakeScheduleProvider(schedule);

			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				MockRepository.GenerateMock<IMappingEngine>(),
				new ShiftExchangeOfferMapper(new FakeLoggedOnUser(person), scheduleProvider));


			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date,
						WishShiftType = ShiftExchangeLookingForDay.EmtpyDay
					},
					ShiftExchangeOfferStatus.Pending);

				var matchingWantedSchedule = ScheduleDayFactory.Create(date, PersonFactory.CreatePerson("assert"));
			
				personRequestRepository.AssertWasCalled(x => x.Add(null),
					o =>
						o.Constraints(
							Rhino.Mocks.Constraints.Is.Matching<IPersonRequest>(
								r => ((ShiftExchangeOffer)r.Request).IsWantedSchedule(matchingWantedSchedule))));
			}
		}

		[Test]
		public void ShouldStoreShiftExchangeOfferLookingForDayOff()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var person = PersonFactory.CreatePerson();
			var date = new DateOnly(2029, 1, 1);

			var schedule = ScheduleDayFactory.Create(date, person);
			var scheduleProvider = new FakeScheduleProvider(schedule);

			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				MockRepository.GenerateMock<IMappingEngine>(),
				new ShiftExchangeOfferMapper(new FakeLoggedOnUser(person), scheduleProvider));


			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date,
						WishShiftType = ShiftExchangeLookingForDay.DayOff
					},
					ShiftExchangeOfferStatus.Pending);

				var matchingWantedSchedule = ScheduleDayFactory.Create(date, PersonFactory.CreatePerson("assert"));
				matchingWantedSchedule.CreateAndAddDayOff(new DayOffTemplate());
				
				personRequestRepository.AssertWasCalled(x => x.Add(null),
					o =>
						o.Constraints(
							Rhino.Mocks.Constraints.Is.Matching<IPersonRequest>(
								r => ((ShiftExchangeOffer)r.Request).IsWantedSchedule(matchingWantedSchedule))));
			}
		}

		[Test]
		public void ShouldStoreShiftExchangeOfferLookingForDayOffAndCriteriaShouldNotMatchEmptyDay()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var person = PersonFactory.CreatePerson();
			var date = new DateOnly(2029, 1, 1);

			var schedule = ScheduleDayFactory.Create(date, person);
			var scheduleProvider = new FakeScheduleProvider(schedule);

			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				MockRepository.GenerateMock<IMappingEngine>(),
				new ShiftExchangeOfferMapper(new FakeLoggedOnUser(person), scheduleProvider));

			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date,
						WishShiftType = ShiftExchangeLookingForDay.DayOff
					},
					ShiftExchangeOfferStatus.Pending);

				var matchingWantedSchedule = ScheduleDayFactory.Create(date, PersonFactory.CreatePerson("assert"));
				
				personRequestRepository.AssertWasCalled(x => x.Add(null),
					o =>
						o.Constraints(
							Rhino.Mocks.Constraints.Is.Matching<IPersonRequest>(
								r => !((ShiftExchangeOffer)r.Request).IsWantedSchedule(matchingWantedSchedule))));
			}
		}

		[Test]
		public void ShouldStoreShiftExchangeOfferLookingForDayOffOrEmptyDay()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var person = PersonFactory.CreatePerson();
			var date = new DateOnly(2029, 1, 1);

			var schedule = ScheduleDayFactory.Create(date, person);
			var scheduleProvider = new FakeScheduleProvider(schedule);

			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				MockRepository.GenerateMock<IMappingEngine>(),
				new ShiftExchangeOfferMapper(new FakeLoggedOnUser(person), scheduleProvider));

			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date,
						WishShiftType = ShiftExchangeLookingForDay.DayOffOrEmptyDay
					},
					ShiftExchangeOfferStatus.Pending);

				var emptyDay = ScheduleDayFactory.Create(date, PersonFactory.CreatePerson("assert"));
				var dayOff = ScheduleDayFactory.Create(date, PersonFactory.CreatePerson("assert day off"));
				dayOff.CreateAndAddDayOff(new DayOffTemplate());

				personRequestRepository.AssertWasCalled(x => x.Add(null),
					o =>
						o.Constraints(
							Rhino.Mocks.Constraints.Is.Matching<IPersonRequest>(
								r =>
								{
									var shiftExchangeOffer = ((ShiftExchangeOffer) r.Request);
									return shiftExchangeOffer.IsWantedSchedule(emptyDay) && shiftExchangeOffer.IsWantedSchedule(dayOff);
								})));
			}
		}
	}
}