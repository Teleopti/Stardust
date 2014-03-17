using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
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

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();

            _person = _mock.StrictMock<IPerson>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _today = new DateOnly(2014,03,05);
            _adjustOvertimeLengthBasedOnAvailability = new AdjustOvertimeLengthBasedOnAvailability();
            _target = new AnalyzePersonAccordingToAvailability(_adjustOvertimeLengthBasedOnAvailability);
        }

        [Test]
        public void NoOvertimeIfThereIsNoAvailability()
        {
            var shiftEndTime = new DateTime(2014,03,05,15,30,0,DateTimeKind.Utc );
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.OvertimeAvailablityCollection())
                      .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>()));
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(TimeSpan.Zero, _target.AdustOvertimeAvailability(_scheduleDay, _today, TimeZoneInfo.Utc, TimeSpan.FromHours(2), shiftEndTime));
            }
            
        }

        [Test]
        public void ShouldReturnAdjustedOvertime()
        {
            var shiftEndTime = new DateTime(2014, 03, 05, 15, 30, 0, DateTimeKind.Utc);
            IOvertimeAvailability overtimeAvailability = new OvertimeAvailability(_person,_today,TimeSpan.FromHours(11),TimeSpan.FromHours(12));
            using (_mock.Record())
            {
                
                Expect.Call(_scheduleDay.OvertimeAvailablityCollection())
                      .Return(new ReadOnlyCollection<IOvertimeAvailability>(new List<IOvertimeAvailability>(){overtimeAvailability }));
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(TimeSpan.Zero, _target.AdustOvertimeAvailability(_scheduleDay, _today, TimeZoneInfo.Utc, TimeSpan.FromHours(2), shiftEndTime));
            }

        }
    }
}
