using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public class DB : IDB
    {

		#region Fields (7) 

        private string _commandText;
        private readonly CommandType _commandType;
        private readonly string _connString;
        private bool _disposed;
        private readonly IList<IDbDataParameter> _procParam;
        // Fields
        private  IDbCommand cmd;
        private IDbConnection conn;

		#endregion Fields 

		#region Constructors (3) 

        public DB(CommandType commandType, string commandText, string connectionString)
            : this()
        {
            _connString = connectionString;
            _commandType = commandType;
            _commandText = commandText;
            //if (_commandType == CommandType.StoredProcedure)
            //    SetProcName();

        }

        // Methods
        private DB()
        {
            _procParam = new List<IDbDataParameter>();
        }

        ~DB()
        {
            Dispose(false);
        }

		#endregion Constructors 

		#region Properties (1) 

        // Properties
        protected virtual CommandBehavior CmdBehavior
        {
            get
            {
                return CommandBehavior.CloseConnection;
            }
        }

		#endregion Properties 

		#region Methods (14) 


		// Public Methods (8) 

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

        public void AddProcParameter(string parameterName, object value)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Db object disposed!");
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
                throw new ObjectDisposedException("Db object disposed!");
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
                throw new ObjectDisposedException("Db object disposed!");
            }
            //Trace.WriteLine("Prepare to get datareader - " + _commandText, "Db");
            GrabConnection();
            SetCommand();
            setParams();
            //Trace.WriteLine("Starting reading stream - " + _commandText, "Db");
            return cmd.ExecuteReader(CmdBehavior);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public DataSet ExecuteDataSet()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Db object disposed!");
            }
            //Trace.WriteLine("Getting datatable - " + _commandText, "Db");
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
                //dataSet = null;
                throw;
            }
            finally
            {
                if (adapter != null) ((IDisposable) adapter).Dispose();
            }
            
            //Trace.WriteLine("Finished getting datatable - " + _commandText, "Db");
            return dataSet;
        }

        public int ExecuteNonQuery()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Db object disposed!");
            }
            //Trace.WriteLine("Executing command - " + _commandText, "Db");
            GrabConnection();
            SetCommand();
            setParams();
            int num2 = cmd.ExecuteNonQuery();
            //Trace.WriteLine("Finished command - " + _commandText, "Db");
            return num2;
        }

        public object ExecuteScalar()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Db object disposed!");
            }
            //Trace.WriteLine("Executing scalar command - " + _commandText, "Db");
            GrabConnection();
            SetCommand();
            setParams();
            object retVal = cmd.ExecuteScalar();
            //Trace.WriteLine("Finished executing scalar command - " + _commandText, "Db");
            return retVal;
        }



		// Protected Methods (4) 

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
            //cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandType = _commandType;
            cmd.CommandTimeout = int.Parse(ConfigurationManager.AppSettings["databaseTimeout"], CultureInfo.InvariantCulture);
        }

        //protected void SetProcName()
        //{
        //    if (!_commandText.StartsWith("dbo.",StringComparison.OrdinalIgnoreCase))
        //    {
        //        _commandText = "dbo." + _commandText;
        //    }
        //}



		// Private Methods (2) 

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


		#endregion Methods 
    }


}
