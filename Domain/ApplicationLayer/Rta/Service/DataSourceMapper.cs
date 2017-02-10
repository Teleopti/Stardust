using System.Collections.Concurrent;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class DataSourceMapper
	{
		private readonly IDataSourceReader _dataSourceReader;
		private readonly PerTenant<ConcurrentDictionary<string, int>> _cache;

		public DataSourceMapper(IDataSourceReader dataSourceReader, ICurrentDataSource dataSource)
		{
			_dataSourceReader = dataSourceReader;
			_cache = new PerTenant<ConcurrentDictionary<string, int>>(dataSource);
		}

		public int ValidateSourceId(string sourceId)
		{
			if (_cache.Value == null)
				_cache.Set(_dataSourceReader.Datasources());

			if (string.IsNullOrEmpty(sourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!_cache.Value.TryGetValue(sourceId, out dataSourceId))
				throw new InvalidSourceException($"Source id \"{sourceId}\" not found");
			return dataSourceId;
		}
	}
}