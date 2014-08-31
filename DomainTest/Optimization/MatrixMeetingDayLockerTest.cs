using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class MatrixMeetingDayLockerTest
    {
        private MatrixMeetingDayLocker _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IPersonMeeting _personMeeting;
        private IList<IScheduleMatrixPro> _scheduleMatrixList;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
			_scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mockRepository.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mockRepository.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mockRepository.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mockRepository.StrictMock<IScheduleDay>();
			_personMeeting = _mockRepository.StrictMock<IPersonMeeting>();
            _scheduleMatrixList = new List<IScheduleMatrixPro>{ _scheduleMatrix };
            _target = new MatrixMeetingDayLocker(_scheduleMatrixList);
        }

        [Test]
        public void VerifyLockDayWithPersonMeetingTest()
        {
            IList<IScheduleDayPro> unlockedList = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2 };
            ReadOnlyCollection<IScheduleDayPro> unlockedDays = new ReadOnlyCollection<IScheduleDayPro>(unlockedList);
            DateTime lockDate = new DateTime(2000, 01, 01);

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleMatrix.UnlockedDays).Return(unlockedDays).Repeat.AtLeastOnce();

                Expect.Call(_scheduleDayPro1.DaySchedulePart())
                    .Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonMeetingCollection())
                    .Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()))
                    .Repeat.AtLeastOnce();

                Expect.Call(_scheduleDayPro2.DaySchedulePart())
                    .Return(_scheduleDay2);
                Expect.Call(_scheduleDay2.PersonMeetingCollection())
                    .Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>{_personMeeting}))
                    .Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro2.Day)
                    .Return(new DateOnly(lockDate))
                    .Repeat.Times(2);
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(new DateOnly(lockDate), new DateOnly(lockDate)));
            }
            using (_mockRepository.Playback())
            {
                _target.Execute();
            }
        }

    }
}
