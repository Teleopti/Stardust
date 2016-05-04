using System.Collections.Concurrent;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class DatabaseLoader : IDatabaseLoader
	{
		private readonly IDatabaseReader _databaseReader;

		public DatabaseLoader(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return _databaseReader.Datasources();
		}
	}
}