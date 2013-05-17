using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockScheduledDayCheckerTest
    {
        private MockRepository _mocks;
        private IGroupPerson _groupPerson;
        private IScheduleMatrixPro _matrix1;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleRange _scheduleRange;
        private IScheduleMatrixPro _matrix2;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _groupPerson = _mocks.StrictMock<IGroupPerson>();
            _matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
        }

        [Test]
        public void ShouldReturnTrueIfDateIsScheduled()
        {
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            var dateOnly = new DateOnly(2013, 04, 10);
            
            var matrixes = new List<IScheduleMatrixPro> { _matrix1 };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var teaminfo = new TeamInfo(_groupPerson, groupMatrixList);
            IBlockInfo blockInfo = new BlockInfo(period);
            ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

           using (_mocks.Record())
           {
               var scheduleDay = commonMocks(dateOnly);
               Expect.Call(scheduleDay.IsScheduled()).Return(true);
           }

           Assert.IsTrue(TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(teamBlockInfo, dateOnly));
        }

        [Test]
        public void ShouldReturnFalseIfDateIsNotScheduled()
        {
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            var dateOnly = new DateOnly(2013, 04, 10);

            var matrixes = new List<IScheduleMatrixPro> { _matrix1 };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
            var teaminfo = new TeamInfo(_groupPerson, groupMatrixList);
            IBlockInfo blockInfo = new BlockInfo(period);
            ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

            using (_mocks.Record())
            {
                var scheduleDay = commonMocks(dateOnly);
                Expect.Call(scheduleDay.IsScheduled()).Return(false );
            }

            Assert.IsFalse(TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(teamBlockInfo, dateOnly));
        }

        private IScheduleDay commonMocks(DateOnly dateOnly)
        {
            IPerson person = PersonFactory.CreatePerson("test");
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            Expect.Call(_matrix1.SchedulingStateHolder).Return(_schedulingResultStateHolder);
            Expect.Call(_schedulingResultStateHolder.Schedules[person]).Return(_scheduleRange);
            Expect.Call(_scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
            Expect.Call(_matrix1.Person).Return(person);
            return scheduleDay;
        }

        [Test]
        public void ShouldReturnFalseIfRangeIsNull()
        {
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            var dateOnly = new DateOnly(2013, 04, 10);

            var matrixes = new List<IScheduleMatrixPro> { _matrix1 };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
            var teaminfo = new TeamInfo(_groupPerson, groupMatrixList);
            IBlockInfo blockInfo = new BlockInfo(period);
            ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

            using (_mocks.Record())
            {
                IPerson person = PersonFactory.CreatePerson("test");
                Expect.Call(_matrix1.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules[person]).Return(null);
                Expect.Call(_matrix1.Person).Return(person);

            }

            Assert.IsFalse(TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(teamBlockInfo, dateOnly));
        }

        [Test]
        public void ShouldReturnFalseIfFirstDayIsScheduled()
        {
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            var dateOnly = new DateOnly(2013, 04, 10);

            var matrixes = new List<IScheduleMatrixPro> { _matrix1,_matrix2  };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
            var teaminfo = new TeamInfo(_groupPerson, groupMatrixList);
            IBlockInfo blockInfo = new BlockInfo(period);
            ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

            using (_mocks.Record())
            {
                IPerson person = PersonFactory.CreatePerson("test");
                var scheduleDay = _mocks.StrictMock<IScheduleDay>();
                Expect.Call(_matrix1.SchedulingStateHolder).Return(_schedulingResultStateHolder).Repeat.AtLeastOnce();
                Expect.Call(_schedulingResultStateHolder.Schedules[person]).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay).Repeat.AtLeastOnce() ;
                Expect.Call(_matrix1.Person).Return(person);

                Expect.Call(scheduleDay.IsScheduled()).Return(true);
                var schedulingResultStateHolder2= _mocks.StrictMock<ISchedulingResultStateHolder>();
                Expect.Call(_matrix2.SchedulingStateHolder).Return(schedulingResultStateHolder2);
                Expect.Call(schedulingResultStateHolder2.Schedules[person]).Return(_scheduleRange);
                Expect.Call(_matrix2.Person).Return(person);
                
                Expect.Call(scheduleDay.IsScheduled()).Return(false);
                
            }

            Assert.IsFalse(TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(teamBlockInfo, dateOnly));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldReturnFalseIfTeamBlockIsNull()
        {
            var dateOnly = new DateOnly(2013, 04, 10);
            Assert.IsFalse(TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(null, dateOnly));
        }
    }
}
