using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_dataTarget = new AgentRestrictionsDisplayRow(_matrix);
		}

		[Test]
		public void VerifyDefaultProperties()
		{
			Assert.AreSame(_matrix, _dataTarget.Matrix);
		}
	}
}
