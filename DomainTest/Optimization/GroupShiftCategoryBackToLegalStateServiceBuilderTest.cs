using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupShiftCategoryBackToLegalStateServiceBuilderTest
    {
        private GroupShiftCategoryBackToLegalStateServiceBuilder _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculator;
        private IGroupSchedulingService _scheduleService;
        private IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
        private IGroupPersonsBuilder _groupPersonsBuilder;
  

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scheduleMatrixValueCalculator = _mockRepository.StrictMock<IScheduleMatrixValueCalculatorPro>();
            _scheduleService = _mockRepository.StrictMock<IGroupSchedulingService>();
            _groupPersonsBuilder = _mockRepository.StrictMock<IGroupPersonsBuilder>();
            _groupOptimizerFindMatrixesForGroup = _mockRepository.StrictMock<IGroupOptimizerFindMatrixesForGroup>();
            _target = new GroupShiftCategoryBackToLegalStateServiceBuilder(
                _scheduleMatrixValueCalculator,
                _scheduleService, _groupPersonsBuilder, _groupOptimizerFindMatrixesForGroup);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyBuild()
        {
            var schedulingOptions = _mockRepository.StrictMock<ISchedulingOptions>();
            var scheduleMatrixPro = _mockRepository.StrictMock<IScheduleMatrixPro>();
            var schedulePartModifyAndRollbackService =
                _mockRepository.StrictMock<ISchedulePartModifyAndRollbackService>();

            using (_mockRepository.Record())
            {
                Expect.Call(schedulingOptions.Fairness).Return(new Percent(0.5));

            }
            var result = _target.Build(scheduleMatrixPro, schedulePartModifyAndRollbackService);
            Assert.IsNotNull(result);
        }

        [Test]
        public void VerifyBuildRemoveShiftCategoryOnBestDateService()
        {
            var scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            var scheduleMatrixValueCalculatorPro = _mockRepository.StrictMock<IScheduleMatrixValueCalculatorPro>();

            var result =
                _target.BuildRemoveGroupShiftCategoryOnBestDateService(
                    scheduleMatrix,
                    scheduleMatrixValueCalculatorPro,
                    _scheduleService);

            Assert.IsNotNull(result);
        }

        [Test]
        public void VerifyBuildSchedulePeriodBackToLegalStateService()
        {
            var removeShiftCategoryBackToLegalService = _mockRepository.StrictMock<IRemoveShiftCategoryBackToLegalService>();

            var result =
                _target.BuildSchedulePeriodBackToLegalStateService(
                    removeShiftCategoryBackToLegalService, _scheduleService);

            Assert.IsNotNull(result);
        }

        [Test]
        public void VerifyBuildRemoveShiftCategoryBackToLegalService()
        {
            var scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            var removeShiftCategoryOnBestDateService =
                _mockRepository.StrictMock<IRemoveShiftCategoryOnBestDateService>();

            IRemoveShiftCategoryBackToLegalService result =
                _target.BuildRemoveShiftCategoryBackToLegalService(
                    removeShiftCategoryOnBestDateService, scheduleMatrix);

            Assert.IsNotNull(result);
        }
    }
}
