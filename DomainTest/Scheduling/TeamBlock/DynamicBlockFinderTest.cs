using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
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
    }

    
}
