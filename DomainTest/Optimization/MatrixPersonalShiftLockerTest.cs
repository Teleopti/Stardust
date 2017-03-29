using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class MatrixPersonalShiftLockerTest
    {
        private MatrixPersonalShiftLocker _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IPersonAssignment _personAssignment;
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
			_personAssignment = _mockRepository.StrictMock<IPersonAssignment>();
            _scheduleMatrixList = new List<IScheduleMatrixPro>{ _scheduleMatrix };
            _target = new MatrixPersonalShiftLocker(_scheduleMatrixList);
        }

        [Test]
        public void VerifyLockDayWithPersonalShiftTest()
        {
            var unlockedDays = new []{ _scheduleDayPro1, _scheduleDayPro2 };
            DateTime lockDate = new DateTime(2000, 01, 01);

            using(_mockRepository.Record())
            {
                Expect.Call(_scheduleMatrix.UnlockedDays).Return(unlockedDays).Repeat.AtLeastOnce();

                Expect.Call(_scheduleDayPro1.DaySchedulePart())
                    .Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
	            Expect.Call(_personAssignment.PersonalActivities()).Return(new List<IPersonalShiftLayer>());

                Expect.Call(_scheduleDayPro2.DaySchedulePart())
                    .Return(_scheduleDay2);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
								Expect.Call(_personAssignment.PersonalActivities()).Return(new List<IPersonalShiftLayer> {MockRepository.GenerateMock<IPersonalShiftLayer>()});
                Expect.Call(_scheduleDayPro2.Day)
                    .Return(new DateOnly(lockDate))
                    .Repeat.Times(2);
                _scheduleMatrix.LockDay(new DateOnly(lockDate));
            }
            using (_mockRepository.Playback())
            {
                _target.Execute();
            }
        }

        [Test]
        public void VerifyLockDayWithNullPersonAssignment()
        {
            var unlockedDays = new []{ _scheduleDayPro1 };

            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleMatrix.UnlockedDays).Return(unlockedDays).Repeat.AtLeastOnce();

                Expect.Call(_scheduleDayPro1.DaySchedulePart())
                    .Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.PersonAssignment())
                    .Return(null).Repeat.AtLeastOnce();
            }
            using (_mockRepository.Playback())
            {
                _target.Execute();
            }
        }
    }
}
