using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Analytics.Parameters 
{
	/// <summary>
	/// Summary description for Reader.
	/// </summary>
	public class Reader 
	{
		//string SelectString;
		//string _ConnString;
        private readonly int _langId;
		//string SourceTable;
        private readonly SqlConnection _connection = new SqlConnection();
        //IList<SqlParameter> SelectParameters = new List<SqlParameter>();

		public Reader(string connectionString, int langId)
		{
            _langId = langId;
			_connection.ConnectionString = connectionString;
		}

		public DataSet LoadReportControls(Guid reportId, Guid groupPageIndex)
		{
//			if (Disposed == true)
//			{
//				throw New ObjectDisposedException("LoadComponentControls");
//			}
			var ret = new DataSet();
			//string cult = (string)  HttpContext.Current.Session["culture"];

			var adap = new SqlDataAdapter();
			var cmdType = new SqlCommand {CommandText = "mart.report_controls_get"};

		    cmdType.Parameters.AddWithValue("@report_id", reportId);
            cmdType.Parameters.AddWithValue("@group_page_code", groupPageIndex);
            

			cmdType.CommandType = CommandType.StoredProcedure;
			cmdType.Connection = _connection;
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
			adap.SelectCommand = cmdType;
			adap.Fill(ret,"Controls");
			_connection.Close();
			return ret;		
		}

        public DataTable ReportProperties(Guid reportId, int savedId)
        {
            var ret = new DataSet();
            var adap = new SqlDataAdapter();
            var cmdType = new SqlCommand
                              {
                                  CommandText = "mart.report_description_get",
                                  CommandType = CommandType.StoredProcedure
                              };

            cmdType.Parameters.AddWithValue("@report_id", reportId);
            cmdType.Parameters.AddWithValue("@saved_name_id", savedId);
            //CmdType.Parameters.AddWithValue("@culture", _culture);

            cmdType.CommandType = CommandType.StoredProcedure;
            cmdType.Connection = _connection;
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            adap.SelectCommand = cmdType;
            adap.Fill(ret, "Props");
            _connection.Close();
            return ret.Tables[0];
        }



        public DataSet LoadControlData(string proc, IList<SqlParameter> @params, Guid componentId, Guid personCode, Guid buCode)
		{
//			SqlParameter param;
//			if (Disposed == true)
//			{
//				Throw New ObjectDisposedException("LoadControlData");
//			}

			var ret = new DataSet();
			var adap = new SqlDataAdapter();
			var cmdType = new SqlCommand {CommandText = proc, CommandType = CommandType.StoredProcedure};

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
			adap.Fill(ret,"Controls");
			_connection.Close();
			return ret;
		}

		public string LoadUserSetting(Guid componentId, Guid personCode, string parameter, int savedId)
		{
//			SqlParameter param;
//			if (Disposed == true)
//			{
//				Throw New ObjectDisposedException("LoadControlData");
//			}
		    var cmd = new SqlCommand {CommandText = "mart.report_user_setting_get", CommandType = CommandType.StoredProcedure};

		    cmd.Parameters.AddWithValue("@param_name", parameter);
            cmd.Parameters.AddWithValue("@report_id", componentId);
            cmd.Parameters.AddWithValue("@person_code", personCode);
            cmd.Parameters.AddWithValue("@saved_name_id", savedId);
			
			cmd.Connection = _connection;
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
			object objret = cmd.ExecuteScalar();
			string ret = objret == null ? "" : objret.ToString();
			_connection.Close();
		
			return ret;
		}

		public int SaveNewName(string name, int id, Guid personCode)
		{
			var cmd = new SqlCommand {CommandText = "p_webx2_save_setting_name", CommandType = CommandType.StoredProcedure};

		    cmd.Parameters.AddWithValue("@person_code", personCode);
            cmd.Parameters.AddWithValue("@name", name);
            SqlParameter p = cmd.Parameters.AddWithValue("@id", id);
			p.Direction = ParameterDirection.InputOutput;

			cmd.Connection = _connection;
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
			cmd.ExecuteNonQuery();
			_connection.Close();

			return (int) p.Value;
		}

		public void DeleteSavedReport(int id)
		{
		var cmd = new SqlCommand {CommandText = "p_webx2_delete_saved", CommandType = CommandType.StoredProcedure};

		    cmd.Parameters.AddWithValue("@id", id);
			cmd.Connection = _connection;
			if (_connection.State != ConnectionState.Open)
			{
				_connection.Open();
			}
			cmd.ExecuteNonQuery();
			_connection.Close();
		}

		public void SaveUserSetting(Guid componentId, Guid personCode, string parameter, int savedId, string setting)
		{
			//			SqlParameter param;
//			if (Disposed == true)
//			{
//				Throw New ObjectDisposedException("LoadControlData");
//			}
			var cmd = new SqlCommand {CommandText = "mart.report_user_setting_save", CommandType = CommandType.StoredProcedure};

		    cmd.Parameters.AddWithValue("@person_code", personCode);
            cmd.Parameters.AddWithValue("@report_id", componentId);
            cmd.Parameters.AddWithValue("@param_name", parameter);
            cmd.Parameters.AddWithValue("@saved_name_id", savedId);
            cmd.Parameters.AddWithValue("@setting", setting);
			cmd.Connection = _connection;
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
            var cmd = new SqlCommand
                          {
                              CommandText = "mart.report_permission_get",
                              CommandType = CommandType.StoredProcedure
                          };

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
            _connection.Close();

            return returnValue;
        }


	}
}
