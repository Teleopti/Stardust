using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class DynamicBlockFinderTest
    {
        private IDynamicBlockFinder _target;
        private SchedulingOptions  _schedulingOptions;
        private MockRepository _mock;
        private IScheduleMatrixPro _matrixPro;
	    private ITeamInfo _teamInfo;
	    private DateOnly _date;
	    private IVirtualSchedulePeriod _schedulePeriod;
	    private IScheduleMatrixPro _matrixPro2;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDay _scheduleDay1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDay _scheduleDay2;
        private IScheduleDayPro _scheduleDayPro3;
        private IScheduleDay _scheduleDay3;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulingOptions = new SchedulingOptions();
            _matrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
	        _teamInfo = _mock.StrictMock<ITeamInfo>();
			_target = new DynamicBlockFinder();
			_date = new DateOnly(2013, 02, 22);
		    _schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
        }

      

		[Test]
		public void ShouldReturnSameDateAsAskedForIfBlockFinderTypeIsSingleDay()
		{	_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
			
			using (_mock.Record())
            {
                
            }
            using (_mock.Playback())
			{
                IList<DateOnly> result = _target.ExtractBlockInfo(_date, _teamInfo, BlockFinderType.SingleDay).BlockPeriod.DayCollection();
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
				DateOnlyPeriod result = _target.ExtractBlockInfo(_date, _teamInfo, BlockFinderType.SchedulePeriod).BlockPeriod;
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
				DateOnlyPeriod result = _target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, BlockFinderType.SchedulePeriod).BlockPeriod;
				Assert.AreEqual(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)), result);
			}
		}

        [Test]
        public void ShouldReturnNullIfNoPeriodFound()
        {
	        var result = _target.ExtractBlockInfo(_date, _teamInfo, BlockFinderType.None);
			Assert.IsNull(result);
        }

        [Test]
        public void ShouldReturnCorrectBlockPeriodIfThereIsADayOff()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamInfo.MatrixesForGroupAndDate(_date.AddDays(1))).Return(new List<IScheduleMatrixPro> { _matrixPro, _matrixPro2 });
                Expect.Call(_matrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(3))).Repeat.AtLeastOnce() ;
                
                Expect.Call(_matrixPro.GetScheduleDayByKey(_date.AddDays(1))).Return(_scheduleDayPro1).Repeat.AtLeastOnce() ;
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

                Expect.Call(_matrixPro.GetScheduleDayByKey(_date.AddDays(2))).Return(_scheduleDayPro2).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

                Expect.Call(_matrixPro.GetScheduleDayByKey(_date.AddDays(3))).Return(_scheduleDayPro3).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro3.DaySchedulePart()).Return(_scheduleDay3).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();

            }

            using (_mock.Playback())
            {
                DateOnlyPeriod result = _target.ExtractBlockInfo(_date.AddDays(1), _teamInfo, BlockFinderType.BetweenDayOff).BlockPeriod ;
                Assert.AreEqual(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(2)), result);
            }
        }
        
    }

    
}
