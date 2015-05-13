using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class FairnessAndMaxSeatCalculatorsManagerTest
    {
        private MockRepository _mocks;

        private ISchedulingResultStateHolder _stateHolder;
        private IPerson _person;
        private ISeatLimitationWorkShiftCalculator2 _seatLimitationWorkShiftCalculator;
        private IShiftCategoryFairnessShiftValueCalculator _shiftCategoryFairnessShiftValueCalculator;
        private IShiftCategoryFairnessManager _shiftCatFairnessManager;
        private FairnessAndMaxSeatCalculatorsManager _target;
        private IShiftCategoryFairnessFactors _shiftCategoryFairnessFactors;
        private IList<IShiftCategory> _shiftCategories;
        private IShiftCategory _shiftCategory;
        private SchedulingOptions _options;
	    private IWorkflowControlSet _workFlowControlSet;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _seatLimitationWorkShiftCalculator = _mocks.StrictMock<ISeatLimitationWorkShiftCalculator2>();
            _shiftCategoryFairnessShiftValueCalculator = _mocks.StrictMock<IShiftCategoryFairnessShiftValueCalculator>();
            _shiftCatFairnessManager = _mocks.StrictMock<IShiftCategoryFairnessManager>();
            _options = new SchedulingOptions{ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
                WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime, UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak};
            _target = new FairnessAndMaxSeatCalculatorsManager(()=>_stateHolder, _shiftCatFairnessManager,
                _shiftCategoryFairnessShiftValueCalculator,_seatLimitationWorkShiftCalculator);

            _shiftCategoryFairnessFactors = _mocks.StrictMock<IShiftCategoryFairnessFactors>();
            _person = _mocks.StrictMock<IPerson>();
            _shiftCategory = _mocks.StrictMock<IShiftCategory>();
            _shiftCategories = new List<IShiftCategory> { _shiftCategory };
			_workFlowControlSet = new WorkflowControlSet();
			_workFlowControlSet.SetFairnessType(FairnessType.EqualNumberOfShiftCategory);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldRunCategoryFairnessAndMaxSeatCalculators()
        {
            var dateOnly = new DateOnly(2011,4,28);
            var maxSeatSkillPeriods = _mocks.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();
            var shiftCache = _mocks.StrictMock<IShiftProjectionCache>();
            var mainShift = _mocks.StrictMock<IEditableShift>();
            var projection = _mocks.StrictMock<IVisualLayerCollection>();
            var category = new ShiftCategory("theCat");
            var allValues = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = 1, ShiftProjection = shiftCache } 
                              };
			_options.Fairness = new Percent(0.25d);
            Expect.Call(_shiftCatFairnessManager.GetFactorsForPersonOnDate(_person, dateOnly)).Return(
                _shiftCategoryFairnessFactors);
            
            Expect.Call(shiftCache.TheMainShift).Return(mainShift);
            Expect.Call(mainShift.ShiftCategory).Return(category);
            Expect.Call(_shiftCategoryFairnessFactors.FairnessFactor(category)).Return(5);
            Expect.Call(_shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(1, 5, 3, _options)).IgnoreArguments().Return(11);

				Expect.Call(shiftCache.MainShiftProjection).Return(projection).Repeat.AtLeastOnce();

            Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
                                                                               _options.UserOptionMaxSeatsFeature)).Return(55);
			Expect.Call(_person.WorkflowControlSet).Return(_workFlowControlSet);
            _mocks.ReplayAll();
            var result = _target.RecalculateFoundValues(allValues, 3, _person, dateOnly,  maxSeatSkillPeriods,
										   TimeSpan.FromHours(8), _options);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Value, Is.EqualTo(66));
            _mocks.VerifyAll();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnAllResultsWithMaxValue()
        {
            var dateOnly = new DateOnly(2011, 4, 28);
            var maxSeatSkillPeriods = _mocks.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();
            var shiftCache = _mocks.StrictMock<IShiftProjectionCache>();
            var mainShift = _mocks.StrictMock<IEditableShift>();
            var projection = _mocks.StrictMock<IVisualLayerCollection>();
            var category = new ShiftCategory("theCat");
            var allValues = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = 1, ShiftProjection = shiftCache },
                                  new WorkShiftCalculationResult { Value = 1, ShiftProjection = shiftCache }
                              };

				_options.Fairness = new Percent(0.25d);
            Expect.Call(_shiftCatFairnessManager.GetFactorsForPersonOnDate(_person, dateOnly)).Return(
                _shiftCategoryFairnessFactors);

            Expect.Call(shiftCache.TheMainShift).Return(mainShift).Repeat.Twice();
            Expect.Call(mainShift.ShiftCategory).Return(category).Repeat.Twice();
            Expect.Call(_shiftCategoryFairnessFactors.FairnessFactor(category)).Return(5).Repeat.Twice();
				Expect.Call(_shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(1, 5, 3, _options))
				.IgnoreArguments()
				.Return(11)
				.Repeat.Twice();

            Expect.Call(shiftCache.MainShiftProjection).Return(projection).Repeat.AtLeastOnce();
           
            Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
                                                                               _options.UserOptionMaxSeatsFeature)).Return(0);
			Expect.Call(_person.WorkflowControlSet).Return(_workFlowControlSet);

            _mocks.ReplayAll();
			var result = _target.RecalculateFoundValues(allValues, 3, _person, dateOnly, maxSeatSkillPeriods,
										   TimeSpan.FromHours(8), _options);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Value, Is.EqualTo(11));
            Assert.That(result[1].Value, Is.EqualTo(11));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSkipShiftWhenMaxSeatCalculatorReturnsNull()
        {
            var dateOnly = new DateOnly(2011, 4, 28);
            var maxSeatSkillPeriods = _mocks.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();
            var shiftCache = _mocks.StrictMock<IShiftProjectionCache>();
            var mainShift = _mocks.StrictMock<IEditableShift>();
            var projection = _mocks.StrictMock<IVisualLayerCollection>();
            var category = new ShiftCategory("theCat");
            var allValues = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = 1, ShiftProjection = shiftCache } 
                              };
				_options.Fairness = new Percent(0.25d);
            Expect.Call(_shiftCatFairnessManager.GetFactorsForPersonOnDate(_person, dateOnly)).Return(
                _shiftCategoryFairnessFactors);

            Expect.Call(shiftCache.TheMainShift).Return(mainShift);
            Expect.Call(mainShift.ShiftCategory).Return(category);
            Expect.Call(_shiftCategoryFairnessFactors.FairnessFactor(category)).Return(5);
			Expect.Call(_shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(1, 5, 3, new SchedulingOptions { Fairness = new Percent(0.25d) })).IgnoreArguments().Return(11);

            Expect.Call(shiftCache.MainShiftProjection).Return(projection);

            Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
                                                                               _options.UserOptionMaxSeatsFeature)).Return(null);
			Expect.Call(_person.WorkflowControlSet).Return(_workFlowControlSet);
            _mocks.ReplayAll();
			var result = _target.RecalculateFoundValues(allValues, 3, _person, dateOnly, maxSeatSkillPeriods,
										   TimeSpan.FromHours(8), _options);
            Assert.That(result.Count, Is.EqualTo(0));
           _mocks.VerifyAll();
        }

		[Test]
		public void ShouldNotRunFairnessCalculatorsIfNoFairnessValue()
		{
			var dateOnly = new DateOnly(2011, 4, 28);
			var maxSeatSkillPeriods = _mocks.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();
			var shiftCache = _mocks.StrictMock<IShiftProjectionCache>();
			var projection = _mocks.StrictMock<IVisualLayerCollection>();
			var allValues = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = 1, ShiftProjection = shiftCache } 
                              };
			_options.Fairness = new Percent(0);
			Expect.Call(shiftCache.MainShiftProjection).Return(projection).Repeat.AtLeastOnce();
			Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
																			   _options.UserOptionMaxSeatsFeature)).Return(55);
			Expect.Call(_person.WorkflowControlSet).Return(_workFlowControlSet);

			_mocks.ReplayAll();
			var result = _target.RecalculateFoundValues(allValues, 3, _person, dateOnly, maxSeatSkillPeriods,
										   TimeSpan.FromHours(8), _options);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].Value, Is.EqualTo(56));
			_mocks.VerifyAll();
		}
    }

    
}