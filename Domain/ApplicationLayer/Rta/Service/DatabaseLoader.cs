using System.Collections.Concurrent;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class DatabaseLoader : IDatabaseLoader
	{
		private readonly IDataSourceReader _dataSourceReader;

		public DatabaseLoader(IDataSourceReader dataSourceReader)
		{
			_dataSourceReader = dataSourceReader;
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return _dataSourceReader.Datasources();
		}
	}
}