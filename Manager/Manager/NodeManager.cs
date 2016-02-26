using System;
using System.Data.SqlClient;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class NodeManager : INodeManager
	{
		private readonly IJobRepository _jobRepository;
		private readonly IWorkerNodeRepository _nodeRepository;

		public NodeManager(IWorkerNodeRepository nodeRepository, IJobRepository jobRepository)
		{
			_nodeRepository = nodeRepository;
			_jobRepository = jobRepository;
		}

		public void AddIfNeeded(Uri nodeUrl)
		{
			try
			{
				var node = new WorkerNode
				{
					Url = nodeUrl
				};
				_nodeRepository.Add(node);
			}
			catch (SqlException exception)
			{
				if (exception.Message.Contains("UQ_WorkerNodes_Url"))
					return;
				throw;
			}
		}

		public void FreeJobIfAssingedToNode(Uri url)
		{
			_jobRepository.FreeJobIfNodeIsAssigned(url.ToString());
		}

	}
}