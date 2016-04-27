using System;
using System.Data.SqlClient;
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
			try
			{
				var node = new WorkerNode
				{
					Url = workerNodeUri
				};

				_nodeRepository.AddWorkerNode(node);
			}

			catch (SqlException exception)
			{
				if (exception.Message.Contains("UQ_WorkerNodes_Url"))
				{
					return;
				}
					
				throw;
			}
		}


		public void WorkerNodeRegisterHeartbeat(string nodeUri)
		{
			_nodeRepository.RegisterHeartbeat(nodeUri, true);
		}

		public void RequeueJobsThatDidNotFinishedByWorkerNodeUri(Uri workerNodeUri, 
																 bool keepJobDetailsIfExists)
		{
			_jobRepository.RequeueJobThatDidNotEndByWorkerNodeUri(workerNodeUri.ToString(), 
																  keepJobDetailsIfExists);
		}
	}
}