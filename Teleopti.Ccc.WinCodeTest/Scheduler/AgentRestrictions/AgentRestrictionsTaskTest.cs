using System;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsTaskTest : IDisposable
	{
		private AgentRestrictionsTask _task;
		private AgentRestrictionsDisplayRow _agentRestrictionsDisplayRow;
		private MockRepository _mocks;
		private BackgroundWorker _worker;
		private IScheduleMatrixPro _scheduleMatrixPro;
		//private bool _doWork;
		//private bool _completed;
		//private bool _cancel;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			//_agentDisplayData = _mocks.StrictMock<IAgentDisplayData>();
			_agentRestrictionsDisplayRow = new AgentRestrictionsDisplayRow(_scheduleMatrixPro);
			_worker = new BackgroundWorker();
			_task = new AgentRestrictionsTask(_agentRestrictionsDisplayRow, _worker);
			//_worker.DoWork += WorkerDoWork;
			//_worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
			//_doWork = false;
			//_completed = false;
			//_cancel = false;
		}

		//void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		//{
		//    if (e.Cancelled) _cancel = true;
		//    _completed = true;
		//}

		//void WorkerDoWork(object sender, DoWorkEventArgs e)
		//{
		//    if (((BackgroundWorker)sender).CancellationPending) e.Cancel = true;
		//    _doWork = true;
		//}


		[Test]
		public void ShouldInit()
		{
			Assert.AreEqual(_agentRestrictionsDisplayRow, _task.AgentRestrictionsDisplayRow);
			var worker = _task.Worker;
			Assert.IsNotNull(worker);
			Assert.IsTrue(worker.WorkerReportsProgress);
			Assert.IsNotNull(worker.WorkerSupportsCancellation);
			Assert.AreEqual(3, _task.Priority);
		}

		//[Test]
		//public void ShouldCancel()
		//{
		//    _task.Run();
		//    _task.Cancel();

		//    while(_completed == false)
		//    {
		//        //do nothing
		//    }

		//    Assert.IsTrue(_cancel);
		//}

		//[Test]
		//public void ShouldRun()
		//{
		//    _task.Run();

		//    while(_completed == false)
		//    {
		//        //do nothing	
		//    }

		//    Assert.IsTrue(_doWork);
		//}

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
