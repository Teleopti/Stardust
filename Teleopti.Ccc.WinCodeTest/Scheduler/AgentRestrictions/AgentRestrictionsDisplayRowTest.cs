using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDisplayRowTest
	{
		private IAgentDisplayData _dataTarget;
		private AgentRestrictionsDisplayRow _displayRow;
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_dataTarget = new AgentRestrictionsDisplayRow(_matrix);
			_displayRow = new AgentRestrictionsDisplayRow(_matrix);	
		}

		[Test]
		public void VerifyDefaultProperties()
		{
			Assert.AreSame(_matrix, _dataTarget.Matrix);
			Assert.AreEqual(AgentRestrictionDisplayRowState.NotAvailable, _displayRow.State);	
		}

		[Test]
		public void ShouldGetSetState()
		{
			_displayRow.State = AgentRestrictionDisplayRowState.Loading;
			Assert.AreEqual(AgentRestrictionDisplayRowState.Loading, _displayRow.State);
		}

		[Test]
		public void ShouldGetSetAgentName()
		{
			_displayRow.AgentName = "AgentName";
			Assert.AreEqual("AgentName", _displayRow.AgentName);
		}

		[Test]
		public void ShouldGetSetWarnings()
		{
			Assert.AreEqual(0, _displayRow.Warnings);
			_displayRow.SetWarning(AgentRestrictionDisplayRowColumn.ContractTargetTime, "warning");
			Assert.AreEqual(1, _displayRow.Warnings);
			Assert.AreEqual("warning", _displayRow.Warning(5));
			Assert.AreEqual(null, _displayRow.Warning(1));	
		}
	}
}
