using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public interface IGeneralInfrastructure
	{
		IList<IDataSourceEtl> GetDataSourceList(bool getValidDataSources, bool includeOptionAll);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		int GetInitialLoadState();
		void LoadNewDataSourcesFromAggregationDatabase();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IList<ITimeZoneDim> GetTimeZonesFromMart();
		ITimeZoneDim GetTimeZoneDim(string timeZoneCode);
		void SaveDataSource(int dataSourceId, int timeZoneId);
		void SetUtcTimeZoneOnRaptorDataSource();
		IBaseConfiguration LoadBaseConfiguration();
		void SaveBaseConfiguration(IBaseConfiguration configuration);
		int LoadRowsInDimIntervalTable();
		void SetDataMartConnectionString(string dataMartConnectionString);
		DateOnlyPeriod GetFactQueuePeriod(int dataSourceId);
		DateOnlyPeriod GetFactAgentPeriod(int dataSourceId);
	}
}