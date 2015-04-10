using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class RemoveShiftCategoryBackToLegalServiceBuilderTest
    {
        private SchedulePeriodShiftCategoryBackToLegalStateServiceBuilder _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculator;
    	private IScheduleDayService _scheduleService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scheduleMatrixValueCalculator = _mockRepository.StrictMock<IScheduleMatrixValueCalculatorPro>();
        	_scheduleService = _mockRepository.StrictMock<IScheduleDayService>();
            _target = new SchedulePeriodShiftCategoryBackToLegalStateServiceBuilder(
                _scheduleMatrixValueCalculator,
                ()=>_scheduleService);
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

            using(_mockRepository.Record())
            {
				Expect.Call(schedulingOptions.Fairness).Return(new Percent(0.5));

            }
			ISchedulePeriodShiftCategoryBackToLegalStateService result = _target.Build(scheduleMatrixPro, schedulePartModifyAndRollbackService);
            Assert.IsNotNull(result);
        }

        [Test]
        public void VerifyBuildRemoveShiftCategoryOnBestDateService()
        {
            var scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            var scheduleMatrixValueCalculatorPro = _mockRepository.StrictMock<IScheduleMatrixValueCalculatorPro>();
            var scheduleDayService = _mockRepository.StrictMock<IScheduleDayService>();

            IRemoveShiftCategoryOnBestDateService result =
                _target.BuildRemoveShiftCategoryOnBestDateService(
                    scheduleMatrix,
                    scheduleMatrixValueCalculatorPro,
                    scheduleDayService);

            Assert.IsNotNull(result);
        }

        [Test]
        public void VerifyBuildSchedulePeriodBackToLegalStateService()
        {
            var removeShiftCategoryBackToLegalService = _mockRepository.StrictMock<IRemoveShiftCategoryBackToLegalService>();
            var scheduleDayService = _mockRepository.StrictMock<IScheduleDayService>();

            ISchedulePeriodShiftCategoryBackToLegalStateService result =
                _target.BuildSchedulePeriodBackToLegalStateService(
                    removeShiftCategoryBackToLegalService, scheduleDayService);

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
