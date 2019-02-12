using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[DomainTest]
	[NoDefaultData]
	public class ShiftTradeRequestApprovalServiceTest : IIsolateSystem
	{
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public ICurrentScenario Scenario;
		public FakeGlobalSettingDataRepository GlobalSettingDataRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public IScheduleStorage ScheduleStorage;
		public ICurrentAuthorization CurrentAuthorization;

		private IRequestApprovalService _requestApprovalService;
		private IScheduleDictionary _scheduleDictionary;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario() {DefaultScenario = true}.WithId()))
				.For<IScenarioRepository>();
		}

		[Test]
		public void ShouldApproveOnlyOneRequestOfMultipleShiftTradeRequetsFromSamePersonInSameDay()
		{
			var personFrom = PersonFactory.CreatePersonWithId();
			var personTo1 = PersonFactory.CreatePersonWithId();
			var personTo2 = PersonFactory.CreatePersonWithId();
			var personTo3 = PersonFactory.CreatePersonWithId();

			var scheduleDatas = createScheduleDatas(new DateOnlyPeriod(2016, 11, 23, 2016, 11, 23), new TimePeriod(8, 17), personFrom).ToList();
			scheduleDatas.AddRange(createScheduleDatas(new DateOnlyPeriod(2016, 11, 23, 2016, 11, 23), new TimePeriod(7, 18), personTo1,
				personTo2, personTo3));
			_scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(Scenario.Current(), new DateTimePeriod(2016, 11, 23, 7, 2016, 11, 23, 18), CurrentAuthorization, scheduleDatas.ToArray());

			var requestDate = new DateOnly(2016, 11, 23);
			var shiftTradeRequest1 = createPersonShiftTradeRequest(personFrom, personTo1, requestDate);
			var shiftTradeRequest2 = createPersonShiftTradeRequest(personFrom, personTo2, requestDate);
			var shiftTradeRequest3 = createPersonShiftTradeRequest(personFrom, personTo3, requestDate);

			setShiftTradeRequestApprovalService(personFrom);
			_requestApprovalService.Approve(shiftTradeRequest1.Request);
			_requestApprovalService.Approve(shiftTradeRequest2.Request);
			_requestApprovalService.Approve(shiftTradeRequest3.Request);

			assertShiftTradeRequestStatus(shiftTradeRequest1, ShiftTradeStatus.OkByBothParts);
			assertScheduleSwaped(shiftTradeRequest1);

			assertShiftTradeRequestStatus(shiftTradeRequest2, ShiftTradeStatus.Referred);
			assertShiftTradeRequestStatus(shiftTradeRequest3, ShiftTradeStatus.Referred);
		}

		[Test]
		public void ShouldDenyOtherShiftTradeRequestsWhenShiftExchangeOfferIsCompleted()
		{
			var agent1 = PersonFactory.CreatePersonWithId();
			var agent2 = PersonFactory.CreatePersonWithId();
			var agent3 = PersonFactory.CreatePersonWithId();

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 01, 01, 15, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var shiftDate = new DateOnly(2007, 1, 1);

			var agent1Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent1, activity);
			var shiftTradeOffer =
				new ShiftExchangeOffer(agent1Shift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending)
				{
					Criteria = new ShiftExchangeCriteria(DateOnly.Today.AddDays(1),
						new ScheduleDayFilterCriteria
						{
							DayType = ShiftExchangeLookingForDay.DayOffOrEmptyDay
						}),
					ShiftExchangeOfferId = Guid.NewGuid().ToString()
				}.WithId();
			PersonRequestRepository.Add(new PersonRequest(agent1, shiftTradeOffer).WithId());

			var agent2Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent2, activity);
			var personRequest2 = shiftTradeOffer.MakeShiftTradeRequest(agent2Shift).WithId();
			personRequest2.ForcePending();
			((ShiftTradeRequest) personRequest2.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, null);
			PersonRequestRepository.Add(personRequest2);

			var agent3Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent3, activity);
			var personRequest3 = shiftTradeOffer.MakeShiftTradeRequest(agent3Shift).WithId();
			personRequest3.ForcePending();
			((ShiftTradeRequest) personRequest3.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, null);
			PersonRequestRepository.Add(personRequest3);

			_scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(Scenario.Current()
				, new DateTimePeriod(2007, 01, 01, 7, 2007, 01, 01, 18), CurrentAuthorization, agent1Shift.PersonAssignment(),
				agent2Shift.PersonAssignment(), agent3Shift.PersonAssignment());

			setShiftTradeRequestApprovalService(agent1);
			((ShiftTradeRequestApprovalService) _requestApprovalService).CheckShiftTradeExchangeOffer = true;
			_requestApprovalService.Approve(personRequest3.Request);

			personRequest2.IsDenied.Should().Be(true);
			personRequest2.DenyReason.Should().Be(nameof(Resources.ShiftTradeRequestForExchangeOfferHasBeenCompleted));
			personRequest3.IsPending.Should().Be(true);
		}

		[Test]
		public void ShouldNotDenyOtherShiftTradeRequestsWhenShiftExchangeFailed()
		{
			var agent1 = PersonFactory.CreatePersonWithId();
			var agent2 = PersonFactory.CreatePersonWithId();
			var agent3 = PersonFactory.CreatePersonWithId();

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 01, 01, 15, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("Phone").WithId();
			var shiftDate = new DateOnly(2007, 1, 1);

			var agent1Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent1, activity);
			var shiftTradeOffer =
				new ShiftExchangeOffer(agent1Shift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending)
				{
					Criteria = new ShiftExchangeCriteria(DateOnly.Today.AddDays(1),
						new ScheduleDayFilterCriteria
						{
							DayType = ShiftExchangeLookingForDay.DayOffOrEmptyDay
						}),
					ShiftExchangeOfferId = Guid.NewGuid().ToString()
				}.WithId();
			PersonRequestRepository.Add(new PersonRequest(agent1, shiftTradeOffer).WithId());

			var agent2Shift = createScheduleDay(shiftDate, period.ChangeEndTime(TimeSpan.FromHours(3)), agent2, activity);
			var personRequest2 = shiftTradeOffer.MakeShiftTradeRequest(agent2Shift).WithId();
			personRequest2.ForcePending();
			((ShiftTradeRequest)personRequest2.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, null);
			PersonRequestRepository.Add(personRequest2);

			var nextShiftDay = shiftDate.AddDays(1);
			var agent3Shift = createScheduleDay(nextShiftDay, period.ChangeEndTime(TimeSpan.FromHours(3)), agent3, activity);
			var personRequest3 = shiftTradeOffer.MakeShiftTradeRequest(agent3Shift).WithId();
			personRequest3.ForcePending();
			((ShiftTradeRequest)personRequest3.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, null);
			PersonRequestRepository.Add(personRequest3);

			ScheduleStorage.Add(agent1Shift.PersonAssignment());
			ScheduleStorage.Add(agent2Shift.PersonAssignment());
			ScheduleStorage.Add(agent3Shift.PersonAssignment());

			_scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] {agent1, agent2, agent3},
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(shiftDate, nextShiftDay), Scenario.Current());

			setShiftTradeRequestApprovalService(agent1);
			((ShiftTradeRequestApprovalService)_requestApprovalService).CheckShiftTradeExchangeOffer = true;
			_requestApprovalService.Approve(personRequest3.Request);

			personRequest2.IsPending.Should().Be(true);
			personRequest3.IsPending.Should().Be(true);
		}

		private IScheduleDay createScheduleDay(DateOnly date, DateTimePeriod period, IPerson agent, IActivity activity)
		{
			var scheduleDay = ScheduleDayFactory.Create(date, agent, Scenario.Current());
			scheduleDay.AddMainShift(EditableShiftFactory.CreateEditorShift(activity,
				period,
				ShiftCategoryFactory.CreateShiftCategory("Early")));
			return scheduleDay;
		}

		private void assertShiftTradeRequestStatus(IPersonRequest request, ShiftTradeStatus shiftTradeStatus)
		{
			var requestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequest = request.Request as IShiftTradeRequest;
			shiftTradeRequest.GetShiftTradeStatus(requestStatusChecker).Should().Be(shiftTradeStatus);
		}

		private void assertScheduleSwaped(IPersonRequest request)
		{
			var shiftTradeRequest = request.Request as IShiftTradeRequest;
			var shiftTradeSwapDetails = shiftTradeRequest.ShiftTradeSwapDetails;
			foreach (var shiftTradeSwapDetail in shiftTradeSwapDetails)
			{
				shiftTradeSwapDetail.SchedulePartFrom.PersonAssignment().ShiftLayers.Single().Payload.Id.Should()
					.Be(
						_scheduleDictionary[shiftTradeRequest.PersonTo].ScheduledDay(shiftTradeSwapDetail.DateFrom)
							.PersonAssignment()
							.ShiftLayers.Single()
							.Payload.Id);
				shiftTradeSwapDetail.SchedulePartTo.PersonAssignment().ShiftLayers.Single().Payload.Id.Should()
					.Be(
						_scheduleDictionary[shiftTradeRequest.PersonFrom].ScheduledDay(shiftTradeSwapDetail.DateFrom)
							.PersonAssignment()
							.ShiftLayers.First()
							.Payload.Id);
			}
		}

		private IList<IScheduleData> createScheduleDatas(DateOnlyPeriod dateOnlyPeriod, TimePeriod timePeriod, params IPerson[] persons)
		{
			var scheduleDatas = new List<IScheduleData>();
			foreach (var person in persons)
			{
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var dateOnlys = dateOnlyPeriod.DayCollection();
				foreach (var dateOnly in dateOnlys)
				{
					scheduleDatas.Add(addAssignment(person, dateOnly.ToDateTimePeriod(timePeriod, timeZone)));
				}
			}
			return scheduleDatas;
		}

		private IPersonRequest createPersonShiftTradeRequest(IPerson personFrom, IPerson personTo, DateOnly requestDate)
		{
			var request = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, requestDate);
			var shiftTradeRequest = request.Request as IShiftTradeRequest;
			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, null);
			foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
			{
				shiftTradeSwapDetail.SchedulePartFrom = _scheduleDictionary[personFrom].ScheduledDay(requestDate);
				shiftTradeSwapDetail.SchedulePartTo = _scheduleDictionary[personTo].ScheduledDay(requestDate);
				shiftTradeSwapDetail.ChecksumFrom = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartFrom).CalculateChecksum();
				shiftTradeSwapDetail.ChecksumTo = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartTo).CalculateChecksum();
			}
			return request;
		}
		
		private IPersonAssignment addAssignment(IPerson person, DateTimePeriod dateTimePeriod)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), dateTimePeriod);
			return assignment;
		}

		private void setShiftTradeRequestApprovalService(IPerson person)
		{
			_requestApprovalService =
				RequestApprovalServiceFactory.MakeShiftTradeRequestApprovalService(_scheduleDictionary, person);
		}
	}
}
