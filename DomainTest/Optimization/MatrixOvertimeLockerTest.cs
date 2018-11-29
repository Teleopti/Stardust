using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class MatrixOvertimeLockerTest
    {
        private MatrixOvertimeLocker _matrixOvertimeLocker;
        private MockRepository _mockRepository;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDay _scheduleDay;
        private IScheduleDayPro _scheduleDayPro;
        private IList<IScheduleMatrixPro> _scheduleMatrixPros;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scheduleMatrixPro = _mockRepository.StrictMock<IScheduleMatrixPro>();
            _scheduleDay = _mockRepository.StrictMock<IScheduleDay>();
            _scheduleDayPro = _mockRepository.StrictMock<IScheduleDayPro>();
            _scheduleMatrixPros = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
            _matrixOvertimeLocker = new MatrixOvertimeLocker(_scheduleMatrixPros);
        }

        [Test]
        public void ShouldLockDaysWithOvertime()
        {
            var unlockedDays = new [] {_scheduleDayPro};
            var personAssignment = _mockRepository.StrictMock<IPersonAssignment>(); 
            var dateOnly = new DateOnly(2010,1,1);
           
            using(_mockRepository.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(unlockedDays).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(personAssignment.OvertimeActivities()).Return(new[]{new OvertimeShiftLayer(new Activity("d"), new DateTimePeriod(2010,1,1,2010,1,2), MockRepository.GenerateMock<IMultiplicatorDefinitionSet>())}).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.PersonAssignment()).Return(personAssignment).Repeat.AtLeastOnce();
                Expect.Call(() => _scheduleMatrixPro.LockDay(dateOnly)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day).Return(dateOnly).Repeat.AtLeastOnce();
            }

            using(_mockRepository.Playback())
            {
                _matrixOvertimeLocker.Execute();
            }
        }
    }
}
