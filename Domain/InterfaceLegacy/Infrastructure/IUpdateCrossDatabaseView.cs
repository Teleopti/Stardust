namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IUpdateCrossDatabaseView
	{
		void Execute(string analyticsDbConnectionString, string aggDatabase);
	}
}