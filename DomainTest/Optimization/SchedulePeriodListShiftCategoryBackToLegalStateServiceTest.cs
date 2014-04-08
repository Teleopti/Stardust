using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulePeriodListShiftCategoryBackToLegalStateServiceTest
    {
        private SchedulePeriodListShiftCategoryBackToLegalStateService _target;
        private MockRepository _mockRepository;
        private IList<IScheduleMatrixPro> _scheduleMatrixList;
        private IOptimizationPreferences _optimizerPreferences;
        private IScheduleFairnessCalculator _scheduleFairnessCalculator;
        private IScheduleMatrixValueCalculatorProFactory _scheduleMatrixValueCalculatorFactory;
        private ISchedulingResultStateHolder _stateHolder;
    	private IScheduleDayService _scheduleService;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scheduleMatrixList = new List<IScheduleMatrixPro>();
            _optimizerPreferences = new OptimizationPreferences();
            _scheduleFairnessCalculator = _mockRepository.StrictMock<IScheduleFairnessCalculator>();
            _stateHolder = _mockRepository.StrictMock<ISchedulingResultStateHolder>();
            _scheduleDayChangeCallback = _mockRepository.DynamicMock<IScheduleDayChangeCallback>();
            _scheduleMatrixValueCalculatorFactory = _mockRepository.StrictMock<IScheduleMatrixValueCalculatorProFactory>();
        	_scheduleService = _mockRepository.StrictMock<IScheduleDayService>();
            _target = new SchedulePeriodListShiftCategoryBackToLegalStateService(
                _stateHolder,
                _scheduleMatrixValueCalculatorFactory,
				_scheduleFairnessCalculator,
				_scheduleService,
                _scheduleDayChangeCallback);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }


        [Test]
        public void VerifyExecute()
        {
            //var schedulePeriodBackToLegalStateServiceBuilder = _mockRepository.StrictMock<ISchedulePeriodShiftCategoryBackToLegalStateServiceBuilder>();
             var scheduleMatrixValueCalculator = _mockRepository.StrictMock<IScheduleMatrixValueCalculatorPro>();
             //var schedulePeriodBackToLegalStateService = _mockRepository.StrictMock<ISchedulePeriodShiftCategoryBackToLegalStateService>();

            var scheduleMatrixPro = _mockRepository.StrictMock<IScheduleMatrixPro>();
            var schedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
			var scheduleService = _mockRepository.StrictMock<IScheduleDayService>();
        	//var schedulePartModifyAndRollbackService = _mockRepository.StrictMock<ISchedulePartModifyAndRollbackService>();
            
            _scheduleMatrixList.Add(scheduleMatrixPro);
            _scheduleMatrixList.Add(scheduleMatrixPro);
            _scheduleMatrixList.Add(scheduleMatrixPro);

            var target = new SchedulePeriodListShiftCategoryBackToLegalStateService(
                _stateHolder,
                _scheduleMatrixValueCalculatorFactory,
                _scheduleFairnessCalculator,
                scheduleService, _scheduleDayChangeCallback);

            int iteration = _scheduleMatrixList.Count;

            using (_mockRepository.Record())
            {
                Expect.Call(
                    _scheduleMatrixValueCalculatorFactory.CreateScheduleMatrixValueCalculatorPro(new List<DateOnly>(),
                                                                                                 null,
                                                                                                 null, null)).
                    IgnoreArguments().Return(scheduleMatrixValueCalculator);

                Expect.Call(scheduleMatrixPro.EffectivePeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>())).Repeat.Times(iteration);
                Expect.Call(schedulePeriod.ShiftCategoryLimitationCollection()).Return(
                    new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation>())).Repeat.AtLeastOnce();
                //Expect.Call(schedulePeriodBackToLegalStateServiceBuilder.Build(scheduleMatrixPro, schedulePartModifyAndRollbackService)).IgnoreArguments().Return(
                //    schedulePeriodBackToLegalStateService).Repeat.Times(iteration);
                Expect.Call(scheduleMatrixPro.SchedulePeriod).Return(schedulePeriod).Repeat.Times(iteration);
                //Expect.Call(schedulePeriodBackToLegalStateService.Execute(schedulePeriod)).Return(true).Repeat.Once();
                //Expect.Call(schedulePeriodBackToLegalStateService.Execute(schedulePeriod)).Return(false).Repeat.Once();
                //Expect.Call(schedulePeriodBackToLegalStateService.Execute(schedulePeriod)).Return(true).Repeat.Once();
            }
            using(_mockRepository.Playback())
            {
                target.Execute(_scheduleMatrixList, new SchedulingOptions(), _optimizerPreferences);
            }

            
        }

        [Test]
        public void VerifyBuildScheduleMatrixValueCalculator()
        {
            _scheduleMatrixValueCalculatorFactory = new ScheduleMatrixValueCalculatorProFactory();

            // create 2 matrixes with 2 days in each
            var matrix1 = _mockRepository.StrictMock<IScheduleMatrixPro>();
            var matrix2 = _mockRepository.StrictMock<IScheduleMatrixPro>();
            IScheduleDayPro matrix1Day1 = new ScheduleDayPro(new DateOnly(), matrix1);
            IScheduleDayPro matrix1Day2 = new ScheduleDayPro(new DateOnly(), matrix1);
            IScheduleDayPro matrix2Day1 = new ScheduleDayPro(new DateOnly(), matrix2);
            IScheduleDayPro matrix2Day2 = new ScheduleDayPro(new DateOnly(), matrix2);
            IList<IScheduleDayPro> list1 = new List<IScheduleDayPro> { matrix1Day1, matrix1Day2 };
            var readOnlyCollection1 = new ReadOnlyCollection<IScheduleDayPro>(list1);
            IList<IScheduleDayPro> list2 = new List<IScheduleDayPro> { matrix2Day1, matrix2Day2 };
            var readOnlyCollection2 = new ReadOnlyCollection<IScheduleDayPro>(list2);
            _scheduleMatrixList = new List<IScheduleMatrixPro> { matrix1, matrix2 };



            _target = new SchedulePeriodListShiftCategoryBackToLegalStateService(
                _stateHolder,
                _scheduleMatrixValueCalculatorFactory,
                _scheduleFairnessCalculator,
                _scheduleService, _scheduleDayChangeCallback);

            using (_mockRepository.Record())
            {
               Expect.Call(matrix1.EffectivePeriodDays).Return(readOnlyCollection1).Repeat.AtLeastOnce();
                Expect.Call(matrix2.EffectivePeriodDays).Return(readOnlyCollection2).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.Skills).Return(new List<ISkill>()).Repeat.AtLeastOnce();
            }
            using (_mockRepository.Playback())
            {
                
                var result = _target.BuildScheduleMatrixValueCalculator(
                    _scheduleMatrixValueCalculatorFactory,
                    _scheduleMatrixList,
                    _optimizerPreferences,
                    _stateHolder,
                    _scheduleFairnessCalculator);
                Assert.IsNotNull(result);
                Assert.AreEqual(4, new List<DateOnly>(result.ScheduleDays).Count);
            }
        }

        [Test]
        public void VerifyCreateSchedulePeriodBackToLegalStateServiceBuilder()
        {

            var scheduleMatrixValueCalculator = _mockRepository.StrictMock<IScheduleMatrixValueCalculatorPro>();

            ISchedulePeriodShiftCategoryBackToLegalStateServiceBuilder result =
                _target.CreateSchedulePeriodBackToLegalStateServiceBuilder(scheduleMatrixValueCalculator);

            Assert.IsNotNull(result);
        }

    }
}
