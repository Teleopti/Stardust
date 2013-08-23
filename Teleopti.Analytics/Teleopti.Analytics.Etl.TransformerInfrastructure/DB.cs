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
        private IDbCommand cmd;
        private IDbConnection conn;

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

        protected virtual CommandBehavior CmdBehavior
        {
            get
            {
                return CommandBehavior.CloseConnection;
            }
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
                adapter.SelectCommand = cmd;
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
            int num2 = cmd.ExecuteNonQuery();
            
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
            object retVal = cmd.ExecuteScalar();
            
			return retVal;
        }

        protected virtual void Dispose(bool calledExplicit)
        {
            if (!_disposed && calledExplicit)
            {
                if (cmd != null)
                {
                    cmd.Cancel();
                }
                DisposeResources();
            }
            _disposed = true;
        }

        protected virtual void DisposeResources()
        {
            if (cmd != null)
            {
                cmd.Dispose();
            }
            if (conn != null)
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
                conn.Dispose();
            }
        }

        protected virtual void SetCommand()
        {
            cmd = new SqlCommand();
            cmd.CommandText = _commandText;
            cmd.Connection = conn;
            cmd.CommandType = _commandType;
            cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["databaseTimeout"], CultureInfo.InvariantCulture);
        }

        private void GrabConnection()
        {
            conn = new SqlConnection(_connString);
            conn.Open();
        }

        private void setParams()
        {
            int num2 = _procParam.Count - 1;
            for (int i = 0; i <= num2; i++)
            {
                cmd.Parameters.Add(_procParam[i]);
            }
        }
    }
}
