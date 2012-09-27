using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupShiftCategoryBackToLegalStateServiceBuilderTest
    {
        private GroupShiftCategoryBackToLegalStateServiceBuilder _target;
        private MockRepository _mockRepository;
        private IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculator;
        private IDeleteSchedulePartService _deleteSchedulePartService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IGroupSchedulingService _scheduleService;
        private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _deleteSchedulePartService = _mockRepository.StrictMock<IDeleteSchedulePartService>();
            _resourceOptimizationHelper = _mockRepository.StrictMock<IResourceOptimizationHelper>();
            _scheduleMatrixValueCalculator = _mockRepository.StrictMock<IScheduleMatrixValueCalculatorPro>();
            _effectiveRestrictionCreator = _mockRepository.DynamicMock<IEffectiveRestrictionCreator>();
            _scheduleService = _mockRepository.StrictMock<IGroupSchedulingService>();
            _groupPersonBuilderForOptimization = _mockRepository.StrictMock<IGroupPersonBuilderForOptimization>();
            _target = new GroupShiftCategoryBackToLegalStateServiceBuilder(
                _scheduleMatrixValueCalculator,
                _scheduleService,_groupPersonBuilderForOptimization);
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
