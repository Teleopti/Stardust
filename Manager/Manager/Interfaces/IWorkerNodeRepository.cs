using System;
using System.Collections.Generic;
using Stardust.Manager.Models;

namespace Stardust.Manager.Interfaces
{
	public interface IWorkerNodeRepository
	{
		List<WorkerNode> GetAllWorkerNodes();

		void AddWorkerNode(WorkerNode workerNode);

		void RegisterHeartbeat(string nodeUri, bool updateStatus);

		List<string> CheckNodesAreAlive(TimeSpan timeSpan);
	}
}