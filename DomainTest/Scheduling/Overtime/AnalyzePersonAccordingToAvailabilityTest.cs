using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
    [TestFixture]
    public class AnalyzePersonAccordingToAvailabilityTest
    {
        private AdjustOvertimeLengthBasedOnAvailability _adjustOvertimeLengthBasedOnAvailability;
        private IAnalyzePersonAccordingToAvailability _target;
        private IScheduleDay _scheduleDay;
        private DateOnly _today;
        private MockRepository _mock;
        private IPerson _person;
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;
		private DateTimePeriod _dateTimePeriod;
		private DateTime _shiftEndingTime;
		private DateTime _shiftStartTime;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();

            _person = _mock.StrictMock<IPerson>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _today = new DateOnly(2014,03,05);
            _adjustOvertimeLengthBasedOnAvailability = new AdjustOvertimeLengthBasedOnAvailability();
            _target = new AnalyzePersonAccordingToAvailability(_adjustOvertimeLengthBasedOnAvailability);

			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
			_shiftStartTime = new DateTime(2014, 03, 05, 14, 30, 0, DateTimeKind.Utc);
			_shiftEndingTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_shiftStartTime, _shiftEndingTime);
        }

		[Test]
		public void NoOvertimeIfThereIsNoAvailability()
		{
			var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
			var overtimePeriod = new DateTimePeriod(shiftEndTime, shiftEndTime.Add(TimeSpan.FromHours(2)));
			
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.OvertimeAvailablityCollection()).Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>()));
			}
			using (_mock.Playback())
			{
				Assert.IsNull(_target.AdjustOvertimeAvailability(_scheduleDay, _today, TimeZoneInfo.Utc, overtimePeriod));
			}

		}

		[Test]
		public void ShouldReturnAdjustedOvertime()
		{
			var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
			IOvertimeAvailability overtimeAvailability = new OvertimeAvailability(_person, _today, TimeSpan.FromHours(11), TimeSpan.FromHours(12));

			var overtimePeriod = new DateTimePeriod(shiftEndTime, shiftEndTime.Add(TimeSpan.FromHours(2)));
	

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.OvertimeAvailablityCollection()).Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>() { overtimeAvailability }));
				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(_dateTimePeriod);	
			}
			using (_mock.Playback())
			{
				Assert.IsNull(_target.AdjustOvertimeAvailability(_scheduleDay, _today, TimeZoneInfo.Utc, overtimePeriod));
			}

		}
    }
}
