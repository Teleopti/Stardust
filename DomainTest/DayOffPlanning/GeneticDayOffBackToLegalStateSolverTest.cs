using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class GeneticDayOffBackToLegalStateSolverTest
	{
		private MockRepository _mocks;
		private IGeneticDayOffBackToLegalStateSolver _target;
		private DayOffOptimizationLegalStateValidatorListCreator _dayOffOptimizationLegalStateValidatorListCreator;
		private IDaysOffPreferences _daysOffPreferences;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new GeneticDayOffBackToLegalStateSolver();
			_daysOffPreferences = new DaysOffPreferences();
			
			_dayOffOptimizationLegalStateValidatorListCreator = new DayOffOptimizationLegalStateValidatorListCreator(_daysOffPreferences, new OfficialWeekendDays(), new BitArray(28+14), new MinMax<int>(7,28+7));
		}

		[Test]
		public void ShouldWorkForThis()
		{
			_daysOffPreferences.UseDaysOffPerWeek = true;
			_daysOffPreferences.DaysOffPerWeekValue = new MinMax<int>(1, 2);
			_daysOffPreferences.UseConsecutiveDaysOff = true;
			_daysOffPreferences.ConsecutiveDaysOffValue = new MinMax<int>(1, 3);
			_daysOffPreferences.UseConsecutiveWorkdays = true;
			_daysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 5);
			_daysOffPreferences.UseFullWeekendsOff = true;
			_daysOffPreferences.FullWeekendsOffValue = new MinMax<int>(3, 3);
			_daysOffPreferences.UseWeekEndDaysOff = true;
			_daysOffPreferences.WeekEndDaysOffValue = new MinMax<int>(6, 6);
			var validators = _dayOffOptimizationLegalStateValidatorListCreator.BuildActiveValidatorList();
			var result = _target.Execute(28, 8, validators);
			Assert.That(result.Count > 0);
			Debug.Print(result.Count.ToString());
		}

		[Test]
		public void xx()
		{
			var i1 = new DayOffArray(6);
			i1.Set(0, true);
			i1.Set(2, true);
			i1.Set(4, true);

			var i2 = new DayOffArray(6);
			i2.Set(1, true);
			i2.Set(3, true);
			i2.Set(5, true);

			var i3 = new DayOffArray(6);
			i3.Set(0, true);
			i3.Set(2, true);
			i3.Set(4, true);

			Debug.Print(i1.GetHashCode().ToString());
			Debug.Print(i2.GetHashCode().ToString());
			Debug.Print(i3.GetHashCode().ToString());

			Assert.IsTrue(i1.Equals(i3));

			var l = new HashSet<DayOffArray> { i1, i2, i3 };
			Assert.AreEqual(2, l.Count);
			
		}
	}
}