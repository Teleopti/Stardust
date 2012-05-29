﻿using NUnit.Framework;
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
		private IAgentRestrictionsDisplayRowCreator _agentRestrictionsDisplayRowCreator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_agentRestrictionsDisplayRow = new AgentRestrictionsDisplayRow(_scheduleMatrixPro);
			_agentRestrictionsDisplayRowCreator = _mocks.StrictMock<IAgentRestrictionsDisplayRowCreator>();
			_model = new AgentRestrictionsModel();
		}

		[Test]
		public void ShouldReturnDisplayRows()
		{
			Assert.IsNotNull(_model.DisplayRows);	
		}

		[Test]
		public void ShouldLoadDisplayRows()
		{
			using(_mocks.Record())
			{
				Expect.Call(_agentRestrictionsDisplayRowCreator.Create());
			}

			using(_mocks.Playback())
			{
				_model.LoadDisplayRows(_agentRestrictionsDisplayRowCreator);		
			}	
		}

		[Test]
		public void ShouldGetDisplayRowFromIndex()
		{
			_model.DisplayRows.Add(_agentRestrictionsDisplayRow);
			Assert.AreEqual(_agentRestrictionsDisplayRow, _model.DisplayRowFromRowIndex(2));
		}
	}
}
