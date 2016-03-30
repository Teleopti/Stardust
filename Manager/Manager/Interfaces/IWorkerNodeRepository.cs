using System;
using System.Collections.Generic;
using Stardust.Manager.Models;

namespace Stardust.Manager.Interfaces
{
	public interface IWorkerNodeRepository
	{
		List<WorkerNode> LoadAll();

		void Add(WorkerNode workerNode);

        void DeleteNode(Guid nodeId);

		WorkerNode LoadWorkerNode(Uri nodeUri);
		void RegisterHeartbeat(string nodeUri, bool updateStatus);

		List<string> CheckNodesAreAlive(TimeSpan timeSpan);
	}
}