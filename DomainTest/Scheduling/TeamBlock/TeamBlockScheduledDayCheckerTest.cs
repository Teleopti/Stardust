using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockScheduledDayCheckerTest
    {
        private MockRepository _mocks;
        private Group _group;
        private IScheduleMatrixPro _matrix1;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleRange _scheduleRange;
        private IScheduleMatrixPro _matrix2;
	    private IVirtualSchedulePeriod _schedulePeriod;
	    private DateOnly _dateOnly;
	    private DateOnlyPeriod _dateOnlyPeriod;
	    private ITeamBlockSchedulingCompletionChecker _target;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _group = new Group();
            _matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_dateOnly = new DateOnly(2013, 04, 10);
		    _dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
		    _target = new TeamBlockSchedulingCompletionChecker();
        }

        [Test]
        public void ShouldReturnTrueIfDateIsScheduled()
        {
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            
            var matrixes = new List<IScheduleMatrixPro> { _matrix1 };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var teaminfo = new TeamInfo(_group, groupMatrixList);
            IBlockInfo blockInfo = new BlockInfo(period);
            ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

           using (_mocks.Record())
           {
               var scheduleDay = commonMocks(_dateOnly);
               Expect.Call(scheduleDay.IsScheduled()).Return(true);
           }

		   Assert.IsTrue(_target.IsDayScheduledInTeamBlock(teamBlockInfo, _dateOnly));
        }

        [Test]
        public void ShouldReturnFalseIfDateIsScheduledForNonSelectedTeamMember()
        {
            IPerson person1 = PersonFactory.CreatePerson("test1");
            IPerson person2 = PersonFactory.CreatePerson("test2");
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            var matrixes = new List<IScheduleMatrixPro> { _matrix1,_matrix2  };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var teaminfo = new TeamInfo(_group, groupMatrixList);
            IBlockInfo blockInfo = new BlockInfo(period);
            ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

            using (_mocks.Record())
            {
                Expect.Call(_matrix1.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();

                Expect.Call(_matrix1.Person).Return(person1).Repeat.AtLeastOnce();
                Expect.Call(_matrix2.Person).Return(person2).Repeat.AtLeastOnce();

                Expect.Call(_matrix2.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules[person2]).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(scheduleDay);
                Expect.Call(scheduleDay.IsScheduled()).Return(false);

            }

			Assert.IsFalse(_target.IsDayScheduledInTeamBlockForSelectedPersons(teamBlockInfo, _dateOnly, new List<IPerson> { person2 }));
        }

        [Test]
        public void ShouldReturnTrueIfDateIsScheduledForSelectedTeamMember()
        {
            IPerson person1 = PersonFactory.CreatePerson("test1");
            IPerson person2 = PersonFactory.CreatePerson("test2");
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            var matrixes = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var teaminfo = new TeamInfo(_group, groupMatrixList);
            IBlockInfo blockInfo = new BlockInfo(period);
            ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

            using (_mocks.Record())
            {
                Expect.Call(_matrix1.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();

                Expect.Call(_matrix1.Person).Return(person1).Repeat.AtLeastOnce();
                Expect.Call(_matrix2.Person).Return(person2).Repeat.AtLeastOnce();

                Expect.Call(_matrix2.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules[person2]).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(scheduleDay);
                Expect.Call(scheduleDay.IsScheduled()).Return(true);

            }
			Assert.IsTrue(_target.IsDayScheduledInTeamBlockForSelectedPersons(teamBlockInfo, _dateOnly, new List<IPerson> { person2 }));
        }

        [Test]
        public void ShouldReturnFalseIfDateIsNotScheduled()
        {
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            var dateOnly = new DateOnly(2013, 04, 10);

            var matrixes = new List<IScheduleMatrixPro> { _matrix1 };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var teaminfo = new TeamInfo(_group, groupMatrixList);
            IBlockInfo blockInfo = new BlockInfo(period);
            ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);

            using (_mocks.Record())
            {
                var scheduleDay = commonMocks(dateOnly);
                Expect.Call(scheduleDay.IsScheduled()).Return(false );
            }

			Assert.IsFalse(_target.IsDayScheduledInTeamBlock(teamBlockInfo, dateOnly));
        }



        private IScheduleDay commonMocks(DateOnly dateOnly)
        {
            IPerson person = PersonFactory.CreatePerson("test");
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            Expect.Call(_matrix1.SchedulingStateHolder).Return(_schedulingResultStateHolder);
            Expect.Call(_schedulingResultStateHolder.Schedules[person]).Return(_scheduleRange);
            Expect.Call(_scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
            Expect.Call(_matrix1.Person).Return(person);
			Expect.Call(_matrix1.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
			Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod).Repeat.AtLeastOnce();
            return scheduleDay;
        }

        [Test]
        public void ShouldReturnFalseIfFirstDayIsScheduled()
        {
            var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
            var dateOnly = new DateOnly(2013, 04, 10);

            var matrixes = new List<IScheduleMatrixPro> { _matrix1,_matrix2  };
            var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var teaminfo = new TeamInfo(_group, groupMatrixList);
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

				Expect.Call(_matrix1.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(period).Repeat.AtLeastOnce();
				Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
            }

			Assert.IsFalse(_target.IsDayScheduledInTeamBlock(teamBlockInfo, dateOnly));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldReturnFalseIfTeamBlockIsNull()
        {
            var dateOnly = new DateOnly(2013, 04, 10);
			Assert.IsFalse(_target.IsDayScheduledInTeamBlock(null, dateOnly));
        }

		  [Test]
		  public void ShouldReturnTrueIfDatesAreScheduledForSelectedTeamMember()
		  {
			  IPerson person1 = PersonFactory.CreatePerson("test1");
			  IPerson person2 = PersonFactory.CreatePerson("test2");
			  var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			  var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			  var period = new DateOnlyPeriod(2013, 04, 09, 2013, 04, 11);
			  var matrixes = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
			  var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			  var teaminfo = new TeamInfo(_group, groupMatrixList);
			  IBlockInfo blockInfo = new BlockInfo(period);
			  ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			  var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			  var dateOnly = new DateOnly(2013, 04, 10);
			  var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1));
			  var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			  using (_mocks.Record())
			  {
				  Expect.Call(_matrix1.SchedulePeriod).Return(schedulePeriod).Repeat.AtLeastOnce();
				  Expect.Call(schedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
				  Expect.Call(_matrix2.SchedulePeriod).Return(schedulePeriod).Repeat.AtLeastOnce();

				  Expect.Call(_matrix1.Person).Return(person1).Repeat.AtLeastOnce();
				  Expect.Call(_matrix2.Person).Return(person2).Repeat.AtLeastOnce();

				  Expect.Call(_matrix2.SchedulingStateHolder).Return(_schedulingResultStateHolder).Repeat.Twice();
				  Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary).Repeat.Twice();
				  Expect.Call(scheduleDictionary[person2]).Return(_scheduleRange).Repeat.Twice();
				  Expect.Call(_scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay1);
				  Expect.Call(_scheduleRange.ScheduledDay(dateOnly.AddDays(1))).Return(scheduleDay2);
				  Expect.Call(scheduleDay1.IsScheduled()).Return(true);
				  Expect.Call(scheduleDay2.IsScheduled()).Return(true);
			  }
			  Assert.IsTrue(_target.IsTeamBlockScheduledForSelectedPersons(teamBlockInfo, new List<IPerson> { person2 }));
		  }
    }
}
