using System;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsTaskTest : IDisposable
	{
		private AgentRestrictionsTask _task;
		private IAgentDisplayData _agentDisplayData;
		private MockRepository _mocks;
		private BackgroundWorker _worker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_agentDisplayData = _mocks.StrictMock<IAgentDisplayData>();
			_worker = new BackgroundWorker();
			_task = new AgentRestrictionsTask(_agentDisplayData, _worker);
		}


		[Test]
		public void ShouldInit()
		{
			Assert.AreEqual(_agentDisplayData, _task.AgentDisplayData);
			var worker = _task.Worker;
			Assert.IsNotNull(worker);
			Assert.IsTrue(worker.WorkerReportsProgress);
			Assert.IsNotNull(worker.WorkerSupportsCancellation);
			Assert.AreEqual(1, _task.Priority);
		}

		[Test]
		public void ShouldCancel()
		{
			_task.Cancel();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_worker.Dispose();
			}
		}
	}
}
