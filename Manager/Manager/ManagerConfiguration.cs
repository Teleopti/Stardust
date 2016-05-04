using System;

namespace Stardust.Manager
{
	public class ManagerConfiguration
	{
		public ManagerConfiguration(string connectionString, string route, int allowedNodeDownTimeSeconds, int checkNewJobIntervalSeconds)
		{
			ConnectionString = connectionString;
			Route = route;
			AllowedNodeDownTimeSeconds = allowedNodeDownTimeSeconds;
			CheckNewJobIntervalSeconds = checkNewJobIntervalSeconds;
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
		}

		public string ConnectionString { get; private set; }
		public string Route { get; private set; }
		public int AllowedNodeDownTimeSeconds { get; private set; }
		public int CheckNewJobIntervalSeconds { get; private set; }
	}
}