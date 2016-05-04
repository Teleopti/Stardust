using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
	[TestFixture]
	public class RequestDetailsShiftTradePresenterTest
	{
		private MockRepository _mocks;
		private IRangeProjectionService _rangeProjectionService;
		private IPersonRequestViewModel _model;
		private RequestDetailsShiftTradePresenter _target;
		private IScheduleDictionary _schedules;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_rangeProjectionService = _mocks.StrictMock<IRangeProjectionService>();
			_model = _mocks.StrictMock<IPersonRequestViewModel>();
			_schedules = CreateSchedules();
			_target = new RequestDetailsShiftTradePresenter(_model, _schedules, _rangeProjectionService);
		}

		[Test]
		public void ShouldInitialize()
		{
			var request = CreateShiftTradeRequestObject(2);
			var dateTimePeriod = DateTimeFactory.CreateDateTimePeriod(DateTime.UtcNow, 2);
			var visualLayers = new List<IVisualLayer>();
			using (_mocks.Record())
			{
				Expect.Call(_model.PersonRequest).Return(request).Repeat.Any();
				Expect.Call(_target.GetVisualLayersForPerson(new ContactPersonViewModel(PersonFactory.CreatePerson()), dateTimePeriod)).Return(visualLayers).Repeat.Any();
				Expect.Call(_rangeProjectionService.CreateProjection(null, dateTimePeriod)).IgnoreArguments().Return(visualLayers).
					Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
			}
		}
		
		[Test]
		public void ShouldGetParticipantList()
		{
			const int requestedDays = 3;
			var request = CreateShiftTradeRequestObject(requestedDays);
			var dateTimePeriod = DateTimeFactory.CreateDateTimePeriod(DateTime.UtcNow, 2);
			var visualLayers = new List<IVisualLayer>();
			using (_mocks.Record())
			{
				Expect.Call(_model.PersonRequest).Return(request).Repeat.Any();
				Expect.Call(_target.GetVisualLayersForPerson(new ContactPersonViewModel(PersonFactory.CreatePerson()), dateTimePeriod)).Return(visualLayers).Repeat.Any();
				Expect.Call(_rangeProjectionService.CreateProjection(null, dateTimePeriod)).IgnoreArguments().Return(visualLayers).Repeat.Any();

			}
			using (_mocks.Playback())
			{
				_target.Initialize();
				Assert.AreEqual(requestedDays * 2, _target.ParticipantList.Count);
				Assert.AreEqual(requestedDays * 2, _target.RowCount);
			}
		}

        [Test]
        public void ShouldHandleMultipleLayers()
        {
        	var person = PersonFactory.CreatePerson();
            const int requestedDays = 3;
            var request = CreateShiftTradeRequestObject(requestedDays);
            var dateTimePeriod = DateTimeFactory.CreateDateTimePeriod(DateTime.UtcNow, 2).ChangeEndTime(TimeSpan.FromSeconds(1));
            var visualLayerFactory = new VisualLayerFactory();
            var activity = ActivityFactory.CreateActivity("test");
            var visualLayers = new List<IVisualLayer>
                                   {
                                       visualLayerFactory.CreateShiftSetupLayer(activity,
                                           new DateTimePeriod(dateTimePeriod.StartDateTime,
                                                              dateTimePeriod.StartDateTime.AddHours(1)),person),
                                                              visualLayerFactory.CreateShiftSetupLayer(
                                           activity,
                                           new DateTimePeriod(dateTimePeriod.StartDateTime.AddHours(1),
                                                              dateTimePeriod.StartDateTime.AddHours(2)),person)
                                   };
            using (_mocks.Record())
            {
                Expect.Call(_model.PersonRequest).Return(request).Repeat.Any();
                Expect.Call(_target.GetVisualLayersForPerson(new ContactPersonViewModel(PersonFactory.CreatePerson()), dateTimePeriod)).Return(visualLayers).Repeat.Any();
                Expect.Call(_rangeProjectionService.CreateProjection(null, dateTimePeriod)).IgnoreArguments().Return(visualLayers).Repeat.Any();

            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                Assert.AreEqual(requestedDays * 2, _target.ParticipantList.Count);
                Assert.AreEqual(requestedDays * 2, _target.RowCount);
            }
        }

		[Test]
		public void ShouldDecideIsDayOff()
		{
			var tradingPerson = PersonFactory.CreatePerson("First", "Last");
			var dateOnly = new DateOnly(2015, 1, 1);
			var model = new ContactPersonViewModel(tradingPerson);
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_target = new RequestDetailsShiftTradePresenter(null, schedules, null);

			using (_mocks.Record())
			{
				Expect.Call(schedules[tradingPerson]).Return(scheduleRange);
				Expect.Call(scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.DayOff);
			}

			using (_mocks.Playback())
			{
				var isDayOff = _target.IsDayOff(model, dateOnly);
				Assert.IsTrue(isDayOff);
			}	
		}

		[Test]
		public void ShouldDecideIsNotDayOff()
		{
			var tradingPerson = PersonFactory.CreatePerson("First", "Last");
			var dateOnly = new DateOnly(2015, 1, 1);
			var model = new ContactPersonViewModel(tradingPerson);
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_target = new RequestDetailsShiftTradePresenter(null, schedules, null);

			using (_mocks.Record())
			{
				Expect.Call(schedules[tradingPerson]).Return(scheduleRange);
				Expect.Call(scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
				Expect.Call(scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
			}

			using (_mocks.Playback())
			{
				var isDayOff = _target.IsDayOff(model, dateOnly);
				Assert.IsFalse(isDayOff);
			}
		}

		private static ScheduleDictionary CreateSchedules()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var startDateTime = DateTime.UtcNow.Date.AddDays(7);
			var endDateTime = startDateTime.AddHours(2);
			var dateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var scheduleDateTimePeriod = new ScheduleDateTimePeriod(dateTimePeriod);
			return new ScheduleDictionary(scenario, scheduleDateTimePeriod);
		}

		private static IPersonRequest CreateShiftTradeRequestObject(int reqeustedDays)
		{
			var tradingPerson = PersonFactory.CreatePerson("First", "Last");
			var tradeWithPerson = PersonFactory.CreatePerson("First1", "Last1");
			var period = new List<DateOnly>();
			for (var i = 0; i < reqeustedDays; i++)
				period.Add(new DateOnly(DateTime.Now.AddDays(i)));
			IList<IShiftTradeSwapDetail> shiftTradeSwapDetails = period.Select(dateOnly => new ShiftTradeSwapDetail(tradingPerson, tradeWithPerson, dateOnly, dateOnly)).Cast<IShiftTradeSwapDetail>().ToList();
			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetails);
			var request = new PersonRequest(tradingPerson, shiftTradeRequest);
			request.Pending();
			return request;
		}
	}
}
