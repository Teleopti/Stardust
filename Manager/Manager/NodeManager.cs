using System;
using System.Linq;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class NodeManager : INodeManager
	{
		private readonly IWorkerNodeRepository _nodeRepository;
		private readonly IJobRepository _jobRepository;

		public NodeManager(IWorkerNodeRepository nodeRepository, IJobRepository jobRepository)
		{
			_nodeRepository = nodeRepository;
			_jobRepository = jobRepository;
		}

		public void AddIfNeeded(string nodeUrl)
		{

			var existingNodes = _nodeRepository.LoadAll().Where(x => x.Url == nodeUrl);
			if (!existingNodes.Any())
			{
				var node = new WorkerNode() { Url = nodeUrl, Id = Guid.NewGuid()};
				_nodeRepository.Add(node);
			} 
		}

		public void FreeJobIfAssingedToNode(string url)
		{
			_jobRepository.FreeJobIfNodeIsAssigned(url);
		}

		
	}
}