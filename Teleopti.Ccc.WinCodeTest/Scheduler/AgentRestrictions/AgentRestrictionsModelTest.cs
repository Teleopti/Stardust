using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsModelTest
	{
		private AgentRestrictionsModel _model;

		[SetUp]
		public void Setup()
		{
			_model = new AgentRestrictionsModel();
		}

		[Test]
		public void ShouldReturnDisplayRows()
		{
			Assert.IsNotNull(_model.DisplayRows);	
		}
	}
}
