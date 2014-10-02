namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers
{
	public interface IDataSourceResolver
	{
		bool TryResolveId(string sourceId, out int dataSourceId);
	}
}