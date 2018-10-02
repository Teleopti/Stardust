using System;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class NodeManager
	{
		private readonly IJobRepository _jobRepository;
		private readonly IWorkerNodeRepository _nodeRepository;

		public NodeManager(IWorkerNodeRepository nodeRepository, IJobRepository jobRepository)
		{
			_nodeRepository = nodeRepository;
			_jobRepository = jobRepository;
		}

		public void AddWorkerNode(Uri workerNodeUri)
		{
			var node = new WorkerNode
			{
				Url = workerNodeUri
			};

			_nodeRepository.AddWorkerNode(node);
		}


		public void WorkerNodeRegisterHeartbeat(string nodeUri)
		{
			_nodeRepository.RegisterHeartbeat(nodeUri, true);
		}

		public void RequeueJobsThatDidNotFinishedByWorkerNodeUri(string workerNodeUri)
		{
			_jobRepository.RequeueJobThatDidNotEndByWorkerNodeUri(workerNodeUri);
		}
	}
}