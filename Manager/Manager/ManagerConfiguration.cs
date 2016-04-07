namespace Stardust.Manager
{
	public class ManagerConfiguration
	{
		public string ConnectionString { get; set; }

		public string Route { get; set; }

		public int AllowedNodeDownTimeSeconds { get; set; }


		public int CheckNewJobIntervalSeconds { get; set; }

		public ManagerConfiguration()
		{
			AllowedNodeDownTimeSeconds = 600;
			CheckNewJobIntervalSeconds = 10;
		}
	}
}