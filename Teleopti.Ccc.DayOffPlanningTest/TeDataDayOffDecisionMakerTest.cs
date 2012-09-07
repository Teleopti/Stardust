using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Te"), TestFixture]
	public class TeDataDayOffDecisionMakerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private MockRepository _mock;
		private IDayOffDecisionMaker _target;
		private IDayOffLegalStateValidator _consecutiveWorkDaysValidator;
		private IDayOffLegalStateValidator _otherValidator;
		private IList<IDayOffLegalStateValidator> _validatorListWithoutConsecutiveWorkDaysValidator;
		//private IDayOffBackToLegalStateFunctions _functions;
		private ILockableBitArray _array;
		private IList<double?> _values;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_consecutiveWorkDaysValidator = _mock.StrictMock<IDayOffLegalStateValidator>();
			_otherValidator = _mock.StrictMock<IDayOffLegalStateValidator>();
			_validatorListWithoutConsecutiveWorkDaysValidator = new List<IDayOffLegalStateValidator> {_otherValidator};
			_array = new LockableBitArray(14, false, false, null);
			_array.Set(5, true);
			_array.Set(6, true);
			_array.Set(12, true);
			_array.Set(13, true);
			//_functions = new DayOffBackToLegalStateFunctions(_array);
			_target = new TeDataDayOffDecisionMaker(_validatorListWithoutConsecutiveWorkDaysValidator,
			                                        _consecutiveWorkDaysValidator, true, new LogWriterForTest());
			_values = new List<double?> {0, 0, 1, 1, 0, -1, -1, 0, 0, 0.5, 0.5, 0, -1, -1};
		}

		[Test]
		public void ShouldReturnFalseIfNotUsed()
		{
			_target = new TeDataDayOffDecisionMaker(_validatorListWithoutConsecutiveWorkDaysValidator, _consecutiveWorkDaysValidator, false, new LogWriterForTest());
			Assert.IsFalse(_target.Execute(null, null));
		}

		[Test]
		public void ShouldSolveTheCase()
		{
			using (_mock.Record())
			{
				Expect.Call(_otherValidator.IsValid(null, 5)).IgnoreArguments().Return(true).Repeat.Times(4);
				Expect.Call(_consecutiveWorkDaysValidator.IsValid(null, 5)).IgnoreArguments().Return(false);
				Expect.Call(_otherValidator.IsValid(null, 5)).IgnoreArguments().Return(true).Repeat.Times(4);
			}
			bool result;
			using(_mock.Playback())
			{
				result = _target.Execute(_array, _values);
			}
			
			Assert.IsTrue(result);
			Assert.AreEqual("Teleopti.Ccc.DayOffPlanning.LockableBitArray 00110000011000", _array.ToString());
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