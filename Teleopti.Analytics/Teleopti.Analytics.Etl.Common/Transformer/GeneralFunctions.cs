using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class GeneralFunctions : IGeneralFunctions
	{
		private readonly IGeneralInfrastructure _generalInfrastructure;

		public GeneralFunctions(IGeneralInfrastructure generalInfrastructure)
		{
			_generalInfrastructure = generalInfrastructure;
		}

		public IList<IDataSourceEtl> DataSourceValidList => getDatasourceList(true, false);

		public IList<IDataSourceEtl> DataSourceInvalidList => getDatasourceList(false, false);

		public IList<IDataSourceEtl> DataSourceValidListIncludedOptionAll => getDatasourceList(true, true);

		private IList<IDataSourceEtl> getDatasourceList(bool getValidDatasources, bool includeOptionAll)
		{
			var origList = _generalInfrastructure.GetDataSourceList(getValidDatasources, includeOptionAll);
			var retList = new List<IDataSourceEtl>();
			foreach (var source in origList)
			{
				var newSource = new DataSourceEtl(source.DataSourceId, source.DataSourceName, source.TimeZoneId, source.TimeZoneCode,
					source.IntervalLength, source.Inactive);
				retList.Add(newSource);
			}
			return retList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public EtlToolStateType GetInitialLoadState()
		{
			var state = _generalInfrastructure.GetInitialLoadState();
			return (EtlToolStateType)state;
		}

		public void LoadNewDataSources()
		{
			_generalInfrastructure.LoadNewDataSourcesFromAggregationDatabase();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public IList<ITimeZoneDim> GetTimeZoneList()
		{
			return _generalInfrastructure.GetTimeZonesFromMart();
		}

		public ITimeZoneDim GetTimeZoneDim(string timeZoneCode)
		{
			return _generalInfrastructure.GetTimeZoneDim(timeZoneCode);
		}

		public void SaveDataSource(int dataSourceId, int timeZoneId)
		{
			_generalInfrastructure.SaveDataSource(dataSourceId, timeZoneId);
		}
		public void SetUtcTimeZoneOnRaptorDataSource()
		{
			_generalInfrastructure.SetUtcTimeZoneOnRaptorDataSource();
		}

		public IBaseConfiguration LoadBaseConfiguration()
		{
			return _generalInfrastructure.LoadBaseConfiguration();
		}

		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			_generalInfrastructure.SaveBaseConfiguration(configuration);
		}

		public int? LoadIntervalLengthInUse()
		{
			var dimIntervalRowCount = _generalInfrastructure.LoadRowsInDimIntervalTable();
			return dimIntervalRowCount == 0 ? (int?) null : 1440 / dimIntervalRowCount;
		}

		public void SetConnectionString(string dataMartConnectionString)
		{
			_generalInfrastructure.SetDataMartConnectionString(dataMartConnectionString);
		}

		public DateOnlyPeriod GetFactQueueAvailblePeriod(int datasourceId)
		{
			return _generalInfrastructure.GetFactQueuePeriod(datasourceId);
		}

		public DateOnlyPeriod GetFactAgentAvailblePeriod(int dataSourceId)
		{
			return _generalInfrastructure.GetFactAgentPeriod(dataSourceId);
		}
	}
}