using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class MaxMovedDaysOverLimitValidatorTest
	{
		private MockRepository _mocks;
		private IMaxMovedDaysOverLimitValidator _target;
		private IDictionary<IPerson, IScheduleRange> _allSelectedScheduleRangeClones;
		private IScheduleDayEquator _scheduleDayEquator;
		private IOptimizationPreferences _optimizationPreferences;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new MaxMovedDaysOverLimitValidator(_allSelectedScheduleRangeClones, _scheduleDayEquator);
			_optimizationPreferences = new OptimizationPreferences();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
		}

		[Test]
		public void ShouldReturnTrueIfMoveMaxIsNotUsed()
		{

			_optimizationPreferences.Shifts.KeepShifts = false;
			_optimizationPreferences.DaysOff.UseKeepExistingDaysOff = false;
			bool result = _target.ValidateMatrix(_matrix, _optimizationPreferences);
			Assert.IsTrue(result);
		}

	}
}