using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IGeneralFunctions
	{
		IList<IDataSourceEtl> DataSourceValidList { get; }
		IList<IDataSourceEtl> DataSourceInvalidList { get; }
		IList<IDataSourceEtl> DataSourceValidListIncludedOptionAll { get; }

		EtlToolStateType GetInitialLoadState();

		void LoadNewDataSources();

		IList<ITimeZoneDim> GetTimeZoneList();

		ITimeZoneDim GetTimeZoneDim(string timeZoneCode);

		void SaveDataSource(int dataSourceId, int timeZoneId);
		void SetUtcTimeZoneOnRaptorDataSource();
		IBaseConfiguration LoadBaseConfiguration();
		void SaveBaseConfiguration(IBaseConfiguration configuration);
		int? LoadIntervalLengthInUse();
		void SetConnectionString(string dataMartConnectionString);

		DateOnlyPeriod GetFactQueueAvailablePeriod(int dataSourceId);
		DateOnlyPeriod GetFactAgentAvailablePeriod(int dataSourceId);
	}
}