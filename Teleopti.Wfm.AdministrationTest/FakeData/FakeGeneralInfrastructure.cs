using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeGeneralInfrastructure : IGeneralInfrastructure
	{
		private readonly IList<IDataSourceEtl> dataSourceEtls = new List<IDataSourceEtl>();
		public IList<IDataSourceEtl> GetDataSourceList(bool getValidDataSources, bool includeOptionAll)
		{
			return dataSourceEtls;
		}

		public void HasDataSources(IDataSourceEtl dataSource)
		{
			dataSourceEtls.Add(dataSource);
		}

		public int GetInitialLoadState()
		{
			throw new NotImplementedException();
		}

		public void LoadNewDataSourcesFromAggregationDatabase()
		{
			throw new NotImplementedException();
		}

		public IList<ITimeZoneDim> GetTimeZonesFromMart()
		{
			throw new NotImplementedException();
		}

		public void SaveDataSource(int dataSourceId, int timeZoneId)
		{
			var dataSource = dataSourceEtls.Single(x => x.DataSourceId == dataSourceId);
			dataSource.TimeZoneId = timeZoneId;
			dataSource.Inactive = false;
		}

		public void SetUtcTimeZoneOnRaptorDataSource()
		{
			throw new NotImplementedException();
		}

		public IBaseConfiguration LoadBaseConfiguration()
		{
			return new BaseConfiguration(1053,15,"UTC", false);
		}

		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			throw new NotImplementedException();
		}

		public int LoadRowsInDimIntervalTable()
		{
			throw new NotImplementedException();
		}

		public void SetDataMartConnectionString(string dataMartConnectionString)
		{
			
		}
	}
}
