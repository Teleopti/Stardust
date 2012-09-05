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

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_consecutiveWorkDaysValidator = _mock.StrictMock<IDayOffLegalStateValidator>();
			_otherValidator = _mock.StrictMock<IDayOffLegalStateValidator>();
			_validatorListWithoutConsecutiveWorkDaysValidator = new List<IDayOffLegalStateValidator>{_otherValidator};
			_target = new TeDataDayOffDecisionMaker(_validatorListWithoutConsecutiveWorkDaysValidator, _consecutiveWorkDaysValidator, true, new LogWriterForTest());
		}

		[Test]
		public void ShouldReturnFalseIfNotUsed()
		{
			_target = new TeDataDayOffDecisionMaker(_validatorListWithoutConsecutiveWorkDaysValidator, _consecutiveWorkDaysValidator, false, new LogWriterForTest());
			Assert.IsFalse(_target.Execute(null, null));
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