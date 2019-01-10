using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.FakeData
{
	public class FakeGeneralFunctions: IGeneralFunctions{
		public IList<IDataSourceEtl> DataSourceValidList { get; }
		public IList<IDataSourceEtl> DataSourceInvalidList { get; }
		public IList<IDataSourceEtl> DataSourceValidListIncludedOptionAll { get; }

		private string _dataMartConnectionString;
		private Dictionary<string, IBaseConfiguration> configurations = new Dictionary<string, IBaseConfiguration>();

		public void AddConfiguration(string connectionStr, IBaseConfiguration config)
		{
			configurations[connectionStr] = config;
		}

		public EtlToolStateType GetInitialLoadState()
		{
			throw new System.NotImplementedException();
		}

		public void LoadNewDataSources()
		{
			throw new System.NotImplementedException();
		}

		public IList<ITimeZoneDim> GetTimeZoneList()
		{
			throw new System.NotImplementedException();
		}

		public ITimeZoneDim GetTimeZoneDim(string timeZoneCode)
		{
			throw new System.NotImplementedException();
		}

		public void SaveDataSource(int dataSourceId, int timeZoneId)
		{
			throw new System.NotImplementedException();
		}

		public void SetUtcTimeZoneOnRaptorDataSource()
		{
			throw new System.NotImplementedException();
		}

		public IBaseConfiguration LoadBaseConfiguration()
		{
			return configurations.ContainsKey(_dataMartConnectionString)
				? configurations[_dataMartConnectionString]
				: null;
		}

		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			throw new System.NotImplementedException();
		}

		public int? LoadIntervalLengthInUse()
		{
			throw new System.NotImplementedException();
		}

		public void SetConnectionString(string dataMartConnectionString)
		{
			_dataMartConnectionString = dataMartConnectionString;
		}

		public DateOnlyPeriod GetFactQueueAvailablePeriod(int dataSourceId)
		{
			throw new System.NotImplementedException();
		}

		public DateOnlyPeriod GetFactAgentAvailablePeriod(int dataSourceId)
		{
			throw new System.NotImplementedException();
		}
	}
}