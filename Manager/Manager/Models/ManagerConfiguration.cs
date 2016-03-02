using Stardust.Manager.Interfaces;

namespace Stardust.Manager.Models
{
	public class ManagerConfiguration : IManagerConfiguration
	{
		public string ConnectionString { get; set; }

		public string Route { get; set; }

		public int AllowedNodeDownTimeSeconds { get; set; }

		public int CheckNewJobIntervalSeconds { get; set; }

		public ManagerConfiguration(int allowedNodeDownTimeSeconds = 600, int checkNewJobIntervalSeconds = 10) //10 minutes
		{
			AllowedNodeDownTimeSeconds = allowedNodeDownTimeSeconds;
			CheckNewJobIntervalSeconds = checkNewJobIntervalSeconds;
		}
	
	}
}