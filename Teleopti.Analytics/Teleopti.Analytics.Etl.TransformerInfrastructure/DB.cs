using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public class DB : IDB
    {
        private readonly string _commandText;
        private readonly CommandType _commandType;
        private readonly string _connString;
        private bool _disposed;
        private readonly IList<IDbDataParameter> _procParam;
        private IDbCommand _cmd;
        private IDbConnection _conn;

        public DB(CommandType commandType, string commandText, string connectionString)
            : this()
        {
            _connString = connectionString;
            _commandType = commandType;
            _commandText = commandText;
        }

        private DB()
        {
            _procParam = new List<IDbDataParameter>();
        }

        ~DB()
        {
            Dispose(false);
        }
        
        public void AddProcParameter(IDbDataParameter parameter)
        {
            InParameter.NotNull("parameter", parameter);

            if (_disposed)
            {
                throw new ObjectDisposedException("Db object disposed!");
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public DataSet ExecuteDataSet()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Db object disposed!");
            }
            
			IDbDataAdapter adapter = null;
            DataSet dataSet = new DataSet();
            try
            {
                adapter = new SqlDataAdapter();
                dataSet.Locale = Thread.CurrentThread.CurrentCulture;
                GrabConnection();
                SetCommand();
                setParams();
                adapter.SelectCommand = _cmd;
                adapter.Fill(dataSet);
            }
            catch (Exception)
            {
                dataSet.Dispose();
                throw;
            }
            finally
            {
                if (adapter != null) ((IDisposable) adapter).Dispose();
            }
            
            return dataSet;
        }

        public int ExecuteNonQuery()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Db object disposed!");
            }
            
			GrabConnection();
            SetCommand();
            setParams();
            int num2 = _cmd.ExecuteNonQuery();
            
			return num2;
        }

        public object ExecuteScalar()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Db object disposed!");
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
            _cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["databaseTimeout"], CultureInfo.InvariantCulture);
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
    }
}
