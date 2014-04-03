using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class FairnessAndMaxSeatCalculatorsManagerTest
    {
        private MockRepository _mocks;

        private ISchedulingResultStateHolder _stateHolder;
        private IPerson _person;
        private IFairnessValueCalculator _fairnessValueCalculator;
        private ISeatLimitationWorkShiftCalculator2 _seatLimitationWorkShiftCalculator;
        private IShiftCategoryFairnessShiftValueCalculator _shiftCategoryFairnessShiftValueCalculator;
        private IShiftCategoryFairnessManager _shiftCatFairnessManager;
        private FairnessAndMaxSeatCalculatorsManager _target;
        private IShiftCategoryFairnessFactors _shiftCategoryFairnessFactors;
        private IList<IShiftCategory> _shiftCategories;
        private IShiftCategory _shiftCategory;
        private SchedulingOptions _options;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _fairnessValueCalculator = _mocks.StrictMock<IFairnessValueCalculator>();
            _seatLimitationWorkShiftCalculator = _mocks.StrictMock<ISeatLimitationWorkShiftCalculator2>();
            _shiftCategoryFairnessShiftValueCalculator = _mocks.StrictMock<IShiftCategoryFairnessShiftValueCalculator>();
            _shiftCatFairnessManager = _mocks.StrictMock<IShiftCategoryFairnessManager>();
            _options = new SchedulingOptions{ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
                WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime, UseMaxSeats = true, DoNotBreakMaxSeats = true};
            _target = new FairnessAndMaxSeatCalculatorsManager(_stateHolder, _shiftCatFairnessManager,
                _shiftCategoryFairnessShiftValueCalculator, _fairnessValueCalculator,_seatLimitationWorkShiftCalculator);

            _shiftCategoryFairnessFactors = _mocks.StrictMock<IShiftCategoryFairnessFactors>();
            _person = _mocks.StrictMock<IPerson>();
            _shiftCategory = _mocks.StrictMock<IShiftCategory>();
            _shiftCategories = new List<IShiftCategory> { _shiftCategory };
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
			//Expect.Call(projection.ContractTime()).Return(TimeSpan.FromHours(7));
            //Expect.Call(_averageShiftLengthValueCalculator.CalculateShiftValue(11, TimeSpan.FromHours(7),
            //                                                                   TimeSpan.FromHours(8))).Return(33);

            Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
                                                                               true)).Return(55);

            //Expect.Call(_averageShiftLengthValueCalculator.CalculateShiftValue(66, TimeSpan.FromHours(7), TimeSpan.FromHours(8))).Return(88);
            _mocks.ReplayAll();
            var result = _target.RecalculateFoundValues(allValues, 3, true, _person, dateOnly,  maxSeatSkillPeriods,
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
			Expect.Call(_shiftCategoryFairnessShiftValueCalculator.ModifiedShiftValue(1, 5, 3, _options)).IgnoreArguments().Return(11).Repeat.Twice();

            Expect.Call(shiftCache.MainShiftProjection).Return(projection).Repeat.AtLeastOnce();
            //Expect.Call(projection.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.Twice();
           
            Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
                                                                               true)).Return(0);
            //Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
            //                                                                   true)).Return(-7);

			//Expect.Call(_averageShiftLengthValueCalculator.CalculateShiftValue(11, TimeSpan.FromHours(7),
			//                                                                  TimeSpan.FromHours(8))).Return(33);
			//Expect.Call(_averageShiftLengthValueCalculator.CalculateShiftValue(11, TimeSpan.FromHours(7),
			//                                                                   TimeSpan.FromHours(8))).Return(33);

            _mocks.ReplayAll();
            var result = _target.RecalculateFoundValues(allValues, 3, true, _person, dateOnly,  maxSeatSkillPeriods,
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
            //Expect.Call(projection.ContractTime()).Return(TimeSpan.FromHours(7));
            //Expect.Call(_averageShiftLengthValueCalculator.CalculateShiftValue(11, TimeSpan.FromHours(7),
            //                                                                   TimeSpan.FromHours(8))).Return(33);

            Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
                                                                               true)).Return(null);
            _mocks.ReplayAll();
            var result = _target.RecalculateFoundValues(allValues, 3, true, _person, dateOnly, maxSeatSkillPeriods,
										   TimeSpan.FromHours(8), _options);
            Assert.That(result.Count, Is.EqualTo(0));
           _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldRunOtherFairnessAndMaxSeatCalculators()
        {
            var dateOnly = new DateOnly(2011, 4, 28);
            var maxSeatSkillPeriods = _mocks.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();
            var shiftCache = _mocks.StrictMock<IShiftProjectionCache>();
            var projection = _mocks.StrictMock<IVisualLayerCollection>();
            var allValues = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = 1, ShiftProjection = shiftCache } 
                              };
			_options.Fairness = new Percent(0.25d);
            var dic = _mocks.StrictMock<IScheduleDictionary>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var fairnessValueResult = _mocks.StrictMock<IFairnessValueResult>();
            Expect.Call(_shiftCatFairnessManager.GetFactorsForPersonOnDate(_person, dateOnly)).Return(
                _shiftCategoryFairnessFactors);

            Expect.Call(_stateHolder.ShiftCategories).Return(_shiftCategories);
            Expect.Call(_shiftCategory.MaxOfJusticeValues()).Return(15);
            Expect.Call(_shiftCategoryFairnessFactors.FairnessPointsPerShift).Return(5);
            Expect.Call(_stateHolder.Schedules).Return(dic);
            Expect.Call(dic[_person]).Return(range);
            Expect.Call(range.FairnessPoints()).Return(fairnessValueResult);
            Expect.Call(shiftCache.ShiftCategoryDayOfWeekJusticeValue).Return(5);
			Expect.Call(_fairnessValueCalculator.CalculateFairnessValue(1, 5, 15, 5, fairnessValueResult, 3, _options)).IgnoreArguments().Return(11);

			Expect.Call(shiftCache.MainShiftProjection).Return(projection).Repeat.AtLeastOnce();
            Expect.Call(_seatLimitationWorkShiftCalculator.CalculateShiftValue(_person, projection, maxSeatSkillPeriods,
                                                                               true)).Return(55);
              
            _mocks.ReplayAll();
            var result = _target.RecalculateFoundValues(allValues, 3, false, _person, dateOnly,  maxSeatSkillPeriods,
										   TimeSpan.FromHours(8), _options);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Value, Is.EqualTo(66));
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
																			   true)).Return(55);

			_mocks.ReplayAll();
			var result = _target.RecalculateFoundValues(allValues, 3, false, _person, dateOnly, maxSeatSkillPeriods,
										   TimeSpan.FromHours(8), _options);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].Value, Is.EqualTo(56));
			_mocks.VerifyAll();
		}
    }

    
}