using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupDayOffOptimizerCreatorTest
    {
        private MockRepository _mock;
        private  IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private  IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private  ILockableBitArrayChangesTracker _changesTracker;
        private  ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private  IGroupSchedulingService _groupSchedulingService;
        private  IGroupMatrixHelper _groupMatrixHelper;
        private GroupDayOffOptimizerCreator _target;
        private IDaysOffPreferences _daysOffPreferences;
    	private IGroupOptimizationValidatorRunner _groupOptimizationValidatorRunner;
    	private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleResultDataExtractorProvider = _mock.StrictMock<IScheduleResultDataExtractorProvider>();
            _dayOffDecisionMakerExecuter = _mock.StrictMock<IDayOffDecisionMakerExecuter>();
            _changesTracker = _mock.StrictMock<ILockableBitArrayChangesTracker>();
            _schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _groupSchedulingService = _mock.StrictMock<IGroupSchedulingService>();
            _groupMatrixHelper = _mock.StrictMock<IGroupMatrixHelper>();
            _daysOffPreferences = new DaysOffPreferences();
        	_groupOptimizationValidatorRunner = _mock.StrictMock<IGroupOptimizationValidatorRunner>();
        	_groupPersonBuilderForOptimization = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
            _target = new GroupDayOffOptimizerCreator(_scheduleResultDataExtractorProvider,
                                                      _changesTracker, _schedulePartModifyAndRollbackService,
                                                      _groupSchedulingService, 
													  _groupMatrixHelper, _groupOptimizationValidatorRunner, _groupPersonBuilderForOptimization);
        }

       

		[Test]
		public void ShouldReturnOrdinaryVersion()
		{
			var converter = _mock.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			var decisionMaker = _mock.StrictMock<IDayOffDecisionMaker>();


			_mock.ReplayAll();
			var ret = _target.CreateDayOffOptimizer(converter, decisionMaker, _dayOffDecisionMakerExecuter, _daysOffPreferences);
			Assert.That(ret.GetType(), Is.EqualTo(typeof(GroupDayOffOptimizer)));
			_mock.VerifyAll();
		}
    }

}