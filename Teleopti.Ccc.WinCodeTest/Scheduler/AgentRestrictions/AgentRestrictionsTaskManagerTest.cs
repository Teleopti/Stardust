using System;
using System.ComponentModel;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsTaskManagerTest : IDisposable
	{
		private AgentRestrictionsTaskManager _taskManager;
		private IAgentRestrictionsTask _task;
		private IAgentRestrictionsTask _anotherTask;
		private MockRepository _mocks;
		private AgentRestrictionsDisplayRow _displayRow;
		private AgentRestrictionsDisplayRow _anotherDisplayRow;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private BackgroundWorker _worker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_task = _mocks.StrictMock<IAgentRestrictionsTask>();
			_anotherTask = _mocks.StrictMock<IAgentRestrictionsTask>();
			_taskManager = new AgentRestrictionsTaskManager();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_displayRow = new AgentRestrictionsDisplayRow(_scheduleMatrixPro);
			_anotherDisplayRow = new AgentRestrictionsDisplayRow(_scheduleMatrixPro);
			_worker = new BackgroundWorker();
		}

		[Test]
		public void ShouldAddRemoveTask()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _task.Cancel());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				Assert.AreEqual(1, _taskManager.Count);
				_taskManager.Remove(_task);
				Assert.AreEqual(0, _taskManager.Count);	
			}	
		}

		[Test]
		public void ShouldCancelDisplayData()
		{
			using(_mocks.Record())
			{
				Expect.Call(_task.AgentRestrictionsDisplayRow).Return(_displayRow);
				Expect.Call(() => _task.Cancel());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Cancel(_displayRow);	
			}
		}

		[Test]
		public void ShouldCancelAll()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _task.Cancel());
				Expect.Call(() => _anotherTask.Cancel());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Add(_anotherTask);
				_taskManager.Cancel();
			}
		}

		[Test]
		public void ShouldCancelOnPriority()
		{
			using(_mocks.Record())
			{
				Expect.Call(_task.Priority).Return(1);
				Expect.Call(_anotherTask.Priority).Return(3);
				Expect.Call(() => _anotherTask.Cancel());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Add(_anotherTask);
				_taskManager.CancelLowPriority(2);
			}
		}

		[Test]
		public void ShouldCancelAllExcept()
		{
			using(_mocks.Record())
			{
				Expect.Call(_task.AgentRestrictionsDisplayRow).Return(_displayRow);
				Expect.Call(_anotherTask.AgentRestrictionsDisplayRow).Return(_anotherDisplayRow);
				Expect.Call(() => _anotherTask.Cancel());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Add(_anotherTask);
				_taskManager.CancelAllExcept(_displayRow);
			}
		}

		[Test]
		public void ShouldRunDisplayData()
		{
			using(_mocks.Record())
			{
				Expect.Call(_task.AgentRestrictionsDisplayRow).Return(_displayRow);
				Expect.Call(() => _task.Run());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Run(_displayRow);
			}
		}

		[Test]
		public void ShouldRunAll()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _task.Run());
				Expect.Call(() => _anotherTask.Run());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Add(_anotherTask);
				_taskManager.Run();
			}	
		}

		[Test]
		public void ShouldRunOnPriority()
		{
			using (_mocks.Record())
			{
				Expect.Call(_task.Priority).Return(1);
				Expect.Call(_anotherTask.Priority).Return(3);
				Expect.Call(() => _task.Run());
			}

			using (_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Add(_anotherTask);
				_taskManager.RunHighPriority(2);
			}
		}

		[Test]
		public void ShouldGetDisplayRow()
		{
			using(_mocks.Record())
			{
				Expect.Call(_task.Worker).Return(_worker);
				Expect.Call(_task.AgentRestrictionsDisplayRow).Return(_displayRow);
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				var displayRow = _taskManager.GetDisplayRow(_worker);
				Assert.AreEqual(_displayRow, displayRow);
			}		
		}

		[Test]
		public void ShouldGetTask()
		{
			using (_mocks.Record())
			{
				Expect.Call(_task.Worker).Return(_worker);
			}

			using (_mocks.Playback())
			{
				_taskManager.Add(_task);
				var task = _taskManager.GetTask(_worker);
				Assert.AreEqual(_task, task);
			}			
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
