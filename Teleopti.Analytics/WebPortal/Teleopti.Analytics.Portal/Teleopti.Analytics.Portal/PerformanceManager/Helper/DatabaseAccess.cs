using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Teleopti.Analytics.Portal.PerformanceManager.Helper
{
    public class DatabaseAccess : IDatabaseAccess
    {
        private readonly string _commandText;
        private readonly CommandType _commandType;
        private readonly string _connString;
        private bool _disposed;
        private readonly IList<IDbDataParameter> _procParam;
        // Fields
        private  IDbCommand _cmd;
        private IDbConnection _conn;

        public DatabaseAccess(CommandType commandType, string commandText, string connectionString)
            : this()
        {
            _connString = connectionString;
            _commandType = commandType;
            _commandText = commandText;
            //if (_commandType == CommandType.StoredProcedure)
            //    SetProcName();

        }

        private DatabaseAccess()
        {
            _procParam = new List<IDbDataParameter>();
        }

        ~DatabaseAccess()
        {
            Dispose(false);
        }

        protected virtual CommandBehavior CmdBehavior
        {
            get
            {
                return CommandBehavior.CloseConnection;
            }
        }

        public void AddProcParameter(IDbDataParameter parameter)
        {
            NotNull("parameter", parameter);

            if (_disposed)
            {
                throw new ObjectDisposedException("DatabaseAccess object disposed!");
            }
            if (((parameter.Direction == ParameterDirection.Input) && (parameter.Value == null)))
            {
                throw new ArgumentNullException("Missing " + parameter.ParameterName + " value for sproc " + _commandText);
            }
            _procParam.Add(parameter);
        }

        public void AddProcParameter(string parameterName, object value)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DatabaseAccess object disposed!");
            }
            AddProcParameter(new SqlParameter(parameterName, RuntimeHelpers.GetObjectValue(value)));
        }

        //private string buildConnectionString(string server, string db)
        //{
        //    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        //    builder.DataSource = server.Trim();
        //    builder.InitialCatalog = db.Trim();
        //    builder.IntegratedSecurity = false;
        //    builder.UserID = "toptiUser";
        //    builder.Password = "semtex315";
        //    return builder.ConnectionString;
        //}
        public void Close()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DatabaseAccess object disposed!");
            }
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDataReader ExecuteDataReader()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DatabaseAccess object disposed!");
            }
            //Trace.WriteLine("Prepare to get datareader - " + _commandText, "DatabaseAccess");
            GrabConnection();
            SetCommand();
            setParams();
            //Trace.WriteLine("Starting reading stream - " + _commandText, "DatabaseAccess");
            return _cmd.ExecuteReader(CmdBehavior);
        }

        public DataSet ExecuteDataSet()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DatabaseAccess object disposed!");
            }
            //Trace.WriteLine("Getting datatable - " + _commandText, "DatabaseAccess");
            IDbDataAdapter adapter = new SqlDataAdapter();
            DataSet dataSet = new DataSet();
            dataSet.Locale = Thread.CurrentThread.CurrentCulture;
            GrabConnection();
            SetCommand();
            setParams();
            adapter.SelectCommand = _cmd;
            adapter.Fill(dataSet);
            //Trace.WriteLine("Finished getting datatable - " + _commandText, "DatabaseAccess");
            return dataSet;
        }

        public int ExecuteNonQuery()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DatabaseAccess object disposed!");
            }
            //Trace.WriteLine("Executing command - " + _commandText, "DatabaseAccess");
            GrabConnection();
            SetCommand();
            setParams();
            int num2 = _cmd.ExecuteNonQuery();
            //Trace.WriteLine("Finished command - " + _commandText, "DatabaseAccess");
            return num2;
        }

        public object ExecuteScalar()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DatabaseAccess object disposed!");
            }
            //Trace.WriteLine("Executing scalar command - " + _commandText, "DatabaseAccess");
            GrabConnection();
            SetCommand();
            setParams();
            object retVal = _cmd.ExecuteScalar();
            //Trace.WriteLine("Finished executing scalar command - " + _commandText, "DatabaseAccess");
            return retVal;
        }

        protected virtual void Dispose(bool calledExplicit)
        {
            if (!_disposed && calledExplicit)
            {
                if (_cmd != null)
                {
                    _cmd.Cancel();
                }
                DisposeResources();
            }
            _disposed = true;
        }

        protected virtual void DisposeResources()
        {
            if (_cmd != null)
            {
                _cmd.Dispose();
            }
            if (_conn != null)
            {
                if (_conn.State != ConnectionState.Closed)
                {
                    _conn.Close();
                }
                _conn.Dispose();
            }
        }

        protected virtual void SetCommand()
        {
            _cmd = new SqlCommand();
            _cmd.CommandText = _commandText;
            _cmd.Connection = _conn;
            //_cmd.CommandType = CommandType.StoredProcedure;
            _cmd.CommandType = _commandType;
            //_cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["databaseTimeout"], CultureInfo.InvariantCulture);
        }

        //protected void SetProcName()
        //{
        //    if (!_commandText.StartsWith("dbo.",StringComparison.OrdinalIgnoreCase))
        //    {
        //        _commandText = "dbo." + _commandText;
        //    }
        //}

        private void GrabConnection()
        {
            _conn = new SqlConnection(_connString);
            _conn.Open();
        }

        private void setParams()
        {
            int num2 = _procParam.Count - 1;
            for (int i = 0; i <= num2; i++)
            {
                _cmd.Parameters.Add(_procParam[i]);
            }
        }

        /// <summary>
        /// Verifies that parameterValue is not null
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="parameterName">Name of the param.</param>
        public static void NotNull(string parameterName, object parameterValue)
        {
            if (parameterValue == null)
            {
                string errMess = string.Format(CultureInfo.CurrentCulture,
                                               "Parameter '{0}' must not be null.",
                                               parameterName);
                throw new ArgumentNullException(parameterName, errMess);
            }
        }
    }
}