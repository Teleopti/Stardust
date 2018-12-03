using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class CommonReports : ICommonReports
	{
		private readonly Guid _reportId;
		private string _reportFileName;
		private string _url;
		private string _procedure;
		private string _sub1Name;
		private string _sub1ProcedureName;
		private string _sub2Name;
		private string _sub2ProcedureName;
		private string _name;
		private readonly SqlConnection _connection = new SqlConnection();
		private readonly int _dbTimeout;


		public CommonReports(string connectionString, Guid reportId)
		{
			_connection.ConnectionString = connectionString;
			_reportId = reportId;
			int dbTimeOutFromConfig;
			if (int.TryParse(ConfigurationManager.AppSettings["DbTimeoutForReports"], out dbTimeOutFromConfig))
				_dbTimeout = dbTimeOutFromConfig > 600
									 ? 600
									 : dbTimeOutFromConfig;
			else
				_dbTimeout = 180;
		}

		public int DbTimeout => _dbTimeout;

		public DataSet GetReportData(Guid personCode, Guid businessUnitCode, IList<SqlParameter> parameters)
		{
			LoadReportInfo();

			var preparedParameters = prepareReportParameters(personCode, businessUnitCode, parameters).ToList();
			DataSet mySet = ExecuteDataSet(_procedure, preparedParameters);
			mySet.Tables[0].TableName = _name;

			if (!string.IsNullOrEmpty(_sub1Name))
			{
				DataSet mySubSet = ExecuteDataSet(_sub1ProcedureName, preparedParameters);
				mySubSet.Tables[0].TableName = _sub1Name;
				mySet.Tables.Add(mySubSet.Tables[0]);
			}

			if (!string.IsNullOrEmpty(_sub2Name))
			{
				DataSet mySubSet2 = ExecuteDataSet(_sub2ProcedureName, preparedParameters);
				mySubSet2.Tables[0].TableName = _sub2Name;
				mySet.Tables.Add(mySubSet2.Tables[0]);
			}
			return mySet;
		}

		public string ReportFileName
		{
			get
			{
				if (string.IsNullOrEmpty(_reportFileName))
				{
					LoadReportInfo();
				}
				return _reportFileName;
			}
		}

		public Uri Url
		{
			get
			{
				if (string.IsNullOrEmpty(_url)) LoadReportInfo();
				return new Uri(_url, UriKind.Relative);
			}
		}

		private IEnumerable<SqlParameter> prepareReportParameters(Guid personCode, Guid businessUnitCode, IEnumerable<SqlParameter> parameters)
		{
			foreach (SqlParameter param in parameters)
			{
				if (param.ParameterName != "@report_type")
				{
					yield return param;
				}
			}

			yield return new SqlParameter("@person_code", personCode);
			yield return new SqlParameter("@report_id", _reportId);
			yield return new SqlParameter("@language_id", Thread.CurrentThread.CurrentUICulture.LCID);
			yield return new SqlParameter("@business_unit_code", businessUnitCode);
		}

		public void LoadReportInfo()
		{
			DataSet mySet = ExecuteDataSet("mart.report_info_get", new[] { new SqlParameter("@report_id", _reportId) });
			foreach (DataRow myRow in mySet.Tables[0].Rows)
			{
				_reportFileName = (string)myRow["rpt_file_name"];
				_url = (string)myRow["url"];
				_procedure = (string)myRow["proc_name"];
				_sub1Name = (string)myRow["sub1_name"];
				_sub1ProcedureName = (string)myRow["sub1_proc_name"];
				_sub2Name = (string)myRow["sub2_name"];
				_sub2ProcedureName = (string)myRow["sub2_proc_name"];
				_name = (string)myRow["report_name"];
				ResourceKey = (string)myRow["report_name_resource_key"];
				HelpKey = (string)myRow["help_key"];
			}
		}

		public string HelpKey { get; set; }

		public string ResourceKey { get; private set; }
		public string Name => _name;

		public DataSet ExecuteDataSet(string procedureName, IEnumerable<SqlParameter> parameters)
		{
			var returnValue = new DataSet { Locale = CultureInfo.CurrentCulture };
			var adapter = new SqlDataAdapter();
			var sqlCommand = new SqlCommand
			{
				CommandText = procedureName,
				CommandTimeout = _dbTimeout,
				CommandType = CommandType.StoredProcedure
			};

			sqlCommand.Parameters.AddRange(parameters.ToArray());
			sqlCommand.Connection = _connection;

			adapter.SelectCommand = sqlCommand;
			adapter.Fill(returnValue, "Data");
			sqlCommand.Parameters.Clear();
			_connection.Close();

			return returnValue;
		}

		public object ExecuteScalar(string commandText)
		{
			object retVal = default(object);
			if (_connection.State != ConnectionState.Open)
				_connection.Open();

			using (var tran = _connection.BeginTransaction())
			{
				using (var command = SetCommand(tran, commandText))
				{
					retVal = command.ExecuteScalar();
					tran.Commit();
				}
			}
			return retVal;
		}

		private SqlCommand SetCommand(SqlTransaction transaction, string commandText)
		{
			var timeout = "60";
			if (ConfigurationManager.AppSettings["databaseTimeout"] != null)
				timeout = ConfigurationManager.AppSettings["databaseTimeout"];
			return new SqlCommand
			{
				CommandText = commandText,
				Transaction = transaction,
				Connection = transaction.Connection,
				CommandType = CommandType.Text,
				CommandTimeout = int.Parse(timeout, CultureInfo.InvariantCulture)
			};
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool all)
		{
			_connection.Dispose();
		}

		public Guid GetGroupPageComboBoxControlCollectionId()
		{
			var returnValue = Guid.Empty;
			IList<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("@report_id", _reportId) };

			DataSet dataSet = ExecuteDataSet("mart.report_group_page_control_collection_get", parameters);
			if (dataSet != null && dataSet.Tables.Count == 1)
			{
				if (dataSet.Tables[0].Rows.Count == 1)
				{
					returnValue = (Guid)dataSet.Tables[0].Rows[0]["Id"];
				}
			}

			return returnValue;
		}
		
		public string GetReportPullDate(IList<SqlParameter> sqlParameters, TimeZoneInfo userTimeZone)
		{
			SqlParameter userPickedTimeZone = sqlParameters.SingleOrDefault(x => x.ParameterName == "@time_zone_id");
			if (userPickedTimeZone != null)
			{
				string timezoneId = (string)ExecuteScalar($"SELECT time_zone_code FROM mart.dim_time_zone WHERE time_zone_id = {userPickedTimeZone.Value}");
				if (!string.IsNullOrEmpty(timezoneId))
				{
					userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				}
			}
			
			return TimeZoneHelper.ConvertFromUtc(DateTime.UtcNow, userTimeZone).ToString();
		}
	}

	public interface ICommonReports : IDisposable
	{
		void LoadReportInfo();
		string ResourceKey { get; }
		string Name { get; }
		string HelpKey { get; }
	}
}
