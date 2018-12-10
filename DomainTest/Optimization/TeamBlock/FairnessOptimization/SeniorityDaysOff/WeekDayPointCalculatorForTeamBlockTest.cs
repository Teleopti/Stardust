using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class WeekDayPointCalculatorForTeamBlockTest
    {
        private MockRepository _mocks;
        private IWeekDayPointCalculatorForTeamBlock _target;
        private IWeekDayPoints _weekDayPoints;
        private ITeamBlockInfo _teamBlockInfo1;
        private IScheduleMatrixPro _matrix1;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
	    private ISeniorityWorkDayRanks _seniorityWorkDayRanks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new WeekDayPointCalculatorForTeamBlock();
            _teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo  >();
            _weekDayPoints = new WeekDayPoints();
            _matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_seniorityWorkDayRanks = new SeniorityWorkDayRanks();
        }

        [Test]
        public void ShouldReturnNullIfListIsEmpty()
        {
            var matrxiList = new List<IScheduleMatrixPro> {_matrix1};
            var scheduleDayList = new [] {_scheduleDayPro1,_scheduleDayPro2};
            using (_mocks.Record())
            {
                Expect.Call(_teamBlockInfo1.MatrixesForGroupAndBlock()).Return(matrxiList);
                Expect.Call(_matrix1.EffectivePeriodDays).Return(scheduleDayList);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
                Expect.Call(_scheduleDayPro1.Day).Return(new DateOnly(2014, 1, 26));
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mocks.Playback())
            {
				var result = _target.CalculateDaysOffSeniorityValue(_teamBlockInfo1, _weekDayPoints.GetWeekDaysPoints(_seniorityWorkDayRanks));
                Assert.AreEqual( result,7);
            }
        }
    }
}
