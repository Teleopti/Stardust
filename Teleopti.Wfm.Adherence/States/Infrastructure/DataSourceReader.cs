using System.Collections.Concurrent;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
{
	public class DataSourceReader : IDataSourceReader
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public DataSourceReader(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			var datasources = _analyticsUnitOfWork.Current().Session()
				.CreateSQLQuery("SELECT datasource_id, source_id FROM mart.sys_datasource")
				.SetResultTransformer(Transformers.AliasToBean(typeof(datasource)))
				.List<datasource>()
				.GroupBy(x => x.source_id, (key, g) => g.First());

			return new ConcurrentDictionary<string, int>(datasources
				.ToDictionary(datasource => datasource.source_id, datasource => datasource.datasource_id));
		}

		private class datasource
		{
			public int datasource_id { get; set; }
			public string source_id { get; set; }
		}

	}
}