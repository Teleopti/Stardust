using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public interface IGeneralInfrastructure
	{
		IList<IDataSourceEtl> GetDataSourceList(bool getValidDataSources, bool includeOptionAll);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		int GetInitialLoadState();
		void LoadNewDataSourcesFromAggregationDatabase();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IList<ITimeZoneDim> GetTimeZonesFromMart();
		void SaveDataSource(int dataSourceId, int timeZoneId);
		void SetUtcTimeZoneOnRaptorDataSource();
		IBaseConfiguration LoadBaseConfiguration();
		void SaveBaseConfiguration(IBaseConfiguration configuration);
		int LoadRowsInDimIntervalTable();
	}
}