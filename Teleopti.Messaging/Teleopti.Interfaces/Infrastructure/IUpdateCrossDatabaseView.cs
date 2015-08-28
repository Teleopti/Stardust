namespace Teleopti.Interfaces.Infrastructure
{
	public interface IUpdateCrossDatabaseView
	{
		void Execute(string analyticsDbConnectionString, string aggDatabase);
	}
}