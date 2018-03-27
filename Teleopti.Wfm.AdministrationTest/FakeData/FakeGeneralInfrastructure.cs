using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeGeneralInfrastructure : IGeneralInfrastructure
	{
		public const int NullTimeZoneId = -9999;

		private readonly List<IDataSourceEtl> aggDataSources = new List<IDataSourceEtl>();
		private readonly List<IDataSourceEtl> dataSourceEtls = new List<IDataSourceEtl>();

		public IList<IDataSourceEtl> GetDataSourceList(bool getValidDataSources, bool includeOptionAll)
		{
			return getValidDataSources
				? dataSourceEtls.Where(x => x.TimeZoneId != NullTimeZoneId).ToList()
				: dataSourceEtls;
		}

		public void HasAggDataSources(IDataSourceEtl dataSource)
		{
			aggDataSources.Add(dataSource);
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
			var newDataSourceId = dataSourceEtls.Any() ? dataSourceEtls.Select(x => x.DataSourceId).Max() + 1 : 1;
			var newDataSources = aggDataSources.Where(x => dataSourceEtls.All(y => y.DataSourceName != x.DataSourceName));
			foreach (var newDataSource in newDataSources)
			{
				dataSourceEtls.Add(new DataSourceEtl(newDataSourceId, newDataSource.DataSourceName, NullTimeZoneId, "Not Defined",
					newDataSource.IntervalLength, false));
				newDataSourceId++;
			}
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
