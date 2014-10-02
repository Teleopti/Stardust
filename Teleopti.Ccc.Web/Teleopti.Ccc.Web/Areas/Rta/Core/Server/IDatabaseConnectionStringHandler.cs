namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public interface IDatabaseConnectionStringHandler
	{
		string AppConnectionString();
		string DataStoreConnectionString();
	}
}