using System;
using System.Data.SqlClient;
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
			try
			{
				var node = new WorkerNode() { Url = nodeUrl, Id = Guid.NewGuid() };
				_nodeRepository.Add(node);
			}
			catch (SqlException exception)
			{
				if(exception.Message.Contains("UQ_WorkerNodes_Url"))
					return;
				throw;
			}
		}

		public void FreeJobIfAssingedToNode(string url)
		{
			_jobRepository.FreeJobIfNodeIsAssigned(url);
		}
	}
}