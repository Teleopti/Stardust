using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsTaskManagerTest
	{
		private AgentRestrictionsTaskManager _taskManager;
		private IAgentRestrictionsTask _task;
		private IAgentRestrictionsTask _anotherTask;
		private MockRepository _mocks;
		private IAgentDisplayData _displayData;
		private IAgentDisplayData _anotherDisplayData;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_task = _mocks.StrictMock<IAgentRestrictionsTask>();
			_anotherTask = _mocks.StrictMock<IAgentRestrictionsTask>();
			_taskManager = new AgentRestrictionsTaskManager();
			_displayData = _mocks.StrictMock<IAgentDisplayData>();
			_anotherDisplayData = _mocks.StrictMock<IAgentDisplayData>();
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
				Expect.Call(_task.AgentDisplayData).Return(_displayData);
				Expect.Call(() => _task.Cancel());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Cancel(_displayData);	
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
				Expect.Call(_task.AgentDisplayData).Return(_displayData);
				Expect.Call(_anotherTask.AgentDisplayData).Return(_anotherDisplayData);
				Expect.Call(() => _anotherTask.Cancel());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Add(_anotherTask);
				_taskManager.CancelAllExcept(_displayData);
			}
		}

		[Test]
		public void ShouldRunDisplayData()
		{
			using(_mocks.Record())
			{
				Expect.Call(_task.AgentDisplayData).Return(_displayData);
				Expect.Call(() => _task.Run());
			}

			using(_mocks.Playback())
			{
				_taskManager.Add(_task);
				_taskManager.Run(_displayData);
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
	}
}
