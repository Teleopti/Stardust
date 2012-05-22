using System;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsTask
	{
		IAgentDisplayData AgentDisplayData { get; }
		BackgroundWorker Worker { get; }
		int Priority { get; set; }
		void Cancel();
		void Run();
	}

	public class AgentRestrictionsTask : IAgentRestrictionsTask
	{
		public IAgentDisplayData AgentDisplayData { get; private set; }
		public BackgroundWorker Worker { get; private set; }
		public int Priority { get; set; }
		

		public AgentRestrictionsTask(IAgentDisplayData agentDisplayData, BackgroundWorker worker)
		{
			if(worker == null) throw new ArgumentNullException("worker");

			AgentDisplayData = agentDisplayData;
			Worker = worker;
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;
			Priority = 3;
		}

		public void Cancel()
		{
			Worker.CancelAsync();
		}

		public void Run()
		{
			Worker.RunWorkerAsync(AgentDisplayData);
		}
	}
}
