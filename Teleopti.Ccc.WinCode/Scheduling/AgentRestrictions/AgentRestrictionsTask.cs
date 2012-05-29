using System;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsTask
	{
		AgentRestrictionsDisplayRow AgentRestrictionsDisplayRow { get; }
		BackgroundWorker Worker { get; }
		int Priority { get; set; }
		void Cancel();
		void Run();
	}

	public class AgentRestrictionsTask : IAgentRestrictionsTask
	{
		public AgentRestrictionsDisplayRow AgentRestrictionsDisplayRow { get; private set; }
		public BackgroundWorker Worker { get; private set; }
		public int Priority { get; set; }
		

		public AgentRestrictionsTask(AgentRestrictionsDisplayRow agentDisplayData, BackgroundWorker worker)
		{
			if(worker == null) throw new ArgumentNullException("worker");

			AgentRestrictionsDisplayRow = agentDisplayData;
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
			Worker.RunWorkerAsync(AgentRestrictionsDisplayRow);
		}
	}
}
