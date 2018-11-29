using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands;



namespace Teleopti.Ccc.WinCodeTest.Scheduler.GridlockCommands
{
    [TestFixture]
    public class WriteProtectionRemoveCommandTest
    {
        private WriteProtectionRemoveCommand _target;
        private MockRepository _mocks;
        private IPerson _person;
        private IScheduleDay _scheduleDay;
        private IList<IScheduleDay> _schedules;
        private IPersonWriteProtectionInfo _personWriteProtectionInfo;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
        private ICollection<IPersonWriteProtectionInfo> _personWriteProtectionInfos;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _person = _mocks.StrictMock<IPerson>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _schedules = new List<IScheduleDay>{_scheduleDay};
            _personWriteProtectionInfo = _mocks.StrictMock<IPersonWriteProtectionInfo>();
            _dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            _personWriteProtectionInfos = _mocks.StrictMock<ICollection<IPersonWriteProtectionInfo>>();
            _target = new WriteProtectionRemoveCommand(_schedules, _personWriteProtectionInfos);
        }

        [Test]
        public void ShouldRemoveWriteProtectionWhenScheduleDayDateIsLessThanProtectedDate()
        {
            var dateOnly = new DateOnly(2012,1,1);
            
            using(_mocks.Record())
            {
                Expect.Call(_scheduleDay.Person).Return(_person);
                Expect.Call(_person.PersonWriteProtection).Return(_personWriteProtectionInfo).Repeat.AtLeastOnce();
                Expect.Call(_personWriteProtectionInfo.PersonWriteProtectedDate).Return(dateOnly);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(dateOnly);
                Expect.Call(() => _personWriteProtectionInfo.PersonWriteProtectedDate = null);
                Expect.Call(() => _personWriteProtectionInfos.Add(_personWriteProtectionInfo));
            }

            using(_mocks.Playback())
            {
                _target.Execute();
            }
        }

        [Test]
        public void ShouldNotRemoveWriteProtectionWhenScheduleDayDateIsGreaterThanProtectedDate()
        {
            var dateOnly = new DateOnly(2012, 1, 1);
            var protectedDateOnly = dateOnly.AddDays(-1);

            using (_mocks.Record())
            {
                Expect.Call(_scheduleDay.Person).Return(_person);
                Expect.Call(_person.PersonWriteProtection).Return(_personWriteProtectionInfo);
                Expect.Call(_personWriteProtectionInfo.PersonWriteProtectedDate).Return(protectedDateOnly);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(dateOnly);
            }

            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }
    }
}
