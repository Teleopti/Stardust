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
        private  IGroupPersonPreOptimizationChecker _groupPersonPreOptimizationChecker;
        private  IGroupMatrixHelper _groupMatrixHelper;
        private GroupDayOffOptimizerCreator _target;
        private IDaysOffPreferences _daysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleResultDataExtractorProvider = _mock.StrictMock<IScheduleResultDataExtractorProvider>();
            _dayOffDecisionMakerExecuter = _mock.StrictMock<IDayOffDecisionMakerExecuter>();
            _changesTracker = _mock.StrictMock<ILockableBitArrayChangesTracker>();
            _schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _groupSchedulingService = _mock.StrictMock<IGroupSchedulingService>();
            _groupPersonPreOptimizationChecker = _mock.StrictMock<IGroupPersonPreOptimizationChecker>();
            _groupMatrixHelper = _mock.StrictMock<IGroupMatrixHelper>();
            _daysOffPreferences = new DaysOffPreferences();
            _target = new GroupDayOffOptimizerCreator(_scheduleResultDataExtractorProvider,
                                                      _changesTracker, _schedulePartModifyAndRollbackService,
                                                      _groupSchedulingService, _groupPersonPreOptimizationChecker,
                                                      _groupMatrixHelper);
        }

        [Test]
        public void ShouldReturnSingleVersionIfNotUseSameDaysOff()
        {
            var converter = _mock.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            var decisionMaker = _mock.StrictMock<IDayOffDecisionMaker>();
            var validators = new List<IDayOffLegalStateValidator>();
            var allPersons = new List<IPerson>();

            Expect.Call(_groupPersonPreOptimizationChecker.GroupPersonBuilder).Return(null);
            _mock.ReplayAll();
            var ret = _target.CreateDayOffOptimizer(converter, decisionMaker,_dayOffDecisionMakerExecuter, _daysOffPreferences, validators, allPersons, false);
            Assert.That(ret.GetType(), Is.EqualTo(typeof(GroupDayOffSingleOptimizer)));
            _mock.VerifyAll();
        }

		[Test]
		public void ShouldReturnOrdinaryVersionIfNotSameDaysOff()
		{
			var converter = _mock.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			var decisionMaker = _mock.StrictMock<IDayOffDecisionMaker>();
			var validators = new List<IDayOffLegalStateValidator>();
			var allPersons = new List<IPerson>();

			//Expect.Call(_groupPersonPreOptimizationChecker.GroupPersonBuilder).Return(_groupPersonsBuilder);
			_mock.ReplayAll();
			var ret = _target.CreateDayOffOptimizer(converter, decisionMaker, _dayOffDecisionMakerExecuter, _daysOffPreferences, validators, allPersons, true);
			Assert.That(ret.GetType(), Is.EqualTo(typeof(GroupDayOffOptimizer)));
			_mock.VerifyAll();
		}
    }

}