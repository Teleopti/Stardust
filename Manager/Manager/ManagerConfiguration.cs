using System;

namespace Stardust.Manager
{
	public class ManagerConfiguration
	{
		public ManagerConfiguration(string connectionString, string route, int allowedNodeDownTimeSeconds, int checkNewJobIntervalSeconds, int purgeJobsBatchSize, int purgeJobsIntervalHours, int purgeJobsOlderThanHours, int purgeNodesIntervalHours)
		{
			ConnectionString = connectionString;
			Route = route;
			AllowedNodeDownTimeSeconds = allowedNodeDownTimeSeconds;
			CheckNewJobIntervalSeconds = checkNewJobIntervalSeconds;
			PurgeJobsBatchSize = purgeJobsBatchSize;
			PurgeJobsIntervalHours = purgeJobsIntervalHours;
			PurgeJobsOlderThanHours = purgeJobsOlderThanHours;
			PurgeNodesIntervalHours = purgeNodesIntervalHours;
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			if (ConnectionString == null)
			{
				throw new ArgumentNullException("ConnectionString");
			}
			if (Route == null)
			{
				throw new ArgumentNullException("Route");
			}
			if (AllowedNodeDownTimeSeconds <= 0)
			{
				throw new ArgumentNullException("AllowedNodeDownTimeSeconds");
			}
			if (CheckNewJobIntervalSeconds <= 0)
			{
				throw new ArgumentNullException("CheckNewJobIntervalSeconds");
			}
			if (PurgeJobsBatchSize <= 0)
			{
				throw new ArgumentNullException("PurgeJobsBatchSize");
			}
			if (PurgeJobsIntervalHours <= 0)
			{
				throw new ArgumentNullException("PurgeJobsIntervalHours");
			}
			if (PurgeJobsOlderThanHours <= 0)
			{
				throw new ArgumentNullException("PurgeJobsOlderThanHours");
			}
			if (PurgeNodesIntervalHours <= 0)
			{
				throw new ArgumentNullException("PurgeNodesIntervalHours");
			}
		}

		public string ConnectionString { get; private set; }
		public string Route { get; private set; }
		public int AllowedNodeDownTimeSeconds { get; private set; }
		public int CheckNewJobIntervalSeconds { get; private set; }
		public int PurgeJobsBatchSize { get; private set; }
		public int PurgeJobsIntervalHours { get; private set; }
		public int PurgeJobsOlderThanHours { get; private set; }
		public int PurgeNodesIntervalHours { get; private set; }
	}
}