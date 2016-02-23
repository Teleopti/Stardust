using System;
using System.Collections.Generic;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest.Fakes
{
	public class FakeWorkerNodeRepository : IWorkerNodeRepository
	{
		private readonly List<WorkerNode> _workerNodes = new List<WorkerNode>();

		public List<WorkerNode> LoadAll()
		{
			return _workerNodes;
		}

		public void Add(WorkerNode workerNode)
		{
			_workerNodes.Add(workerNode);
		}

		public void DeleteNode(Guid nodeId)
		{
			throw new NotImplementedException();
		}

		public List<WorkerNode> LoadAllFreeNodes()
		{
			throw new NotImplementedException();
		}
	}
}