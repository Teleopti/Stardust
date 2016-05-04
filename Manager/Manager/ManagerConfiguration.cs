namespace Stardust.Manager
{
	public class ManagerConfiguration
	{
		public ManagerConfiguration()
		{
			AllowedNodeDownTimeSeconds = 600;
			CheckNewJobIntervalSeconds = 10;
		}

		public string ConnectionString { get; set; }
		public string Route { get; set; }
		public int AllowedNodeDownTimeSeconds { get; set; }
		public int CheckNewJobIntervalSeconds { get; set; }
	}
}