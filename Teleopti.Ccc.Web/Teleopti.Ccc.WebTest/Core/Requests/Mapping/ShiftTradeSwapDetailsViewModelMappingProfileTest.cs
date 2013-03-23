
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
using Teleopti.Ccc.WebTest.Core.Mapping;
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
		private Person _fromPerson;
		private Person _toPerson;

		[SetUp]
		public void Setup()
		{
			_fromPerson = new Person() { Name = new Name("From", "Person"), };
			_toPerson = new Person() { Name = new Name("To", "Person") };
			_scheduleFactory = new StubFactory();
			_timeLineFactory = MockRepository.GenerateStub<IShiftTradeTimeLineHoursViewModelFactory>();
			_dateFrom = new DateTime(2001, 12, 12, 0, 0, 0, DateTimeKind.Utc);
			_dateTo = new DateTime(2001, 12, 13, 0, 0, 0, DateTimeKind.Utc);
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailsViewModelMappingProfile(Depend.On(_timeLineFactory))));
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
		public void CreateTimelineBasedOnShiftTRadePeriodIfNoSchedulesExists()
		{
			var timeLineHoursViewModelFactory = MockRepository.GenerateStrictMock<IShiftTradeTimeLineHoursViewModelFactory>();

			Mapper.Reset();

			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailsViewModelMappingProfile(Depend.On(timeLineHoursViewModelFactory))));
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