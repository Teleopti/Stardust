using Common.Logging;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class RunningEtlJobChecker : IRunningEtlJobChecker
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AgentBadgeCalculator));
		private readonly IStatisticRepository _statisticRepository;

		public RunningEtlJobChecker(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public bool CheckIfNightlyEtlJobRunning()
		{
			return checkIfEtlJobRunning("Nightly");
		}

		private bool checkIfEtlJobRunning(string jobName)
		{
			var runningJobs = _statisticRepository.GetRunningEtlJobs().ToList();
			if (!runningJobs.Any())
			{
				if (logger.IsDebugEnabled)
				{
					logger.Debug("No ETL job is running.");
				}
				return false;
			}

			var runningNightlyJob = runningJobs.SingleOrDefault(x => x.JobName == jobName);
			if (runningNightlyJob == null)
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("ETL job \"{0}\" is not running.", jobName);
				}
				return false;
			}

			logger.WarnFormat(
				"The \"{0}\" ETL job is running. Job information: "
				+ "ComputerName: {1}, StartTime: {2:yyyy-MM-dd HH:mm:ss}, IsStartedByService: {3}, "
				+ "LockUntil: {4:yyyy-MM-dd HH:mm:ss}", jobName,
				runningNightlyJob.ComputerName, runningNightlyJob.StartTime, runningNightlyJob.IsStartedByService,
				runningNightlyJob.LockUntil);
			return true;
		}
	}
}