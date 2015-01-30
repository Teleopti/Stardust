using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Globalization;

// TODO Move to another project
namespace Teleopti.Analytics.Portal.Utils
{
    public class CommonReports : IDisposable
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
        

        public CommonReports(string connectionString,Guid reportId)
        {
	        _connection.ConnectionString = connectionString;
	        _reportId = reportId;
	        int dbTimeOutFromConfig;
	        if (int.TryParse(ConfigurationManager.AppSettings["DbTimeout"], out dbTimeOutFromConfig))
		        _dbTimeout = dbTimeOutFromConfig > 600
			                     ? 600
			                     : dbTimeOutFromConfig;
	        else
		        _dbTimeout = 180;
        }

	    public DataSet GetReportData(Guid reportId, Guid personCode, Guid businessUnitCode, IList<SqlParameter> parameters)
        {
            LoadReportInfo(reportId);

            var preparedParameters = prepareReportParameters(reportId, personCode, businessUnitCode, parameters);
            DataSet mySet = ExecuteDataSet(_procedure, preparedParameters);
            DataSet mySubSet;
            DataSet mySubSet2;
            mySet.Tables[0].TableName = _name;

            if (!string.IsNullOrEmpty(_sub1Name))
            {
                mySubSet = ExecuteDataSet(_sub1ProcedureName, preparedParameters);
                mySubSet.Tables[0].TableName = _sub1Name;
                mySet.Tables.Add(mySubSet.Tables[0]);
            }

            if (!string.IsNullOrEmpty(_sub2Name))
            {
                mySubSet2 = ExecuteDataSet(_sub2ProcedureName, preparedParameters);
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
                    LoadReportInfo(_reportId);
                }
                return _reportFileName;
            }
        }

        public Uri Url
        {
            get
            {
                if (string.IsNullOrEmpty(_url)) LoadReportInfo(_reportId);
                return new Uri(_url,UriKind.Relative);
            }
        }

        private static IEnumerable<SqlParameter> prepareReportParameters(Guid reportId, Guid personCode, Guid businessUnitCode, IEnumerable<SqlParameter> parameters)
        {
            foreach (SqlParameter param in parameters)
            {
                if (param.ParameterName != "@report_type")
                {
                    yield return param;
                }
            }

            yield return new SqlParameter("@person_code",personCode);
            yield return new SqlParameter("@report_id", reportId);
            yield return new SqlParameter("@language_id", Thread.CurrentThread.CurrentUICulture.LCID);
            yield return new SqlParameter("@business_unit_code", businessUnitCode);
        }

        public void LoadReportInfo(Guid reportId)
        {
            DataSet mySet = ExecuteDataSet("mart.report_info_get", new[] {new SqlParameter("@report_id", reportId)});
            foreach (DataRow myRow in mySet.Tables[0].Rows)
            {
                _reportFileName = myRow.Field<string>("rpt_file_name");
                _url = myRow.Field<string>("url");
                _procedure = myRow.Field<string>("proc_name");
                _sub1Name = myRow.Field<string>("sub1_name");
                _sub1ProcedureName = myRow.Field<string>("sub1_proc_name");
                _sub2Name = myRow.Field<string>("sub2_name");
                _sub2ProcedureName = myRow.Field<string>("sub2_proc_name");
                _name = myRow.Field<string>("report_name");
            }
        }

	    public DataSet ExecuteDataSet(string procedureName, IEnumerable<SqlParameter> parameters)
	    {
		    var returnValue = new DataSet {Locale = CultureInfo.CurrentCulture};
		    var adapter = new SqlDataAdapter();
		    var sqlCommand = new SqlCommand
			    {
				    CommandText = procedureName,
					CommandTimeout = _dbTimeout,
				    CommandType = CommandType.StoredProcedure
			    };

		    sqlCommand.Parameters.AddRange(parameters.ToArray());
		    sqlCommand.Connection = _connection;

		    if (_connection.State != ConnectionState.Open)
			    _connection.Open();

		    adapter.SelectCommand = sqlCommand;
		    adapter.Fill(returnValue, "Data");
		    sqlCommand.Parameters.Clear();
		    _connection.Close();

		    return returnValue;
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
            IList<SqlParameter> parameters = new List<SqlParameter> {new SqlParameter("@report_id", _reportId)};

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
    }
}
