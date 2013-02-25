
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeSwapDetailsViewModelMappingProfileTest
	{
		private DateOnly _dateFrom;
		private DateOnly _dateTo;
		private IShiftTradeTimeLineHoursViewModelFactory _timeLineFactory;

		[SetUp]
		public void Setup()
		{
			_timeLineFactory = MockRepository.GenerateStub<IShiftTradeTimeLineHoursViewModelFactory>();
			_dateFrom = new DateOnly(2001, 12, 12);
			_dateTo = new DateOnly(2002, 12, 12);
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailsViewModelMappingProfile(Depend.On(_timeLineFactory))));
		}


		[Test]
		public void CreateScheduleViewModelsFromMapper()
		{
			var profileForProbing = new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>();
			var scheduleDayFrom = MockRepository.GenerateStub<IScheduleDay>();
			var scheduleDayTo = MockRepository.GenerateStub<IScheduleDay>();
			var shiftTradePersonScheduleViewModelStub = new ShiftTradePersonScheduleViewModel();
			profileForProbing.Result = shiftTradePersonScheduleViewModelStub;
			
			Mapper.AddProfile(profileForProbing);

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom,scheduleDayTo);
			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest);
			Assert.That(profileForProbing.HasBeenMappedFrom(scheduleDayFrom),"The mapper should have been called for the sheduleday From");
			Assert.That(profileForProbing.HasBeenMappedFrom(scheduleDayTo), "The mapper should have been called for the sheduleday To");
			Assert.That(result.To, Is.EqualTo(shiftTradePersonScheduleViewModelStub),"Should have been set from the mapper (we are using the same result for To and For");
			Assert.That(result.To, Is.EqualTo(shiftTradePersonScheduleViewModelStub),"Should have been set from the mapper (we are using the same result for To and For");
		}

		[Test]
		public void CreateTimelineBasedOnTheLargestPeriodFromTheSchedules()
		{
			var timeLineHoursViewModelFactory = MockRepository.GenerateStrictMock<IShiftTradeTimeLineHoursViewModelFactory>(); 

			Mapper.Reset();
			
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailsViewModelMappingProfile(Depend.On(timeLineHoursViewModelFactory))));
			AddNeededMappingProfiles();

			var fromStart = new DateTime(2001, 1, 1, 8, 0, 0,DateTimeKind.Utc);
			var fromEnd = new DateTime(2001, 1, 1, 18, 0, 0,DateTimeKind.Utc);
			var toStart = new DateTime(2001, 1, 1, 12, 0, 0,DateTimeKind.Utc);
			var toEnd = new DateTime(2001, 1, 1, 22, 0, 0,DateTimeKind.Utc);

			IScheduleDay from = CreatScheduleDayWithPeriod(fromStart, fromEnd);
			IScheduleDay to = CreatScheduleDayWithPeriod(toStart, toEnd);
			var shiftTrade = CreateShiftTrade(new DateOnly(fromStart), new DateOnly(fromEnd), from, to);

			var expectedTimelinePeriod = new DateTimePeriod(fromStart, toEnd);
			var timelineHours = new List<ShiftTradeTimeLineHoursViewModel>() {new ShiftTradeTimeLineHoursViewModel(),new ShiftTradeTimeLineHoursViewModel()};
			timeLineHoursViewModelFactory.Expect(s => s.CreateTimeLineHours(expectedTimelinePeriod)).Return(timelineHours);

			var result = Mapper.Map<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>(shiftTrade);

			timeLineHoursViewModelFactory.VerifyAllExpectations();

			Assert.That(result.TimeLineHours, Is.EqualTo(timelineHours));
			Assert.That(result.TimeLineStartDateTime, Is.EqualTo(expectedTimelinePeriod.StartDateTime));

		}


		private void AddNeededMappingProfiles()
		{
			var profileStubForHandlingScheduleDays =  new MappingProfileForProbing<IScheduleDay, ShiftTradePersonScheduleViewModel>(); 
			profileStubForHandlingScheduleDays.Result = new ShiftTradePersonScheduleViewModel();
			Mapper.AddProfile(profileStubForHandlingScheduleDays);
		}

		private static IScheduleDay CreatScheduleDayWithPeriod(DateTime start, DateTime end)
		{
			var period = new DateTimePeriod(start, end);
			var scheduleDay = MockRepository.GenerateStub<IScheduleDay>();
			scheduleDay.Expect(s => s.Period).Return(period).Repeat.Any();
			return scheduleDay;
		}

		private static IShiftTradeRequest CreateShiftTrade(DateOnly dateFrom, DateOnly dateTo, IScheduleDay scheduleDayFrom = null, IScheduleDay scheduleDayTo = null)
		{
			var shiftTrade = MockRepository.GenerateMock<IShiftTradeRequest>();
			var swapDetail = MockRepository.GenerateMock<IShiftTradeSwapDetail>();
			var swapDetails = new ReadOnlyCollection<IShiftTradeSwapDetail>(new List<IShiftTradeSwapDetail>() { swapDetail });
			
			swapDetail.Expect(s => s.DateFrom).Return(dateFrom).Repeat.Any();
			swapDetail.Expect(s => s.DateTo).Return(dateTo).Repeat.Any();
			swapDetail.Expect(s => s.SchedulePartFrom).Return(scheduleDayFrom).Repeat.Any();
			swapDetail.Expect(s => s.SchedulePartTo).Return(scheduleDayTo).Repeat.Any();
			shiftTrade.Expect(s => s.ShiftTradeSwapDetails).Return(swapDetails).Repeat.Any();
			return shiftTrade;
		}
	}
}