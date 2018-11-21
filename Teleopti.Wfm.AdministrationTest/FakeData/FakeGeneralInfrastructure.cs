using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeGeneralInfrastructure : IGeneralInfrastructure
	{
		public const int NullTimeZoneId = -9999;

		private readonly List<IDataSourceEtl> aggDataSources = new List<IDataSourceEtl>();
		private readonly List<IDataSourceEtl> dataSourceEtls = new List<IDataSourceEtl>();
		private readonly List<ITimeZoneDim> timeZoneDims = new List<ITimeZoneDim>();
		private readonly Dictionary<int, DateOnlyPeriod> factQueuePeriods = new Dictionary<int, DateOnlyPeriod>();
		private readonly Dictionary<int, DateOnlyPeriod> factAgentPeriods = new Dictionary<int, DateOnlyPeriod>();

		public IList<IDataSourceEtl> GetDataSourceList(bool getValidDataSources, bool includeOptionAll)
		{
			var result = getValidDataSources
				? dataSourceEtls.Where(x => x.TimeZoneCode != null).ToList()
				: dataSourceEtls.Where(x => x.TimeZoneCode == null).ToList();

			if (includeOptionAll)
			{
				// Refer to SP mart.sys_get_datasources
				result.Insert(0, new DataSourceEtl(-2, "<All>", 0, "UTC", 15, false));
			}

			return result;
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
				dataSourceEtls.Add(new DataSourceEtl(newDataSourceId, newDataSource.DataSourceName, NullTimeZoneId, null,
					newDataSource.IntervalLength, false));
				newDataSourceId++;
			}
		}

		public IList<ITimeZoneDim> GetTimeZonesFromMart()
		{
			return timeZoneDims;
		}

		public ITimeZoneDim GetTimeZoneDim(string timeZoneCode)
		{
			var timeZoneDim = timeZoneDims.SingleOrDefault(x => x.TimeZoneCode == timeZoneCode);
			if (timeZoneDim != null) return timeZoneDim;

			timeZoneDims.Add(new TimeZoneDim(TimeZoneInfo.FindSystemTimeZoneById(timeZoneCode), timeZoneCode == "UTC", false));
			return timeZoneDims.Single(x => x.TimeZoneCode == timeZoneCode);
		}

		public void SaveDataSource(int dataSourceId, int timeZoneId)
		{
			var dataSource = dataSourceEtls.Single(x => x.DataSourceId == dataSourceId);
			dataSource.TimeZoneId = timeZoneId;
			dataSource.TimeZoneCode = "UTC";
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

		public DateOnlyPeriod GetFactQueuePeriod(int dataSourceId)
		{
			return factQueuePeriods.ContainsKey(dataSourceId)
				? factQueuePeriods[dataSourceId]
				: new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue);
		}

		public DateOnlyPeriod GetFactAgentPeriod(int dataSourceId)
		{
			return factAgentPeriods.ContainsKey(dataSourceId)
				? factAgentPeriods[dataSourceId]
				: new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue);
		}

		public void HasFactQueuePeriod(int dataSourceId, DateOnlyPeriod period)
		{
			factQueuePeriods[dataSourceId] = period;
		}

		public void HasFactAgentPeriod(int dataSourceId, DateOnlyPeriod period)
		{
			factAgentPeriods[dataSourceId] = period;
		}
	}
}
