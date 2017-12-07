using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class TenantLogDataSourcesProvider
	{
		private readonly AnalyticsConnectionsStringExtractor _analyticsConnectionsStringExtractor;
		private readonly IGeneralFunctions _generalFunctions;

		public TenantLogDataSourcesProvider(AnalyticsConnectionsStringExtractor analyticsConnectionsStringExtractor, IGeneralFunctions generalFunctions)
		{
			_analyticsConnectionsStringExtractor = analyticsConnectionsStringExtractor;
			_generalFunctions = generalFunctions;
		}

		public IList<DataSourceModel> Load(string tenantName)
		{
			_generalFunctions.SetConnectionString(_analyticsConnectionsStringExtractor.Extract(tenantName));
			var logDataSources = _generalFunctions.DataSourceValidListIncludedOptionAll;
			return logDataSources
				.Select(x => new DataSourceModel{Id = x.DataSourceId, Name = x.DataSourceName})
				.ToList();
		}
	}

	public class DataSourceModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}