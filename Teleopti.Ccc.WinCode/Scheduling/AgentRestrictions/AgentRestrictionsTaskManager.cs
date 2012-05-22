using System;
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
			if(task == null) throw new ArgumentNullException("task");

			task.Cancel();
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

		public void Cancel()	
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				agentRestrictionsTask.Cancel();
			}		
		}

		public void CancelLowPriority(int priority)
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				if(agentRestrictionsTask.Priority > priority) agentRestrictionsTask.Cancel();
			}
		}

		public void CancelAllExcept(IAgentDisplayData displayData)
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				if (!agentRestrictionsTask.AgentDisplayData.Equals(displayData)) agentRestrictionsTask.Cancel();
			}
		}

		public void Run(IAgentDisplayData displayData)
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				if (agentRestrictionsTask.AgentDisplayData.Equals(displayData)) agentRestrictionsTask.Run();
				break;
			}
		}

		public void Run()
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				agentRestrictionsTask.Run();
			}
		}

		public void RunHighPriority(int priority)
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				if (agentRestrictionsTask.Priority < priority) agentRestrictionsTask.Run();
			}
		}
	}
}
