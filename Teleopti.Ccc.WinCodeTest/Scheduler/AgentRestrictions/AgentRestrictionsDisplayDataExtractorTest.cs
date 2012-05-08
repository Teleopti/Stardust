using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDisplayDataExtractorTest
	{
		private MockRepository _mocks;
		private IAgentRestrictionsDisplayDataExtractor _target;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_target = new AgentRestrictionsDisplayDataExtractor();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
		}

		[Test]
		public void ShouldExctractToDisplayData()
		{
			IAgentDisplayData data = new AgentRestrictionsDisplayRow(_matrix);
			_target.ExtractTo(data);
		}
	}
}