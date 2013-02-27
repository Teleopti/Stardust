using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class DynamicBlockFinderTest
    {
        private IDynamicBlockFinder _target;
        private SchedulingOptions  _schedulingOptions;
        private MockRepository _mock;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleMatrixPro _matrixPro;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDayPro _scheduleDayPro3;
        private ReadOnlyCollection<IScheduleDayPro> _scheduleDayReadOnlyProList;
        private IList<IScheduleDayPro> _scheduleDayProList;
        private IScheduleDayPro _scheduleDayPro4;
        private IGroupPerson   _groupPerson;
        private BaseLineData _baseLine;
        private IScheduleDay _schedulePart;
	    private ITeamInfo _teamInfo;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _schedulingOptions = new SchedulingOptions();
            _matrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _matrixList = new List<IScheduleMatrixPro> {_matrixPro};
            _schedulePart = _mock.StrictMock<IScheduleDay>();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _baseLine = new BaseLineData();
	        _teamInfo = _mock.StrictMock<ITeamInfo>();
        }

      

		[Test]
		public void ShouldReturnSameDateAsAskedForIfBlockFinderTypeIsSingleDay()
		{	_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
			var date = new DateOnly(2013, 02, 22);
			_target = new DynamicBlockFinder();
            using (_mock.Record())
            {
                
            }
            using (_mock.Playback())
			{
                IList<DateOnly> result = _target.ExtractBlockInfo(date, _teamInfo, BlockFinderType.SingleDay).BlockPeriod.DayCollection();
				Assert.AreEqual(date, result[0]);
				Assert.AreEqual(1, result.Count);
			}
      
      
		}

       

        [Test]
        public void FindSkillDayFromBlockUsingTwoDaysOff()
        {
            
        }

        [Test]
        public void FindSkillDayFromBlockUsingCalendarWeek()
        {
            
        }
        
    }

    
}
