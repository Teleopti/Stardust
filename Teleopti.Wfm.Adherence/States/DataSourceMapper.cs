using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.States
{
	public class DataSourceMapper
	{
		private readonly IDataSourceReader _dataSourceReader;
		private readonly IRtaTracer _tracer;
		private readonly PerTenant<ConcurrentDictionary<string, int>> _cache;

		public DataSourceMapper(IDataSourceReader dataSourceReader, ICurrentDataSource dataSource, IRtaTracer tracer)
		{
			_dataSourceReader = dataSourceReader;
			_tracer = tracer;
			_cache = new PerTenant<ConcurrentDictionary<string, int>>(dataSource);
		}

		public int ValidateSourceId(string sourceId, IEnumerable<StateTraceLog> traces)
		{
			if (_cache.Value == null)
				_cache.Set(ReadDataSources());

			if (string.IsNullOrEmpty(sourceId))
			{
				_tracer.For(traces, _tracer.InvalidSourceId);
				throw new InvalidSourceException("Source id is required");
			}

			int dataSourceId;
			if (!_cache.Value.TryGetValue(sourceId, out dataSourceId))
			{
				_tracer.For(traces, _tracer.InvalidSourceId);
				throw new InvalidSourceException($"Source id \"{sourceId}\" not found");
			}
			return dataSourceId;
		}

		[AnalyticsUnitOfWork]
		protected virtual ConcurrentDictionary<string, int> ReadDataSources()
		{
			return _dataSourceReader.Datasources();
		}
	}
}