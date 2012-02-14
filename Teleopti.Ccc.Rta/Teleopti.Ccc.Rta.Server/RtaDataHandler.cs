using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Net.Sockets;
using System.Threading;
using log4net;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Rta.Server
{
    public class RtaDataHandler : IRtaDataHandler
    {
        private readonly IMessageSender _messageSender;
        private readonly string _connectionStringDataStore;
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        private static ILog _loggingSvc;
        private readonly IDataSourceResolver _dataSourceResolver;
        private readonly IPersonResolver _personResolver;

        protected RtaDataHandler(ILog loggingSvc, IMessageSender messageSender, string connectionStringDataStore, IDatabaseConnectionFactory databaseConnectionFactory, IDataSourceResolver dataSourceResolver, IPersonResolver personResolver)
        {
            _loggingSvc = loggingSvc;
            _messageSender = messageSender;
            _connectionStringDataStore = connectionStringDataStore;
            _databaseConnectionFactory = databaseConnectionFactory;
            _dataSourceResolver = dataSourceResolver;
            _personResolver = personResolver;
            try
            {
                _messageSender.InstantiateBrokerService();
            }
            catch (BrokerNotInstantiatedException ex)
            {
                _loggingSvc.Error("The message broker will be unavailable until this service is restarted and initialized with correct parameters", ex);
            }
        }

        public RtaDataHandler()
            : this(LogManager.GetLogger(typeof(RtaDataHandler)), MessageSenderFactory.CreateMessageSender(ConfigurationManager.AppSettings["MessageBroker"]), ConfigurationManager.AppSettings["DataStore"], new DatabaseConnectionFactory(), null, null)
        {
            _dataSourceResolver = new DataSourceResolver(_databaseConnectionFactory,_connectionStringDataStore);
            _personResolver = new PersonResolver(_databaseConnectionFactory,_connectionStringDataStore);
        }

        public bool IsAlive
        {
            get { return _messageSender.IsAlive; }
        }

        #region IRTADataHandler Members

		// Probably a WaitHandle object isnt a best choice, but same applies to QueueUserWorkItem method.
		// An alternative using Tasks should be looked at instead.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public WaitHandle ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
        {
            int dataSourceId;
			var waitHandle = new AutoResetEvent(false);
            if (!_dataSourceResolver.TryResolveId(sourceId, out dataSourceId))
            {
                _loggingSvc.ErrorFormat("No data source available for source id = {0}. Event will not be handled before data source is set up.", sourceId);
            	waitHandle.Set();
				return waitHandle;
            }

		    bool sendWithBroker = true;
		    IEnumerable<Guid> personIdList;
		    if (!_personResolver.TryResolveId(dataSourceId,logOn,out personIdList))
		    {
                _loggingSvc.WarnFormat("No person available for datasource id = {0} and log on {1}. Event will not be sent through message broker before person is set up.", dataSourceId, logOn);
		        sendWithBroker = false;
		    }

            IExternalAgentState agentState = new ExternalAgentState(logOn, stateCode, timeInState, timestamp, platformTypeId, dataSourceId, batchId, isSnapshot);
            if (_messageSender.IsAlive && sendWithBroker)
            {
                _loggingSvc.InfoFormat("Trying to send object {0} through Message Broker", agentState);
                try
                {
                    foreach (var personId in personIdList)
                    {
                        _messageSender.SendRtaData(personId, agentState);
                    }
                }
                catch (SocketException exception)
                {
                    _loggingSvc.Error("The message broker seems to be down.", exception);
                }
                catch (BrokerNotInstantiatedException exception)
                {
                    _loggingSvc.Error("The message broker seems to be down.", exception);
                }
            }
            if (string.IsNullOrEmpty(_connectionStringDataStore))
            {
                _loggingSvc.Warn("No connection information available in configuration file.");
				waitHandle.Set();
				return waitHandle;
			}
            ThreadPool.QueueUserWorkItem(saveToDataStore, new object [] {agentState, waitHandle});
        	return waitHandle;
        }

        private void saveToDataStore(object args)
        {
        	var argsArr = args as object[];
			var agentState = argsArr[0] as IExternalAgentState;
			var waitHandle = argsArr[1] as AutoResetEvent;
			if (agentState == null)
			{
				waitHandle.Set();
				return;
			}
            try
            {
                using (IDbConnection connection = _databaseConnectionFactory.CreateConnection(_connectionStringDataStore))
                {
                    IDbCommand command = connection.CreateCommand();

                    IDbDataParameter dataParameter = command.CreateParameter();
                    dataParameter.ParameterName = "@LogOn";
                    dataParameter.Value = agentState.ExternalLogOn;
                    dataParameter.Direction = ParameterDirection.Input;
                    dataParameter.DbType = DbType.String;
                    dataParameter.Size = 50;
                    command.Parameters.Add(dataParameter);

                    dataParameter = command.CreateParameter();
                    dataParameter.ParameterName = "@StateCode";
                    dataParameter.Value = agentState.StateCode;
                    dataParameter.Direction = ParameterDirection.Input;
                    dataParameter.DbType = DbType.String;
                    dataParameter.Size = 50;
                    command.Parameters.Add(dataParameter);

                    dataParameter = command.CreateParameter();
                    dataParameter.ParameterName = "@TimeInState";
                    dataParameter.Value = agentState.TimeInState.Ticks;
                    dataParameter.Direction = ParameterDirection.Input;
                    dataParameter.DbType = DbType.Int64;
                    command.Parameters.Add(dataParameter);

                    if (agentState.Timestamp < SqlDateTime.MinValue.Value || agentState.Timestamp > SqlDateTime.MaxValue.Value)
                    {
                        _loggingSvc.ErrorFormat("The provided timestamp was invalid, further processing of this message will be canceled. The timestamp was {0}.", agentState.Timestamp);
						waitHandle.Set();
						return;
                    }

                    dataParameter = command.CreateParameter();
                    dataParameter.ParameterName = "@Timestamp";
                    dataParameter.Value = agentState.Timestamp;
                    dataParameter.Direction = ParameterDirection.Input;
                    dataParameter.DbType = DbType.DateTime;
                    command.Parameters.Add(dataParameter);

                    dataParameter = command.CreateParameter();
                    dataParameter.ParameterName = "@PlatformTypeId";
                    dataParameter.Value = agentState.PlatformTypeId;
                    dataParameter.Direction = ParameterDirection.Input;
                    dataParameter.DbType = DbType.Guid;
                    command.Parameters.Add(dataParameter);

                    dataParameter = command.CreateParameter();
                    dataParameter.ParameterName = "@DataSourceId";
                    dataParameter.Value = agentState.DataSourceId;
                    dataParameter.Direction = ParameterDirection.Input;
                    dataParameter.DbType = DbType.Int32;
                    command.Parameters.Add(dataParameter);

                    DateTime batchId = agentState.BatchId;
                    if (batchId < SqlDateTime.MinValue.Value || batchId > SqlDateTime.MaxValue.Value)
                    {
                        _loggingSvc.WarnFormat(
                            "Batch id was invalid and the timestamp value is used instead. Provided date time was {0}.",
                            batchId);
                        batchId = agentState.Timestamp;
                    }

                    dataParameter = command.CreateParameter();
                    dataParameter.ParameterName = "@BatchId";
                    dataParameter.Value = batchId;
                    dataParameter.Direction = ParameterDirection.Input;
                    dataParameter.DbType = DbType.DateTime;
                    command.Parameters.Add(dataParameter);

                    dataParameter = command.CreateParameter();
                    dataParameter.ParameterName = "@IsSnapshot";
                    dataParameter.Direction = ParameterDirection.Input;
                    dataParameter.Value = agentState.IsSnapshot;
                    dataParameter.DbType = DbType.Boolean;
                    command.Parameters.Add(dataParameter);

                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "RTA.rta_insert_agentstate";

                    connection.Open();
                    _loggingSvc.InfoFormat("Inserted item in database. Database: {0}, Insertion result: {1}",
                                           connection.Database, command.ExecuteNonQuery());
                }
            }
            catch(DbException ex)
            {
                _loggingSvc.Error("Sql error occured", ex);
            }
        	waitHandle.Set();
        }

        #endregion
    }
}