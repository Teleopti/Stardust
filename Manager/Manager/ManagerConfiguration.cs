using System;

namespace Stardust.Manager
{
	public class ManagerConfiguration
	{
		public ManagerConfiguration(string connectionString, string route, int allowedNodeDownTimeSeconds, int checkNewJobIntervalSeconds, int purgebatchsize, int purgeIntervalHours, int purgeJobsOlderThanHours)
		{
			ConnectionString = connectionString;
			Route = route;
			AllowedNodeDownTimeSeconds = allowedNodeDownTimeSeconds;
			CheckNewJobIntervalSeconds = checkNewJobIntervalSeconds;
			Purgebatchsize = purgebatchsize;
			PurgeIntervalHours = purgeIntervalHours;
			PurgeJobsOlderThanHours = purgeJobsOlderThanHours;
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
			if (Purgebatchsize <= 0)
			{
				throw new ArgumentNullException("Purgebatchsize");
			}
			if (PurgeIntervalHours <= 0)
			{
				throw new ArgumentNullException("PurgeIntervalHours");
			}
			if (PurgeJobsOlderThanHours <= 0)
			{
				throw new ArgumentNullException("PurgeJobsOlderThanHours");
			}
		}

		public string ConnectionString { get; private set; }
		public string Route { get; private set; }
		public int AllowedNodeDownTimeSeconds { get; private set; }
		public int CheckNewJobIntervalSeconds { get; private set; }
		public int Purgebatchsize { get; private set; }
		public int PurgeIntervalHours { get; private set; }
		public int PurgeJobsOlderThanHours { get; private set; }
	}
}