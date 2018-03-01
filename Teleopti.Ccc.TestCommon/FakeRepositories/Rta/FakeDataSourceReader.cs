using System.Collections.Concurrent;
using System.Linq;
using Teleopti.Ccc.Domain.Rta.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeDataSourceReader : IDataSourceReader
	{
		private readonly FakeDataSources _dataSources;

		public FakeDataSourceReader(FakeDataSources dataSources)
		{
			_dataSources = dataSources;
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return new ConcurrentDictionary<string, int>(
				_dataSources
					.Datasources
					.GroupBy(x => x.Key, (key, g) => g.First()
					));
		}
		
	}
}