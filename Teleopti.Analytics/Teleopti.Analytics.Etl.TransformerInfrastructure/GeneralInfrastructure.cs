using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Interfaces.Domain;
using log4net;


namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public class GeneralInfrastructure : IGeneralInfrastructure
	{
		private const string CultureKey = "Culture";
		private const string IntervalLengthMinutesKey = "IntervalLengthMinutes";
		private const string TimeZoneCodeKey = "TimeZoneCode";
		
		private readonly string _dataMartConnectionString;
		readonly ILog _logger = LogManager.GetLogger(typeof(GeneralInfrastructure));

		private GeneralInfrastructure() { }

		public GeneralInfrastructure(string dataMartConnectionString)
			: this()
		{
			_dataMartConnectionString = dataMartConnectionString;
			Trace.WriteLine(_dataMartConnectionString.Length.ToString(CultureInfo.InvariantCulture));
		}

		public IList<IDataSourceEtl> GetDataSourceList(bool getValidDataSources, bool includeOptionAll)
		{
			DataSet dataSet = getDatasources(getValidDataSources, includeOptionAll);
			IList<IDataSourceEtl> dataSourceList = new List<IDataSourceEtl>();

			if (dataSet != null && dataSet.Tables[0] != null)
			{
				foreach (DataRow row in dataSet.Tables[0].Rows)
				{
					IDataSourceEtl dataSourceEtl =
						new DataSourceEtl(Convert.ToInt32(row["datasource_id"], CultureInfo.InvariantCulture),
									   row["datasource_name"].ToString(),
									   Convert.ToInt32(row["time_zone_id"], CultureInfo.InvariantCulture),
									   row["time_zone_code"].ToString(),
									   Convert.ToInt32(row["interval_length"], CultureInfo.InvariantCulture),
									   (bool)row["inactive"]);
					dataSourceList.Add(dataSourceEtl);
				}
			}

			return dataSourceList;
		}

		public int GetInitialLoadState()
		{
			return (int)HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.sys_initial_load_state_get", null,
												 _dataMartConnectionString);
		}

		public void LoadNewDataSourcesFromAggregationDatabase()
		{
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_datasource_load", null,
											_dataMartConnectionString);
		}

		public IList<ITimeZoneDim> GetTimeZonesFromMart()
		{
			IList<ITimeZoneDim> timeZoneList = new List<ITimeZoneDim>();

			DataSet dataSet = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_dim_time_zone_get",
															 null, _dataMartConnectionString);
			if (dataSet != null && dataSet.Tables.Count == 1)
			{
				foreach (DataRow dataRow in dataSet.Tables[0].Rows)
				{
					ITimeZoneDim timeZoneDim = new TimeZoneDim((Int16)dataRow["time_zone_id"],
														(string)dataRow["time_zone_code"],
														(string)dataRow["time_zone_name"],
														handleDbNullValueBool(dataRow["default_zone"]),
														handleDbNullValueInt(dataRow["utc_conversion"]),
														handleDbNullValueInt(dataRow["utc_conversion_dst"])
														);
					timeZoneList.Add(timeZoneDim);
				}
			}

			return timeZoneList;
		}

		public ITimeZoneDim DefaultTimeZone
		{
			get
			{
				var timeZones = GetTimeZonesFromMart();
				ITimeZoneDim utc = timeZones.FirstOrDefault(f => f.TimeZoneCode == "UTC");

				foreach (var timeZoneDim in timeZones)
				{
					if (timeZoneDim.IsDefaultTimeZone)
					{
						return timeZoneDim;
					}
				}
				return utc;
			}
		}

		public void SaveDataSource(int dataSourceId, int timeZoneId)
		{
			var parameterList = new List<SqlParameter>();
			parameterList.Add(new SqlParameter("datasource_id", dataSourceId));
			parameterList.Add(new SqlParameter("time_zone_id", timeZoneId));

			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_datasource_save",
											parameterList, _dataMartConnectionString);
		}

		public void SetUtcTimeZoneOnRaptorDataSource()
		{
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_datasource_set_raptor_time_zone",
											null, _dataMartConnectionString);
		}

		public IBaseConfiguration LoadBaseConfiguration()
		{
			var dataSet = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.sys_configuration_get", null, _dataMartConnectionString);

			if (dataSet == null || dataSet.Tables.Count != 1)
				return new BaseConfiguration(null, null, null);

			int? culture = null;
			int? intervalLength = null;
			string timeZone = null;

			foreach (DataRow row in dataSet.Tables[0].Rows)
			{
				var key = (string)row["key"];
				var value = (string)row["value"];
				switch (key)
				{
					case CultureKey:
						culture = int.Parse(value, CultureInfo.CurrentCulture);
						break;
					case IntervalLengthMinutesKey:
						intervalLength = int.Parse(value, CultureInfo.CurrentCulture);
						break;
					case TimeZoneCodeKey:
						timeZone = value;
						break;
					default:
						_logger.InfoFormat(CultureInfo.InvariantCulture, "Trying to load un unknown configuration key named: '{0}'.", key);
						break;
				}
			}

			return new BaseConfiguration(culture, intervalLength, timeZone);
		}

		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			var parameterList = new List<SqlParameter> {new SqlParameter("key", CultureKey), new SqlParameter("value", configuration.CultureId)};
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_configuration_save", parameterList, _dataMartConnectionString);

			parameterList = new List<SqlParameter> { new SqlParameter("key", IntervalLengthMinutesKey), new SqlParameter("value", configuration.IntervalLength) };
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_configuration_save", parameterList, _dataMartConnectionString);

			parameterList = new List<SqlParameter> { new SqlParameter("key", TimeZoneCodeKey), new SqlParameter("value", configuration.TimeZoneCode) };
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_configuration_save", parameterList, _dataMartConnectionString);
		}

		public int LoadRowsInDimIntervalTable()
		{
			return (int)HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_dim_interval_check", _dataMartConnectionString);
		}

		private static bool handleDbNullValueBool(object value)
		{
			if (value == DBNull.Value)
				return false;

			return (bool)value;
		}

		private static int handleDbNullValueInt(object value)
		{
			if (value == DBNull.Value)
				return 0;

			return (int)value;
		}

		private DataSet getDatasources(bool getValidDatasources, bool includeOptionAll)
		{
			var parameterList = new List<SqlParameter>
                                    {
                                        new SqlParameter("get_valid_datasource", getValidDatasources),
                                        new SqlParameter("include_option_all", includeOptionAll)
                                    };

			return HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.sys_get_datasources", parameterList,
												  _dataMartConnectionString);
		}
	}
}
