using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class RunningEtlJobChecker : IRunningEtlJobChecker
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(RunningEtlJobChecker));
		private readonly IStatisticRepository _statisticRepository;

		public RunningEtlJobChecker(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public bool NightlyEtlJobStillRunning()
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


			if (runningJobs.Any(x => x.JobName == jobName))
			{
				if (logger.IsDebugEnabled)
				{
					var runningNightlyJob = runningJobs.First(x => x.JobName == jobName);
					logger.DebugFormat(
						"The \"{0}\" ETL job is running. Job information: "
						+ "ComputerName: {1}, StartTime: {2:yyyy-MM-dd HH:mm:ss}, IsStartedByService: {3}, "
						+ "LockUntil: {4:yyyy-MM-dd HH:mm:ss}", jobName,
						runningNightlyJob.ComputerName, runningNightlyJob.StartTime, runningNightlyJob.IsStartedByService,
						runningNightlyJob.LockUntil);
				}
				return true;
			}

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("ETL job \"{0}\" is not running.", jobName);
			}
			return false;
		}
	}
}