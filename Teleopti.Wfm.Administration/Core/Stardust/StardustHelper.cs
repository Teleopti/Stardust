using System;
using System.Collections.Generic;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public class StardustHelper
	{
		private readonly StardustRepository _stardustRepository;

		public StardustHelper(StardustRepository stardustRepository)
		{
			_stardustRepository = stardustRepository;
		}

		public IList<JobHistory> GetJobHistoryList(Guid nodeId)
		{
			return _stardustRepository.HistoryList(nodeId);
		}

		public IList<JobHistory> GetJobHistoryList()
		{
			return _stardustRepository.HistoryList();
		}

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			return _stardustRepository.JobHistoryDetails(jobId);
		}

		public List<WorkerNode> WorkerNodes()
		{
			return _stardustRepository.WorkerNodes();
		}

		public WorkerNode WorkerNode(Guid Id)
		{
			return _stardustRepository.WorkerNode(Id);
		}
	}
}