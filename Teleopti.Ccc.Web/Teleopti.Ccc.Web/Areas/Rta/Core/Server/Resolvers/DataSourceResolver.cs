using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers
{
	public class DataSourceResolver
	{
		private readonly IDatabaseReader _databaseReader;

		public DataSourceResolver(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
		}

		public bool TryResolveId(string sourceId, out int dataSourceId)
		{
			var dictionary = _databaseReader.Datasources();
			return dictionary.TryGetValue(sourceId, out dataSourceId);
		}
	}
}