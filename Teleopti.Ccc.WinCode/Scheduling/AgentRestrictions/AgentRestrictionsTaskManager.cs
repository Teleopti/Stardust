using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsTaskManager
	{
		private readonly IList<IAgentRestrictionsTask> _tasks;
		
		public AgentRestrictionsTaskManager()
		{
			_tasks = new List<IAgentRestrictionsTask>();
		}

		public void Add(IAgentRestrictionsTask task)
		{
			_tasks.Add(task);
		}

		public void Remove(IAgentRestrictionsTask task)
		{
			_tasks.Remove(task);
		}

		public int Count
		{
			get { return _tasks.Count; }
		}

		public void Cancel(IAgentDisplayData displayData)
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				if (!agentRestrictionsTask.AgentDisplayData.Equals(displayData)) continue;
				agentRestrictionsTask.Cancel();
				break;
			}
		}
	}
}
