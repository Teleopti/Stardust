namespace Teleopti.Ccc.Rta.Server.Resolvers
{
	public class DataSourceResolver : IDataSourceResolver
	{
		private readonly IDatabaseReader _databaseReader;

		public DataSourceResolver(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
		}

		public bool TryResolveId(string sourceId, out int dataSourceId)
		{
			var dictionary = _databaseReader.LoadDatasources();
			return dictionary.TryGetValue(sourceId, out dataSourceId);
		}
	}
}