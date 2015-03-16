using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Resolvers
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