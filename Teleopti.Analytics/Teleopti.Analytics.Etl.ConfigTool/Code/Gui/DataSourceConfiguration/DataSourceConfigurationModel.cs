using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Code.Gui.DataSourceConfiguration
{
	public class DataSourceConfigurationModel
	{
		private readonly IGeneralFunctions _generalFunctions;
		private readonly IBaseConfiguration _baseConfiguration;

		private DataSourceConfigurationModel() { }

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
			int rowCount = 0;
			foreach (IDataSourceEtl dataSource in allDataSources)
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

			foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
			{

				timeZoneList.Add(new TimeZoneDim(timeZoneInfo, timeZoneInfo.Id == defaultTimeZone.Id, false));
			}

			return timeZoneList;
		}

		public IList<ITimeZoneDim> TimeZonesFromMart
		{
			get
			{
				return _generalFunctions.GetTimeZoneList();
			}
		}

		public void SaveDataSource(DataSourceRow dataSourceRow)
		{
			_generalFunctions.SaveDataSource(dataSourceRow.Id,
													  int.Parse(dataSourceRow.TimeZoneId, CultureInfo.InvariantCulture));
		}

		public void SaveUtcTimeZoneOnRaptorDataSource()
		{
			_generalFunctions.SetUtcTimeZoneOnRaptorDataSource();
		}

		public EtlToolStateType InitialLoadState
		{
			get { return _generalFunctions.GetInitialLoadState(); }
		}

		public TimeZoneInfo DefaultTimeZone
		{
			get { return TimeZoneInfo.FindSystemTimeZoneById(_baseConfiguration.TimeZoneCode); }
		}

		public int IntervalLengthMinutes
		{
			get { return _baseConfiguration.IntervalLength.Value; }
		}
	}
}
