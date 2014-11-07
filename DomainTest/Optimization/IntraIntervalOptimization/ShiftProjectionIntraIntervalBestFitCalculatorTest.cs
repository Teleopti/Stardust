using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	public class ShiftProjectionIntraIntervalBestFitCalculatorTest
	{
		private ShiftProjectionIntraIntervalBestFitCalculator _target;
		private MockRepository _mock;
		private ISkillStaffPeriodIntraIntervalPeriodFinder _skillStaffPeriodIntraIntervalPeriodFinder;
		private ISkillActivityCounter _skillActivityCounter;
		private IShiftProjectionCacheIntraIntervalValueCalculator _shiftProjectionCacheIntraIntervalValueCalculator;
		private IList<IWorkShiftCalculationResultHolder> _sortedListResources;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private DateTimePeriod _period1;
		private DateTimePeriod _period2;
		private ISkill _skill;
		private IWorkShiftCalculationResultHolder _workShiftCalculationResultHolder1;
		private IWorkShiftCalculationResultHolder _workShiftCalculationResultHolder2;
		private IShiftProjectionCache _shiftProjectionCache1;
		private IShiftProjectionCache _shiftProjectionCache2;
		private IList<int> _samplesBefore;
		
		[SetUp]
		public void SetUp()
		{
			_period1 = new DateTimePeriod(2014, 1, 1, 8, 2014, 1, 1, 10);
			_period2 = new DateTimePeriod(2014, 1, 1, 15, 2014, 1, 1, 16);

			_mock = new MockRepository();
			_skillStaffPeriodIntraIntervalPeriodFinder = _mock.StrictMock<ISkillStaffPeriodIntraIntervalPeriodFinder>();
			_skillActivityCounter = _mock.StrictMock<ISkillActivityCounter>();
			_shiftProjectionCacheIntraIntervalValueCalculator =_mock.StrictMock<IShiftProjectionCacheIntraIntervalValueCalculator>();

			_workShiftCalculationResultHolder1 = new WorkShiftCalculationResult();
			_workShiftCalculationResultHolder2 = new WorkShiftCalculationResult();
			_shiftProjectionCache1 = _mock.StrictMock<IShiftProjectionCache>();
			_shiftProjectionCache2 = _mock.StrictMock<IShiftProjectionCache>();

			_workShiftCalculationResultHolder1.ShiftProjection = _shiftProjectionCache1;
			_workShiftCalculationResultHolder2.ShiftProjection = _shiftProjectionCache2;
			_workShiftCalculationResultHolder1.Value = 1;
			_workShiftCalculationResultHolder2.Value = 2;

			_samplesBefore = new List<int>();

			_sortedListResources = new List<IWorkShiftCalculationResultHolder>
			{
				_workShiftCalculationResultHolder1,
				_workShiftCalculationResultHolder2
			};

			_skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period1, new Task(), new ServiceAgreement());
			_skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_period2, new Task(), new ServiceAgreement());

			_skillStaffPeriod1.IntraIntervalSamples = _samplesBefore;
			_skillStaffPeriod2.IntraIntervalSamples = _samplesBefore;

			_skill = SkillFactory.CreateSkill("skill");
			_target = new ShiftProjectionIntraIntervalBestFitCalculator(_skillStaffPeriodIntraIntervalPeriodFinder, _skillActivityCounter, _shiftProjectionCacheIntraIntervalValueCalculator);
		}

		[Test]
		public void ShouldCalculate()
		{
			var affectedPeriods = new List<DateTimePeriod> {_period1};
			var samples = new List<int>();

			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriodIntraIntervalPeriodFinder.Find(_period1, _shiftProjectionCache1, _skill)).Return(affectedPeriods);
				Expect.Call(_skillActivityCounter.Count(affectedPeriods, _period1)).Return(samples);
				Expect.Call(_shiftProjectionCacheIntraIntervalValueCalculator.Calculate(_samplesBefore, samples)).Return(20d);

				Expect.Call(_skillStaffPeriodIntraIntervalPeriodFinder.Find(_period1, _shiftProjectionCache2, _skill)).Return(affectedPeriods);
				Expect.Call(_skillActivityCounter.Count(affectedPeriods, _period1)).Return(samples);
				Expect.Call(_shiftProjectionCacheIntraIntervalValueCalculator.Calculate(_samplesBefore, samples)).Return(20d);


				Expect.Call(_skillStaffPeriodIntraIntervalPeriodFinder.Find(_period2, _shiftProjectionCache1, _skill)).Return(affectedPeriods);
				Expect.Call(_skillActivityCounter.Count(affectedPeriods, _period2)).Return(samples);
				Expect.Call(_shiftProjectionCacheIntraIntervalValueCalculator.Calculate(_samplesBefore, samples)).Return(10d);

				Expect.Call(_skillStaffPeriodIntraIntervalPeriodFinder.Find(_period2, _shiftProjectionCache2, _skill)).Return(affectedPeriods);
				Expect.Call(_skillActivityCounter.Count(affectedPeriods, _period2)).Return(samples);
				Expect.Call(_shiftProjectionCacheIntraIntervalValueCalculator.Calculate(_samplesBefore, samples)).Return(10d);
			}

			using (_mock.Playback())
			{
				var result = _target.GetShiftProjectionCachesSortedByBestIntraIntervalFit(_sortedListResources, new List<ISkillStaffPeriod> {_skillStaffPeriod1, _skillStaffPeriod2}, _skill);
				Assert.AreEqual(2, result.Count);

				var first = result.First() as WorkShiftCalculationResult;
				var last = result.Last() as WorkShiftCalculationResult;

				Assert.AreEqual(first.Value, 20d);
				Assert.AreEqual(last.Value, 40d);
			}
		}
	}
}
