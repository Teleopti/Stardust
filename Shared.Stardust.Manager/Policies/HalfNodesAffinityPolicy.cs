using System.Collections.Generic;
using System.Linq;
using log4net;
using Stardust.Manager.Models;

namespace Stardust.Manager.Policies
{
	public class HalfNodesAffinityPolicy
	{
		public const string PolicyName = "HalfNodesAffinity";
		//private static readonly ILog ManagerLogger = LogManager.GetLogger(typeof(HalfNodesAffinityPolicy));
        private static readonly ILog ManagerLogger = LogManager.GetLogger(typeof(HalfNodesAffinityPolicy));

		public bool CheckPolicy(JobQueueItem jobQueueItem, List<Job> allExecutingJobs, int countOfAliveNodes)
		{
			ManagerLogger.Info($"Checking HalfNodesAffinityPolicy for job {jobQueueItem.JobId}");
			var totalNumberOfNodesCanRun = countOfAliveNodes / 2;
			if (jobQueueItem.Policy != PolicyName)
			{
				return true;
			}
			var isSatisfied = allExecutingJobs.Count(x => x.Policy == PolicyName) < totalNumberOfNodesCanRun;
			if(!isSatisfied)
				ManagerLogger.Info($"Failed HalfNodesAffinityPolicy for job {jobQueueItem.JobId}");
			return isSatisfied;
		}
	}
}