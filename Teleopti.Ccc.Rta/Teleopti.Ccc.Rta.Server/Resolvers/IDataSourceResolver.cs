namespace Teleopti.Ccc.Rta.Server.Resolvers
{
	public interface IDataSourceResolver
	{
		bool TryResolveId(string sourceId, out int dataSourceId);
	}
}