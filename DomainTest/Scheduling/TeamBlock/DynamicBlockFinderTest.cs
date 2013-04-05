using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class DynamicBlockFinderTest
    {
        private IDynamicBlockFinder _target;
        private MockRepository _mock;
        private IScheduleMatrixPro _matrixPro;
	    private ITeamInfo _teamInfo;
	    private DateOnly _date;
	    private IVirtualSchedulePeriod _schedulePeriod;
	    private IScheduleMatrixPro _matrixPro2;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleDictionary _scheduleDictionary;
        private IPerson _person;
        private IScheduleRange _range;
        private IScheduleDay _scheduleDay;


        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _matrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
	        _teamInfo = _mock.StrictMock<ITeamInfo>();
			_target = new DynamicBlockFinder();
			_date = new DateOnly(2013, 02, 22);
		    _schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            _range = _mock.StrictMock<IScheduleRange>();
            _person = PersonFactory.CreatePerson();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
        }

      

		[Test]
		public void ShouldReturnSameDateAsAskedForIfBlockFinderTypeIsSingleDay()
		{	
			using (_mock.Record())
            {
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date)).Return(new List<IScheduleMatrixPro> { _matrixPro });
            }
            using (_mock.Playback())
			{
                IList<DateOnly> result = _target.ExtractBlockInfo(_date, _teamInfo, BlockFinderType.SingleDay,false ).BlockPeriod.DayCollection();
				Assert.AreEqual(_date, result[0]);
				Assert.AreEqual(1, result.Count);
			}
      
		}

        [Test]
        public void ShouldReturnSamePeriodAsSchedulePeriodIfBlockFinderTypeIsSchedulePeriod()
        {

			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date)).Return(new List<IScheduleMatrixPro> { _matrixPro });
				Expect.Call(_matrixPro.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date, _date));
			}

			using (_mock.Playback())
			{
                DateOnlyPeriod result = _target.ExtractBlockInfo(_date, _teamInfo, BlockFinderType.SchedulePeriod, false).BlockPeriod;
				Assert.AreEqual(new DateOnlyPeriod(_date, _date), result);
			}
      
        }

		[Test]
		public void ShouldReturnCorrectBlockPeriodIfMoreThanOneMatrixIsOnTheMembersInTeamInfo()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date.AddDays(1))).Return(new List<IScheduleMatrixPro> { _matrixPro, _matrixPro2 });
				Expect.Call(_matrixPro.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)));
			}

			using (_mock.Playback())
			{
                DateOnlyPeriod result = _target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, BlockFinderType.SchedulePeriod, false).BlockPeriod;
				Assert.AreEqual(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)), result);
			}
		}


		[Test]
		public void ShouldReturnNullIfBlockFinderTypeNone()
		{
            var result = _target.ExtractBlockInfo(_date, _teamInfo, BlockFinderType.None, false);
			Assert.IsNull(result);
		}

        [Test]
        public void ShouldReturnNullIfIfBlockFinderIsNone()
        {
            var result = _target.ExtractBlockInfo(_date, _teamInfo, BlockFinderType.None, false);
			Assert.IsNull(result);
        }

        [Test]
        public void ShouldReturnNullIfIfNoMatrixFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date)).Return(new List<IScheduleMatrixPro> ());
                
            }

            var result = _target.ExtractBlockInfo(_date, _teamInfo, BlockFinderType.SchedulePeriod, false);
            Assert.IsNull(result);
        } 

        [Test]
        public void ShouldReturnNullIfProvidedDateIsDayOff()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date.AddDays(1))).Return(new List<IScheduleMatrixPro> { _matrixPro });
                Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_matrixPro.Person).Return(_person);
                Expect.Call(_range.ScheduledDay(_date.AddDays(1))).Return(_scheduleDay);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
            }

            using (_mock.Playback())
            {
                Assert.IsNull(_target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, BlockFinderType.BetweenDayOff, false));
            }
        }

        [Test]
        public void ShouldReturnNullIfProvidedDateIsAbsenceWithSingleAgentTeam()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date.AddDays(1))).Return(new List<IScheduleMatrixPro> { _matrixPro });
                Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_matrixPro.Person).Return(_person);
                Expect.Call(_range.ScheduledDay(_date.AddDays(1))).Return(_scheduleDay) ;
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.Absence).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                Assert.IsNull(_target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, BlockFinderType.BetweenDayOff, true ));
            }
        }

        [Test]
        public void ShouldReturnNullIfProvidedDateIsFullDayAbsenceWithSingleAgentTeam()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date.AddDays(1))).Return(new List<IScheduleMatrixPro> { _matrixPro });
                Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_matrixPro.Person).Return(_person);
                Expect.Call(_range.ScheduledDay(_date.AddDays(1))).Return(_scheduleDay);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                Assert.IsNull(_target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, BlockFinderType.BetweenDayOff, true));
            }
        }

        [Test]
        public void ShouldReturnCorrectPeriodForBetweenDayOff()
        {
            var date = new DateOnly(2013, 4, 1);
            var rangePeriod = new DateTimePeriod(2013, 1, 1, 2013, 12, 31);
            var matrixPeriod = new DateOnlyPeriod(2013, 4, 1, 2013, 4, 2);
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.MatrixesForGroupAndDate(date)).Return(new List<IScheduleMatrixPro> { _matrixPro });
                Expect.Call(_matrixPro.SchedulingStateHolder).Return(_schedulingResultStateHolder);
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_range);
                Expect.Call(_matrixPro.Person).Return(_person);
                Expect.Call(_range.Period).Return(rangePeriod);

                Expect.Call(_matrixPro.SchedulePeriod).Return(_schedulePeriod);
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(matrixPeriod);
                
                Expect.Call(_range.ScheduledDay(date)).Return(_scheduleDay);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);

                Expect.Call(_range.ScheduledDay(new DateOnly(2013, 3, 31))).Return(_scheduleDay);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);

                Expect.Call(_range.ScheduledDay(new DateOnly(2013, 4, 2))).Return(_scheduleDay);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
                
            }

            using (_mock.Playback())
            {
                DateOnlyPeriod result = _target.ExtractBlockInfo(date, _teamInfo, BlockFinderType.BetweenDayOff, false).BlockPeriod;
                Assert.AreEqual(new DateOnlyPeriod(date, date), result);
            }
        }
    }

    
}
