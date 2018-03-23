using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class TenantLogDataSourcesProvider
	{
		private readonly AnalyticsConnectionsStringExtractor _analyticsConnectionsStringExtractor;
		private readonly IGeneralFunctions _generalFunctions;

		public TenantLogDataSourcesProvider(AnalyticsConnectionsStringExtractor analyticsConnectionsStringExtractor,
			IGeneralFunctions generalFunctions)
		{
			_analyticsConnectionsStringExtractor = analyticsConnectionsStringExtractor;
			_generalFunctions = generalFunctions;
		}

		public IList<DataSourceModel> Load(string tenantName)
		{
			if (Tenants.IsAllTenants(tenantName))
			{
				return new List<DataSourceModel>
				{
					new DataSourceModel {
						Id = -2,
						Name = "< All >",
						TimeZoneId = -1
					}
				};
			}

			_generalFunctions.SetConnectionString(_analyticsConnectionsStringExtractor.Extract(tenantName));
			var logDataSources = _generalFunctions.DataSourceValidListIncludedOptionAll;
			return logDataSources.Select(x => new DataSourceModel
				{
					Id = x.DataSourceId,
					Name = x.DataSourceName,
					TimeZoneId = x.TimeZoneId
				})
				.ToList();
		}
	}

	public class DataSourceModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int TimeZoneId { get; set; }
	}

	public class TenantDataSourceModel
	{
		public string TenantName { get; set; }
		public DataSourceModel DataSource { get; set; }
	}
}