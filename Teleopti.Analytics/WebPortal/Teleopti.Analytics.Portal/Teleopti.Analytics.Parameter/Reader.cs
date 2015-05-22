using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Analytics.Parameters
{
	public class Reader
	{
		private readonly int _langId;
		private readonly int _dbTimeout;
		private readonly SqlConnection _connection = new SqlConnection();

		public Reader(string connectionString, int langId, int dbTimeout)
		{
			_langId = langId;
			_dbTimeout = dbTimeout;
			_connection.ConnectionString = connectionString;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public DataSet LoadReportControls(Guid reportId, Guid groupPageIndex)
		{
			var ret = new DataSet();

			var adap = new SqlDataAdapter();
			var cmdType = new SqlCommand
			{
				CommandText = "mart.report_controls_get",
				CommandTimeout = _dbTimeout,
				CommandType = CommandType.StoredProcedure,
				Connection = _connection
			};

			cmdType.Parameters.AddWithValue("@report_id", reportId);
			cmdType.Parameters.AddWithValue("@group_page_code", groupPageIndex);

			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
			adap.SelectCommand = cmdType;
			adap.Fill(ret, "Controls");
			_connection.Close();
			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public DataTable ReportProperties(Guid reportId, int savedId)
		{
			var ret = new DataSet();
			var adap = new SqlDataAdapter();
			var cmdType = new SqlCommand
									{
										CommandText = "mart.report_description_get",
										CommandType = CommandType.StoredProcedure,
										CommandTimeout = _dbTimeout,
										Connection = _connection
									};

			cmdType.Parameters.AddWithValue("@report_id", reportId);
			cmdType.Parameters.AddWithValue("@saved_name_id", savedId);

			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}

			adap.SelectCommand = cmdType;
			adap.Fill(ret, "Props");
			_connection.Close();
			return ret.Tables[0];
		}



		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "params"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "bu"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public DataSet LoadControlData(string proc, IList<SqlParameter> @params, Guid componentId, Guid personCode, Guid buCode)
		{
			var ret = new DataSet();
			var adap = new SqlDataAdapter();
			using (var cmdType = new SqlCommand())
			{
				cmdType.CommandText = proc;
				cmdType.CommandType = CommandType.StoredProcedure;
				cmdType.CommandTimeout = _dbTimeout;
				foreach (SqlParameter param in @params)
				{
					cmdType.Parameters.Add(param);
				}

				cmdType.Parameters.AddWithValue("@report_id", componentId);
				cmdType.Parameters.AddWithValue("@person_code", personCode);
				cmdType.Parameters.AddWithValue("@language_id", _langId);
				cmdType.Parameters.AddWithValue("@bu_id", buCode);

				cmdType.CommandType = CommandType.StoredProcedure;
				cmdType.Connection = _connection;
				if (_connection.State != ConnectionState.Open)
				{
					_connection.Open();
				}

				adap.SelectCommand = cmdType;
			}

			adap.Fill(ret, "Controls");
			_connection.Close();
			adap.Dispose();
			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public string LoadUserSetting(Guid componentId, Guid personCode, string parameter, int savedId)
		{
			var cmd = new SqlCommand
			{
				CommandText = "mart.report_user_setting_get",
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = _dbTimeout,
				Connection = _connection
			};

			cmd.Parameters.AddWithValue("@param_name", parameter);
			cmd.Parameters.AddWithValue("@report_id", componentId);
			cmd.Parameters.AddWithValue("@person_code", personCode);
			cmd.Parameters.AddWithValue("@saved_name_id", savedId);

			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
			object objret = cmd.ExecuteScalar();
			string ret = objret == null ? "" : objret.ToString();
			_connection.Close();

			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void SaveUserSetting(Guid componentId, Guid personCode, string parameter, int savedId, string setting)
		{
			var cmd = new SqlCommand
			{
				CommandText = "mart.report_user_setting_save",
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = _dbTimeout,
				Connection = _connection
			};

			cmd.Parameters.AddWithValue("@person_code", personCode);
			cmd.Parameters.AddWithValue("@report_id", componentId);
			cmd.Parameters.AddWithValue("@param_name", parameter);
			cmd.Parameters.AddWithValue("@saved_name_id", savedId);
			cmd.Parameters.AddWithValue("@setting", setting);

			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
			cmd.ExecuteNonQuery();
			_connection.Close();
		}

		public bool IsReportPermissionsGranted(Guid reportId, Guid userId)
		{
			bool returnValue = false;
			using (var cmd = new SqlCommand())
			{
				cmd.CommandText = "mart.report_permission_get";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@report_id", reportId);
				cmd.Parameters.AddWithValue("@person_code", userId);

				cmd.Connection = _connection;
				if (_connection.State != ConnectionState.Open)
				{
					_connection.Open();
				}
				object scalarValue = cmd.ExecuteScalar();
				if (scalarValue != null)
				{
					if ((int)scalarValue > 0)
						returnValue = true;
				}
			}

			_connection.Close();

			return returnValue;
		}


	}
}
