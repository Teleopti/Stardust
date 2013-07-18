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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public object ExecuteScalar()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DatabaseAccess object disposed!");
            }
            GrabConnection();
            SetCommand();
            setParams();
            object retVal = _cmd.ExecuteScalar();
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
            _cmd.CommandType = _commandType;
        }

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