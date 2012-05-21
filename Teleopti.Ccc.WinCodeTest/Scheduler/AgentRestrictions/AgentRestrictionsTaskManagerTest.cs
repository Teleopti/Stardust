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
		private MockRepository _mocks;
		private IAgentDisplayData _displayData;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_task = _mocks.StrictMock<IAgentRestrictionsTask>();
			_taskManager = new AgentRestrictionsTaskManager();
			_displayData = _mocks.StrictMock<IAgentDisplayData>();
		}

		[Test]
		public void ShouldAddRemoveTask()
		{
			_taskManager.Add(_task);
			Assert.AreEqual(1, _taskManager.Count);
			_taskManager.Remove(_task);
			Assert.AreEqual(0, _taskManager.Count);
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
	}
}
