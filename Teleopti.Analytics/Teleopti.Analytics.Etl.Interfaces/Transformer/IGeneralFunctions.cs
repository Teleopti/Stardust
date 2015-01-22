using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
	public interface IGeneralFunctions
	{
		IList<IDataSourceEtl> DataSourceValidList { get; }
		IList<IDataSourceEtl> DataSourceInvalidList { get; }
		IList<IDataSourceEtl> DataSourceValidListIncludedOptionAll { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		EtlToolStateType GetInitialLoadState();

		void LoadNewDataSources();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IList<ITimeZoneDim> GetTimeZoneList();

		void SaveDataSource(int dataSourceId, int timeZoneId);
		void SetUtcTimeZoneOnRaptorDataSource();
		IBaseConfiguration LoadBaseConfiguration();
		void SaveBaseConfiguration(IBaseConfiguration configuration);
		int? LoadIntervalLengthInUse();
	}
}