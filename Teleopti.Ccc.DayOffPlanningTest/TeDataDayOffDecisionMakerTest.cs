﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Te"), TestFixture]
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
			_array = new LockableBitArray(14, false, false, null);
			_array.Set(5, true);
			_array.Set(6, true);
			_array.Set(12, true);
			_array.Set(13, true);
			_values = new List<double?> {0, 0, 1, 1, 0, -1, -1, 0, 0, 0.5, 0.5, 0, -1, -1};

			bool result = _target.Execute(_array, _values);

			Assert.IsTrue(result);
			Assert.AreEqual("Teleopti.Ccc.DayOffPlanning.LockableBitArray 00110000011000", _array.ToString());
		}

		[Test]
		public void ShouldSolveTheCaseWithMoreThanTwoWeeksAndHandleLocks()
		{
			_array = new LockableBitArray(21, false, false, null);
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
			Assert.AreEqual("Teleopti.Ccc.DayOffPlanning.LockableBitArray 001100000110000011000", _array.ToString());
		}
	}

	public class LogWriterForTest : ILogWriter
	{
		public void LogInfo(string message)
		{
			//do nothing
		}
	}
}