using System.Collections;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.AdministrationTest.EtlTool
{
	public class FakeGeneralInfrastructure : IGeneralInfrastructure
	{
		public IList GetDataSourceList(bool getValidDataSources, bool includeOptionAll)
		{
			throw new System.NotImplementedException();
		}

		public int GetInitialLoadState()
		{
			throw new System.NotImplementedException();
		}

		public void LoadNewDataSourcesFromAggregationDatabase()
		{
			throw new System.NotImplementedException();
		}

		public IList GetTimeZonesFromMart()
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
			throw new System.NotImplementedException();
		}

		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			throw new System.NotImplementedException();
		}

		public int LoadRowsInDimIntervalTable()
		{
			throw new System.NotImplementedException();
		}

		public void SetDataMartConnectionString(string dataMartConnectionString)
		{
			throw new System.NotImplementedException();
		}

		IList<IDataSourceEtl> IGeneralInfrastructure.GetDataSourceList(bool getValidDataSources, bool includeOptionAll)
		{
			throw new System.NotImplementedException();
		}

		IList<ITimeZoneDim> IGeneralInfrastructure.GetTimeZonesFromMart()
		{
			throw new System.NotImplementedException();
		}
	}
}