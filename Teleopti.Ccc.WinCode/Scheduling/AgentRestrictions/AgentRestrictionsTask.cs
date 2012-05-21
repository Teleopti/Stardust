using System;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsTask
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
			Priority = 1;
		}
	}
}
