using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.Common;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Ccc.WebTest.Core.Requests.DataProvider;
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

			var loggedOnUser = new FakeLoggedOnUser(person);
			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), loggedOnUser, new EmptyShiftTradeRequestChecker(), new FakePersonNameProvider(), new FakeToggleManager()),
				new ShiftExchangeOfferMapper(loggedOnUser, scheduleProvider));


			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date.Date,
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

			var loggedOnUser = new FakeLoggedOnUser(person);
			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), loggedOnUser, new EmptyShiftTradeRequestChecker(), new FakePersonNameProvider(), new FakeToggleManager()),
				new ShiftExchangeOfferMapper(loggedOnUser, scheduleProvider));


			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date.Date,
						WishShiftType = ShiftExchangeLookingForDay.EmptyDay
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

			var loggedOnUser = new FakeLoggedOnUser(person);
			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), loggedOnUser, new EmptyShiftTradeRequestChecker(), new FakePersonNameProvider(), new FakeToggleManager()),
				new ShiftExchangeOfferMapper(loggedOnUser, scheduleProvider));


			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date.Date,
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

			var loggedOnUser = new FakeLoggedOnUser(person);
			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), loggedOnUser, new EmptyShiftTradeRequestChecker(), new FakePersonNameProvider(), new FakeToggleManager()),
				new ShiftExchangeOfferMapper(loggedOnUser, scheduleProvider));

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date.Date,
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

			var loggedOnUser = new FakeLoggedOnUser(person);
			var target = new ShiftExchangeOfferPersister(personRequestRepository,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), loggedOnUser, new EmptyShiftTradeRequestChecker(), new FakePersonNameProvider(), new FakeToggleManager()),
				new ShiftExchangeOfferMapper(loggedOnUser, scheduleProvider));

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				target.Persist(
					new ShiftExchangeOfferForm
					{
						OfferValidTo = new DateTime(2028, 12, 23),
						Date = date.Date,
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