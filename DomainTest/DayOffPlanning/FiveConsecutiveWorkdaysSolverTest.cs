using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Secrets.DayOffPlanning;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class FiveConsecutiveWorkdaysSolverTest
	{
		private IDayOffBackToLegalStateSolver _target;
		private IDayOffBackToLegalStateFunctions _functions;
		private LockableBitArray _bitArray;
		private IDaysOffPreferences _datDaysOffPreferences;

		[SetUp]
		public void Setup()
		{
			_bitArray = array1();
			_functions = new DayOffBackToLegalStateFunctions(_bitArray);
			_datDaysOffPreferences = new DaysOffPreferences();
			_target = new FiveConsecutiveWorkdaysSolver(_bitArray, _functions, _datDaysOffPreferences);
		}

		[Test]
		public void VerifyIsInLegalState()
		{
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 16);
			Assert.AreEqual(MinMaxNumberOfResult.Ok, _target.ResolvableState());
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
			Assert.AreEqual(MinMaxNumberOfResult.ToFew, _target.ResolvableState());
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 6);
			Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
		}

		[Test]
		public void VerifyIsInLegalStateWhenPeriodEndsWithLongestBlock()
		{
			_bitArray.Set(27, false);
			_bitArray.Set(11, true);
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 5);
			Assert.AreEqual(MinMaxNumberOfResult.ToMany, _target.ResolvableState());
		}

		[Test]
		public void VerifySetToManyBackToLegalState()
		{
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 5);
			Assert.IsTrue(_target.SetToManyBackToLegalState());
			Assert.IsFalse(_target.SetToManyBackToLegalState());
		}

		[Test]
		public void VerifySetToManyBackToLegalStateWhenMiddleBitOfLongestBlockIsLocked()
		{
			_bitArray.Lock(18, true);
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 5);
			Assert.IsTrue(_target.SetToManyBackToLegalState());
		}

		[Test]
		public void VerifySetToManyBackToLegalStateWhenNoDaysOff()
		{
			_bitArray.SetAll(false);
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 5);
			Assert.IsFalse(_target.SetToManyBackToLegalState());
		}

		[Test]
		public void VerifySetToManyBackToLegalStateOutOfIterations()
		{
			_target = new FiveConsecutiveWorkdaysSolver(_bitArray, _functions, _datDaysOffPreferences);
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(1, 5);
			Assert.IsTrue(_target.SetToManyBackToLegalState());
		}

		[Test]
		public void VerifySetToFewBackToLegalState()
		{
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 16);
			Assert.IsTrue(_target.SetToFewBackToLegalState());
		}

		[Test]
		public void ShouldHandleSpecialCaseWhenMaxFiveWorkdaysAndBadInitialDaysOff()
		{
			LockableBitArray ret = new LockableBitArray(21, true, true, null);
			ret.SetAll(false);
			ret.Set(1, true);
			ret.Set(12, true);
			ret.Set(13, true);
			ret.Set(19, true);
			_datDaysOffPreferences.ConsecutiveWorkdaysValue = new MinMax<int>(2, 5);
			_datDaysOffPreferences.ConsiderWeekBefore = true;
			_datDaysOffPreferences.ConsiderWeekAfter = true;
			_target = new FiveConsecutiveWorkdaysSolver(ret, _functions, _datDaysOffPreferences);
			_target.SetToManyBackToLegalState();
			Assert.IsTrue(ret.DaysOffBitArray[7]);

		}

		private static LockableBitArray array1()
		{
			LockableBitArray ret = new LockableBitArray(28, true, true, null);
			ret.SetAll(false);
			ret.Set(0, true);
			ret.Set(1, true);
			ret.Set(5, true);//sa
			ret.Set(6, true);//su
			ret.Set(7, true);
			ret.Set(9, true);
			ret.Set(10, true);
			ret.Set(27, true);//su

			return ret;
		}
	}
}