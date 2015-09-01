using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers
{
	public class DataSourceResolver
	{
		private readonly IDatabaseLoader _databaseLoader;

		public DataSourceResolver(IDatabaseLoader databaseLoader)
		{
			_databaseLoader = databaseLoader;
		}

		public bool TryResolveId(string sourceId, out int dataSourceId)
		{
			var dictionary = _databaseLoader.Datasources();
			return dictionary.TryGetValue(sourceId, out dataSourceId);
		}
	}
}