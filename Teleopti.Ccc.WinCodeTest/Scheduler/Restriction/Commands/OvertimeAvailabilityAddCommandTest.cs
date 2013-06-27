using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling.Restriction.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction.Commands
{
    [TestFixture]
    public class OvertimeAvailabilityAddCommandTest
    {
        private IOvertimeAvailabilityAddCommand _target;
        private MockRepository _mock;
        private IScheduleDay _scheduleDay;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private IOvertimeAvailability _overtimeAvailability ;
        private IOvertimeAvailabilityCreator  _overtimeAvailabilityCreator ;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _overtimeAvailabilityCreator = _mock.StrictMock<IOvertimeAvailabilityCreator>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _startTime = TimeSpan.FromHours(8);
            _endTime = TimeSpan.FromHours(10);
            _target = new OvertimeAvailabilityAddCommand(_scheduleDay, _startTime, _endTime, _overtimeAvailabilityCreator);
            _overtimeAvailability  = _mock.StrictMock<IOvertimeAvailability >();
        }

        [Test]
        public void ShouldAdd()
        {

            using (_mock.Record())
            {
                bool startTimeError;
                bool endTimeError;
                Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
                Expect.Call(_overtimeAvailabilityCreator .CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)).Return(true);
                Expect.Call(_overtimeAvailabilityCreator.Create(_scheduleDay, _startTime, _endTime)).Return(_overtimeAvailability );
                Expect.Call(() => _scheduleDay.Add(_overtimeAvailability ));
            }

            using (_mock.Playback())
            {
                _target.Execute();
            }
        }

        [Test]
        public void ShouldNotAddWhenCannotCreateDay()
        {
            using (_mock.Record())
            {
                bool startTimeError;
                bool endTimeError;
                Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
                Expect.Call(_overtimeAvailabilityCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)).Return(false);
            }

            using (_mock.Playback())
            {
                _target.Execute();
            }
        }

        [Test]
        public void ShouldNotAddWhenDayExists()
        {
            using (_mock.Record())
            {
                
                Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailability  }));
            }

            using (_mock.Playback())
            {
                _target.Execute();
            }
        }
    }

    
}
