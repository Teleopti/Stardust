using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class GeneralInfrastructure : IGeneralInfrastructure
	{
		private const string minDateColumn = "MinDate";
		private const string maxDateColumn = "MaxDate";

		private readonly IBaseConfigurationRepository _baseConfigurationRepository;
		private string _connectionString;

		public GeneralInfrastructure(IBaseConfigurationRepository baseConfigurationRepository)
		{
			_baseConfigurationRepository = baseConfigurationRepository;
		}

		private string dataMartConnectionString
		{
			get
			{
				if (_connectionString == null)
					throw new ArgumentException(@"You need to set the datamart connection string before using it.",
						nameof(_connectionString));

				return _connectionString;
			}
			set => _connectionString = value;
		}

		public IList<IDataSourceEtl> GetDataSourceList(bool getValidDataSources, bool includeOptionAll)
		{
			var dataSet = getDatasources(getValidDataSources, includeOptionAll);
			var dataSourceList = new List<IDataSourceEtl>();
			if (dataSet?.Tables[0] == null) return dataSourceList;

			foreach (DataRow row in dataSet.Tables[0].Rows)
			{
				var dataSourceEtl = new DataSourceEtl(Convert.ToInt32(row["datasource_id"], CultureInfo.InvariantCulture),
					row["datasource_name"].ToString(),
					Convert.ToInt32(row["time_zone_id"], CultureInfo.InvariantCulture),
					row["time_zone_code"].ToString(),
					Convert.ToInt32(row["interval_length"], CultureInfo.InvariantCulture),
					(bool) row["inactive"]);
				dataSourceList.Add(dataSourceEtl);
			}

			return dataSourceList;
		}

		public int GetInitialLoadState()
		{
			return (int)HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.sys_initial_load_state_get", null,
				dataMartConnectionString);
		}

		public void LoadNewDataSourcesFromAggregationDatabase()
		{
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_datasource_load", null,
				dataMartConnectionString);
		}

		public IList<ITimeZoneDim> GetTimeZonesFromMart()
		{
			IList<ITimeZoneDim> timeZoneList = new List<ITimeZoneDim>();

			var dataSet = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_dim_time_zone_get", null,
				dataMartConnectionString);
			if (dataSet == null || dataSet.Tables.Count != 1) return timeZoneList;

			foreach (DataRow dataRow in dataSet.Tables[0].Rows)
			{
				var timeZoneDim = new TimeZoneDim((short) dataRow["time_zone_id"],
					(string) dataRow["time_zone_code"],
					(string) dataRow["time_zone_name"],
					handleDbNullValue<bool>(dataRow["default_zone"]),
					handleDbNullValue<int>(dataRow["utc_conversion"]),
					handleDbNullValue<int>(dataRow["utc_conversion_dst"]),
					handleDbNullValue<bool>(dataRow["utc_in_use"])
				);
				timeZoneList.Add(timeZoneDim);
			}

			return timeZoneList;
		}

		public ITimeZoneDim GetTimeZoneDim(string timeZoneCode)
		{
			var timeZone = GetTimeZonesFromMart().SingleOrDefault(tz => tz.TimeZoneCode == timeZoneCode);
			if (timeZone != null)
			{
				return timeZone;
			}

			var timeZoneDim = new TimeZoneDim(TimeZoneInfo.FindSystemTimeZoneById(timeZoneCode), timeZoneCode == "UTC", false);

			var parameterList = new[]
			{
				new SqlParameter("time_zone_code", timeZoneDim.TimeZoneCode),
				new SqlParameter("time_zone_name", timeZoneDim.TimeZoneName),
				new SqlParameter("default_zone", timeZoneDim.IsDefaultTimeZone),
				new SqlParameter("utc_conversion", timeZoneDim.UtcConversion),
				new SqlParameter("utc_conversion_dst", timeZoneDim.UtcConversionDst)
			};

			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_time_zone_insert", parameterList,
				dataMartConnectionString);
			return GetTimeZonesFromMart().SingleOrDefault(tz => tz.TimeZoneCode == timeZoneCode);
		}

		public ITimeZoneDim DefaultTimeZone
		{
			get
			{
				var timeZones = GetTimeZonesFromMart();
				var utc = timeZones.FirstOrDefault(f => f.TimeZoneCode == "UTC");

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
			var parameterList = new[]
			{
				new SqlParameter("datasource_id", dataSourceId),
				new SqlParameter("time_zone_id", timeZoneId)
			};

			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_datasource_save", parameterList,
				dataMartConnectionString);
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_intraday_settings_load", null,
				dataMartConnectionString);
		}

		public void SetUtcTimeZoneOnRaptorDataSource()
		{
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.sys_datasource_set_raptor_time_zone", null,
				dataMartConnectionString);
		}

		public IBaseConfiguration LoadBaseConfiguration()
		{

			return _baseConfigurationRepository.LoadBaseConfiguration(dataMartConnectionString);
		}

		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			_baseConfigurationRepository.SaveBaseConfiguration(dataMartConnectionString, configuration);
		}

		public int LoadRowsInDimIntervalTable()
		{
			return (int) HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_dim_interval_check",
				dataMartConnectionString);
		}

		public void SetDataMartConnectionString(string connectionString)
		{
			dataMartConnectionString = connectionString;
		}

		public DateOnlyPeriod GetFactQueuePeriod(int dataSourceId)
		{
			return getFactTablePeriod(dataSourceId, "mart.fact_queue");
		}

		public DateOnlyPeriod GetFactAgentPeriod(int dataSourceId)
		{
			return getFactTablePeriod(dataSourceId, "mart.fact_agent");
		}

		private static T handleDbNullValue<T>(object value)
		{
			if (value == DBNull.Value)
				return default(T);

			return (T)value;
		}
		
		private DataSet getDatasources(bool getValidDatasources, bool includeOptionAll)
		{
			var parameterList = new[]
			{
				new SqlParameter("get_valid_datasource", getValidDatasources),
				new SqlParameter("include_option_all", includeOptionAll)
			};

			return HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.sys_get_datasources", parameterList,
				dataMartConnectionString);
		}

		private DateOnlyPeriod getFactTablePeriod(int dataSourceId, string tableName)
		{
			var sql
				= "SELECT MIN(d.date_date) as " + minDateColumn + ", MAX(d.date_date) as " + maxDateColumn + " "
				  + "FROM " + tableName + " f "
				  + "INNER JOIN mart.dim_date d ON f.date_id = d.date_id "
				  + "WHERE f.date_id > 0 "
				  + $"AND f.datasource_id = @{nameof(dataSourceId)}";

			var sqlParameters = new[] { new SqlParameter(nameof(dataSourceId), dataSourceId) };
			var dates = HelperFunctions.ExecuteDataSet(CommandType.Text, sql, sqlParameters,
				dataMartConnectionString);

			if (dates?.Tables[0] == null) return new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue);

			var dateRow = dates.Tables[0].Rows[0];
			var minDate = getDateOnlyValue(dateRow, minDateColumn);
			var maxDate = getDateOnlyValue(dateRow, maxDateColumn);
			return new DateOnlyPeriod(minDate, maxDate);
		}

		private DateOnly getDateOnlyValue(DataRow row, string columnName)
		{
			return DBNull.Value.Equals(row[columnName]) ? DateOnly.MinValue : new DateOnly((DateTime)row[columnName]);
		}
	}
}
