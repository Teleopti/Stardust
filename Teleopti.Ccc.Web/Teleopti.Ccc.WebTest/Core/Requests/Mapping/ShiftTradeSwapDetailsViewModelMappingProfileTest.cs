using System;
using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeSwapDetailsViewModelMappingProfileTest
	{
		private DateTime _dateFrom;
		private DateTime _dateTo;
		private StubFactory _scheduleFactory;
		private IShiftTradeTimeLineHoursViewModelFactory _timeLineFactory;
		private IProjectionProvider _projectionProvider;
		private Person _fromPerson;
		private Person _toPerson;

		[SetUp]
		public void Setup()
		{
			_fromPerson = new Person() { Name = new Name("From", "Person"), };
			_toPerson = new Person() { Name = new Name("To", "Person") };
			_scheduleFactory = new StubFactory();
			_timeLineFactory = MockRepository.GenerateStub<IShiftTradeTimeLineHoursViewModelFactory>();
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_dateFrom = new DateTime(2001, 12, 12, 0, 0, 0, DateTimeKind.Utc);
			_dateTo = new DateTime(2001, 12, 13, 0, 0, 0, DateTimeKind.Utc);
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailsViewModelMappingProfile(_timeLineFactory, _projectionProvider)));
		}

		[Test]
		public void CreateScheduleViewModelsFromMapper()
		{
			var profileForProbing = new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>();

			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();

			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();

			var shiftTradePersonScheduleViewModelStub = new ShiftTradePersonScheduleViewModel();
			profileForProbing.Result = shiftTradePersonScheduleViewModelStub;

			Mapper.AddProfile(profileForProbing);

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest);
			Assert.That(profileForProbing.HasBeenMappedFrom(scheduleDayFrom), "The mapper should have been called for the sheduleday From");
			Assert.That(profileForProbing.HasBeenMappedFrom(scheduleDayTo), "The mapper should have been called for the sheduleday To");
			Assert.That(result.To, Is.EqualTo(shiftTradePersonScheduleViewModelStub), "Should have been set from the mapper (we are using the same result for To and For");
			Assert.That(result.To, Is.EqualTo(shiftTradePersonScheduleViewModelStub), "Should have been set from the mapper (we are using the same result for To and For");
		}

		[Test]
		public void CreateTimelineWhenOneNightShift()
		{
			Mapper.AddProfile(new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>());
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(23);
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub(new DateTimePeriod(expectedStart.AddHours(2), expectedEnd));
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedStart.AddHours(1))));
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart.AddHours(2), expectedEnd)));

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest);

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenToScheduleDayIsEmpty()
		{
			Mapper.AddProfile(new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>());
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(10);
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub(new DateTimePeriod(expectedStart, expectedEnd));
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedEnd)));
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub());

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest);

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenFromScheduleDayIsEmpty()
		{
			Mapper.AddProfile(new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>());
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(10);
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedEnd)));

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest);

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenBothScheduleDayIsEmpty()
		{
			Mapper.AddProfile(new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>());
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub());

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest);

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(
				new DateTimePeriod(shiftTradeRequest.Period.StartDateTime.AddHours(-1), shiftTradeRequest.Period.EndDateTime.AddHours(1))));
		}

		[Test]
		public void CreateTimelineBasedOnShiftTradePeriodIfNoSchedulesExists()
		{
			var timeLineHoursViewModelFactory = MockRepository.GenerateStrictMock<IShiftTradeTimeLineHoursViewModelFactory>();

			Mapper.Reset();

			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailsViewModelMappingProfile(timeLineHoursViewModelFactory, _projectionProvider)));
			AddNeededMappingProfiles();

			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			
			var shiftTrade = CreateShiftTrade(from, to, null,null);
			var expectedTimelinePeriod = shiftTrade.Period;

			var timelineHours = new List<ShiftTradeTimeLineHoursViewModel>() { new ShiftTradeTimeLineHoursViewModel(), new ShiftTradeTimeLineHoursViewModel() };
			timeLineHoursViewModelFactory.Expect(s => s.CreateTimeLineHours(expectedTimelinePeriod)).Return(timelineHours);

			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTrade);

			timeLineHoursViewModelFactory.VerifyAllExpectations();

			Assert.That(result.TimeLineHours, Is.EqualTo(timelineHours));
			Assert.That(result.TimeLineStartDateTime, Is.EqualTo(expectedTimelinePeriod.StartDateTime));
		}

		[Test]
		public void CreateEmptyScheduleViewModelsIfNoSchedulesExists()
		{
			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			var shiftTrade = CreateShiftTrade(from, to, null, null);

			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTrade);

			Assert.That(result.To, Is.Not.Null);
			Assert.That(result.From, Is.Not.Null);
		}

		[Test]
		public void Name_WhenNoScheduleExists_ShouldBeSet()
		{
			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			var shiftTrade = CreateShiftTrade(from, to, null, null);

			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTrade);

			result.PersonFrom
				.Should().Be.EqualTo(_fromPerson.Name.ToString());
			result.PersonTo
				.Should().Be.EqualTo(_toPerson.Name.ToString());
		}

		private void AddNeededMappingProfiles()
		{
			var profileStubForHandlingScheduleDays = new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>();
			profileStubForHandlingScheduleDays.Result = new ShiftTradePersonScheduleViewModel();
			Mapper.AddProfile(profileStubForHandlingScheduleDays);
		}

		private  IShiftTradeRequest CreateShiftTrade(DateTime dateFrom, DateTime dateTo, IScheduleDay scheduleDayFrom, IScheduleDay scheduleDayTo)
		{
			var details = new List<IShiftTradeSwapDetail>();

			var detail = new ShiftTradeSwapDetail(_fromPerson, _toPerson, new DateOnly(dateFrom), new DateOnly(dateTo))
									 {
										 SchedulePartFrom = scheduleDayFrom,
										 SchedulePartTo = scheduleDayTo
									 };
			details.Add(detail);

			var shiftTrade = new ShiftTradeRequest(details);

			return shiftTrade;
		}
	}
}