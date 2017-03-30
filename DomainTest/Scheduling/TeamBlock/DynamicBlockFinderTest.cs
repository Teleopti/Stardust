using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
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
        private IPerson _person;
        private IScheduleRange _range;
        private IScheduleDay _scheduleDay;
	    private IScheduleDayPro _scheduleDayPro;


	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _matrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
	        _teamInfo = _mock.StrictMock<ITeamInfo>();
			_target = new DynamicBlockFinder(false);
			_date = new DateOnly(2013, 02, 22);
		    _schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _range = _mock.StrictMock<IScheduleRange>();
            _person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
		    _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
        }

      

		[Test]
		public void ShouldReturnSameDateAsAskedForIfBlockFinderTypeIsSingleDay()
		{	
			using (_mock.Record())
            {
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date)).Return(new List<IScheduleMatrixPro> { _matrixPro });
				Expect.Call(_matrixPro.GetScheduleDayByKey(_date)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
            }
            using (_mock.Playback())
			{
                var result = _target.ExtractBlockInfo(_date, _teamInfo, new SingleDayBlockFinder(), false ).BlockPeriod;
				Assert.AreEqual(_date, result.StartDate);
				Assert.AreEqual(1, result.DayCount());
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
                DateOnlyPeriod result = _target.ExtractBlockInfo(_date, _teamInfo, new SchedulePeriodBlockFinder(), false).BlockPeriod;
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
                DateOnlyPeriod result = _target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, new SchedulePeriodBlockFinder(), false).BlockPeriod;
				Assert.AreEqual(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)), result);
			}
		}

        [Test]
        public void ShouldReturnNullIfIfNoMatrixFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date)).Return(new List<IScheduleMatrixPro> ());
                
            }

            var result = _target.ExtractBlockInfo(_date, _teamInfo, new SchedulePeriodBlockFinder(), false);
            Assert.IsNull(result);
        } 

        [Test]
		public void ShouldReturnNullIfProvidedDateIsDayOffAndBetweenDayOff()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date.AddDays(1))).Return(new List<IScheduleMatrixPro> { _matrixPro });
				Expect.Call(_matrixPro.ActiveScheduleRange).Return(_range);
                Expect.Call(_matrixPro.Person).Return(_person);
                Expect.Call(_range.ScheduledDay(_date.AddDays(1))).Return(_scheduleDay);
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
            }

            using (_mock.Playback())
            {
                Assert.IsNull(_target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, new BetweenDayOffBlockFinder(), false));
            }
        }

		[Test]
		public void ShouldReturnNullIfProvidedDateIsDayOffAndSingleDay()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date.AddDays(1))).Return(new List<IScheduleMatrixPro> { _matrixPro });
				Expect.Call(_matrixPro.GetScheduleDayByKey(_date.AddDays(1))).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(true);
			}

			using (_mock.Playback())
			{
				Assert.IsNull(_target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, new SingleDayBlockFinder(), false));
			}
		}

		[Test]
		public void ShouldNotReturnNullIfProvidedDateIsNotDayOffOnOneOfTheAgentsAndSingleDay()
		{
			var matrixProEmpty = _mock.StrictMock<IScheduleMatrixPro>();
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date.AddDays(1))).Return(new List<IScheduleMatrixPro> { _matrixPro, matrixProEmpty });
				Expect.Call(_matrixPro.GetScheduleDayByKey(_date.AddDays(1))).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(true);
				Expect.Call(matrixProEmpty.GetScheduleDayByKey(_date.AddDays(1))).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.HasDayOff()).Return(false);
			}

			using (_mock.Playback())
			{
				Assert.IsNotNull(_target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, new SingleDayBlockFinder(), false));
			}
		}
        
    }

    
}
