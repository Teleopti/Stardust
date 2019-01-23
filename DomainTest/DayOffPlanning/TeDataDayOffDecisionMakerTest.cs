using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class TeDataDayOffDecisionMakerTest
	{
		private IDayOffDecisionMaker _target;
		private IDayOffLegalStateValidator _consecutiveWorkDaysValidator;
		private IDayOffLegalStateValidator _otherValidator;
		private IList<IDayOffLegalStateValidator> _validatorList;
		private ILockableBitArray _array;
		private IList<double?> _values;

		[SetUp]
		public void Setup()
		{
			_consecutiveWorkDaysValidator = new ConsecutiveWorkdayValidator(new MinMax<int>(1, 5), false, false);
			_otherValidator = new WeeklyDayOffValidator(new MinMax<int>(2, 2));
			_validatorList = new List<IDayOffLegalStateValidator> { _otherValidator, _consecutiveWorkDaysValidator };
			_target = new TeDataDayOffDecisionMaker(_validatorList, true, new LogWriterForTest());
		}

		[Test]
		public void ShouldReturnFalseIfNotUsed()
		{
			_target = new TeDataDayOffDecisionMaker(_validatorList, false, new LogWriterForTest());
			Assert.IsFalse(_target.Execute(null, null));
		}

		[Test]
		public void ShouldSolveTheCaseWithTwoWeeks()
		{
			_array = new LockableBitArray(14, false, false);
			_array.Set(5, true);
			_array.Set(6, true);
			_array.Set(12, true);
			_array.Set(13, true);
			_values = new List<double?> {0, 0, 1, 1, 0, -1, -1, 0, 0, 0.5, 0.5, 0, -1, -1};

			bool result = _target.Execute(_array, _values);

			Assert.IsTrue(result);
			Assert.AreEqual("Teleopti.Ccc.Domain.DayOffPlanning.LockableBitArray 00110000011000", _array.ToString());
		}

		[Test]
		public void ShouldSolveTheCaseWithMoreThanTwoWeeksAndHandleLocks()
		{
			_array = new LockableBitArray(21, false, false);
			_array.Set(5, true);
			_array.Set(6, true);
			_array.Set(12, true);
			_array.Set(13, true);
			_array.Lock(14, true);
			_array.Lock(15, true);
			_array.Set(19, true);
			_array.Set(20, true);
			_values = new List<double?> { 0, 0, 1, 1, 0, -1, -1, 0, 0, 0.5, 0.5, 0, -1, -1, 1, 1, 0.5, 0, 0, -1, -1 };

			bool result = _target.Execute(_array, _values);

			Assert.IsTrue(result);
			Assert.AreEqual("Teleopti.Ccc.Domain.DayOffPlanning.LockableBitArray 001100000110000011000", _array.ToString());
		}

		[Test]
		public void ShouldReturnToWeekIndexZeroIfRequired()
		{
			_array = new LockableBitArray(28, false, false);
			_array.Set(5, true);
			_array.Set(6, true);
			_array.Set(12, true);
			_array.Set(13, true);
			_array.Set(19, true);
			_array.Set(20, true);
			_array.Set(26, true);
			_array.Set(27, true);

			_values = new List<double?> { -0.21, -0.06, -0.14, -0.2, -0.08, -0.45, -0.18, 
											-0.21, -0.09, -0,17, -0.17, -0.08, -0.45, -0.05, 
											-0.21, -0.13, -0.20, -0.17, -0.08, -0.37, 0.05
											-0.21, -0.13, -0.20, -0.22, -0.15, -0.21, 0.30};

			bool result = _target.Execute(_array, _values);

			Assert.IsTrue(result);
			Assert.AreEqual("Teleopti.Ccc.Domain.DayOffPlanning.LockableBitArray 0110000011000011000001100000", _array.ToString());
		}
	}

	public class LogWriterForTest : ILogWriter
	{
		public void LogInfo(Func<FormattableString> message)
		{
			//do nothing
		}
	}
}