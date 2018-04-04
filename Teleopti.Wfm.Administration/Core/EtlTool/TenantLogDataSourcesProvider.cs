using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

		public IList<DataSourceModel> Load(string tenantName, bool includeInvalidDataSource = false)
		{
			if (Tenants.IsAllTenants(tenantName))
			{
				return new List<DataSourceModel>
				{
					new DataSourceModel {
						Id = -2,
						Name = "< All >",
						TimeZoneCode = "All"
					}
				};
			}

			_generalFunctions.SetConnectionString(_analyticsConnectionsStringExtractor.Extract(tenantName));

			List<IDataSourceEtl> logDataSources;
			if (includeInvalidDataSource)
			{
				logDataSources = _generalFunctions.DataSourceValidList.ToList();
				logDataSources.AddRange(_generalFunctions.DataSourceInvalidList);
			}
			else
			{
				logDataSources = _generalFunctions.DataSourceValidListIncludedOptionAll.ToList();
			}

			return logDataSources.Select(x => new DataSourceModel
				{
					Id = x.DataSourceId,
					Name = x.DataSourceName,
					TimeZoneCode = x.TimeZoneCode,
					IntervalLength = x.IntervalLength,
					IsIntervalLengthSameAsTenant = isIntervalLengthValid(x.IntervalLength)
			})
				.OrderBy(x => x.Name)
				.ToList();
		}

		private bool isIntervalLengthValid(int argIntervalLength)
		{
			var baseConfiguration = _generalFunctions.LoadBaseConfiguration();
			return argIntervalLength == baseConfiguration.IntervalLength;
		}
	}

	public class DataSourceModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string TimeZoneCode { get; set; }
		public int IntervalLength { get; set; }
		public bool IsIntervalLengthSameAsTenant { get; set; }
	}

	public class TenantDataSourceModel
	{
		public string TenantName { get; set; }
		public List<DataSourceModel> DataSources { get; set; }
	}
}