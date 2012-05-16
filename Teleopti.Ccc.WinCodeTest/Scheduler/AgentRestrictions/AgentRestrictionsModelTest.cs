using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsModelTest
	{
		private AgentRestrictionsModel _model;
		private AgentRestrictionsDisplayRow _agentRestrictionsDisplayRow;
		private MockRepository _mocks;
		private IScheduleMatrixPro _scheduleMatrixPro;

		[SetUp]
		public void Setup()
		{
			_model = new AgentRestrictionsModel();
			_mocks = new MockRepository();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_agentRestrictionsDisplayRow = new AgentRestrictionsDisplayRow(_scheduleMatrixPro);
		}

		[Test]
		public void ShouldReturnDisplayRows()
		{
			Assert.IsNotNull(_model.DisplayRows);	
		}

		[Test]
		public void ShouldLoadData()
		{
			_model.LoadData();
		}

		[Test]
		public void ShouldGetDisplayRowFromIndex()
		{
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow);
			Assert.AreEqual(_agentRestrictionsDisplayRow, _model.DisplayRowFromRowIndex(2));
		}
	}
}
