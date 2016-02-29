namespace Stardust.Manager.Interfaces
{
	public interface IManagerConfiguration
	{
		int AllowedNodeDownTimeSeconds { get; set; }
		string ConnectionString { get; set; }
		string Route { get; set; }
	}
}