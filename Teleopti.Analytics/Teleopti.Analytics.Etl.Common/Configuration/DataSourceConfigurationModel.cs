using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Configuration
{
	public class DataSourceConfigurationModel
	{
		private readonly IGeneralFunctions _generalFunctions;
		private readonly IBaseConfiguration _baseConfiguration;

		private DataSourceConfigurationModel()
		{
		}

		public DataSourceConfigurationModel(IGeneralFunctions generalFunctions, IBaseConfiguration baseConfiguration)
			: this()
		{
			_generalFunctions = generalFunctions;
			_baseConfiguration = baseConfiguration;
		}

		public IList<DataSourceRow> LoadDataSources()
		{
			var allDataSources = new List<IDataSourceEtl>(_generalFunctions.DataSourceInvalidList);
			allDataSources.AddRange(_generalFunctions.DataSourceValidList);

			var dataSourceRows = new List<DataSourceRow>();
			var rowCount = 0;
			foreach (var dataSource in allDataSources)
			{
				var dataSourceRow = new DataSourceRow(dataSource, rowCount);
				dataSourceRows.Add(dataSourceRow);
				rowCount++;
			}

			return dataSourceRows;
		}

		public IList<ITimeZoneDim> LoadTimeZones()
		{
			IList<ITimeZoneDim> timeZoneList = new List<ITimeZoneDim>();

			var timeZoneNoSelected = new TimeZoneDim(-1, "", "[No Time Zone assigned]", false, 0, 0, false);
			timeZoneList.Add(timeZoneNoSelected);
			var defaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_baseConfiguration.TimeZoneCode);

			foreach (var timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
			{

				timeZoneList.Add(new TimeZoneDim(timeZoneInfo, timeZoneInfo.Id == defaultTimeZone.Id, false));
			}

			return timeZoneList;
		}

		public IList<ITimeZoneDim> TimeZonesFromMart => _generalFunctions.GetTimeZoneList();

		public void SaveDataSource(DataSourceRow dataSourceRow)
		{
			_generalFunctions.SaveDataSource(dataSourceRow.Id,
				int.Parse(dataSourceRow.TimeZoneId, CultureInfo.InvariantCulture));
		}

		public void SaveUtcTimeZoneOnRaptorDataSource()
		{
			_generalFunctions.SetUtcTimeZoneOnRaptorDataSource();
		}

		public EtlToolStateType InitialLoadState => _generalFunctions.GetInitialLoadState();

		public TimeZoneInfo DefaultTimeZone => TimeZoneInfo.FindSystemTimeZoneById(_baseConfiguration.TimeZoneCode);

		public int IntervalLengthMinutes => _baseConfiguration.IntervalLength.Value;
	}
}
