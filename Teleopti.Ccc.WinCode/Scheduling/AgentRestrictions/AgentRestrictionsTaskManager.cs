using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsTaskManager
	{
		void Add(IAgentRestrictionsTask task);
		void Remove(IAgentRestrictionsTask task);
		int Count { get; }
		void Cancel(AgentRestrictionsDisplayRow displayData);
		void Cancel();
		void CancelLowPriority(int priority);
		void CancelAllExcept(AgentRestrictionsDisplayRow displayData);
		void Run(AgentRestrictionsDisplayRow displayData);
		void Run();
		void RunHighPriority(int priority);
		AgentRestrictionsDisplayRow GetDisplayRow(BackgroundWorker worker);
		IAgentRestrictionsTask GetTask(BackgroundWorker worker);
	}

	public class AgentRestrictionsTaskManager : IAgentRestrictionsTaskManager
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

		public void Cancel(AgentRestrictionsDisplayRow displayData)
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				if (!agentRestrictionsTask.AgentRestrictionsDisplayRow.Equals(displayData)) continue;
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

		public void CancelAllExcept(AgentRestrictionsDisplayRow displayData)
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				if (!agentRestrictionsTask.AgentRestrictionsDisplayRow.Equals(displayData)) agentRestrictionsTask.Cancel();
			}
		}

		public void Run(AgentRestrictionsDisplayRow displayData)
		{
			foreach (var agentRestrictionsTask in _tasks)
			{
				if (agentRestrictionsTask.AgentRestrictionsDisplayRow.Equals(displayData)) agentRestrictionsTask.Run();
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

		public AgentRestrictionsDisplayRow GetDisplayRow(BackgroundWorker worker)
		{
			return (from task in _tasks where task.Worker.Equals(worker) select task.AgentRestrictionsDisplayRow).FirstOrDefault();	
		}

		public IAgentRestrictionsTask GetTask(BackgroundWorker worker)
		{
			return (from task in _tasks where task.Worker.Equals(worker) select task).FirstOrDefault();	
		}
	}
}
