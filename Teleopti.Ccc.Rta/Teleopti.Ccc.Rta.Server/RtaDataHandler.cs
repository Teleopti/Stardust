using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using log4net;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Rta.Server
{
	public class RtaDataHandler : IRtaDataHandler
	{
		private readonly IActualAgentHandler _agentHandler;
		private readonly IMessageSender _messageSender;
		private readonly string _connectionStringDataStore;
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private static ILog _loggingSvc;
		private readonly IDataSourceResolver _dataSourceResolver;
		private readonly IPersonResolver _personResolver;
		private readonly IStateResolver _stateResolver;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "1")]
		protected RtaDataHandler(ILog loggingSvc, IMessageSender messageSender, string connectionStringDataStore,
		                         IDatabaseConnectionFactory databaseConnectionFactory, IDataSourceResolver dataSourceResolver,
		                         IPersonResolver personResolver,
		                         IStateResolver stateResolver)
		{
			_loggingSvc = loggingSvc;
			_messageSender = messageSender;
			_connectionStringDataStore = connectionStringDataStore;
			_databaseConnectionFactory = databaseConnectionFactory;
			_dataSourceResolver = dataSourceResolver;
			_personResolver = personResolver;
			_stateResolver = stateResolver;

			if (_messageSender == null) return;

			try
			{
				_messageSender.InstantiateBrokerService();
			}
			catch (BrokerNotInstantiatedException ex)
			{
				_loggingSvc.Error(
					"The message broker will be unavailable until this service is restarted and initialized with correct parameters",
					ex);
			}
		}

		public RtaDataHandler(ILog loggingSvc, IMessageSender messageSender, string connectionStringDataStore,
		                      IDatabaseConnectionFactory databaseConnectionFactory, IDataSourceResolver dataSourceResolver,
		                      IPersonResolver personResolver, IStateResolver stateResolver, IActualAgentHandler agentHandler)
		{
			_loggingSvc = loggingSvc;
			_messageSender = messageSender;
			_connectionStringDataStore = connectionStringDataStore;
			_databaseConnectionFactory = databaseConnectionFactory;
			_dataSourceResolver = dataSourceResolver;
			_personResolver = personResolver;
			_agentHandler = agentHandler;
			_stateResolver = stateResolver;

			if (_messageSender == null) return;

			try
			{
				_messageSender.InstantiateBrokerService();
			}
			catch (BrokerNotInstantiatedException ex)
			{
				_loggingSvc.Error(
					"The message broker will be unavailable until this service is restarted and initialized with correct parameters",
					ex);
			}
		}


		public RtaDataHandler(IActualAgentHandler agentHandler)
			: this(
				LogManager.GetLogger(typeof (RtaDataHandler)),
				MessageSenderFactory.CreateMessageSender(ConfigurationManager.AppSettings["MessageBroker"]),
				ConfigurationManager.AppSettings["DataStore"], new DatabaseConnectionFactory(), null, null, null, null)
		{
			_agentHandler = agentHandler;

			_dataSourceResolver = new DataSourceResolver(_databaseConnectionFactory, _connectionStringDataStore);
			_personResolver = new PersonResolver(_databaseConnectionFactory, _connectionStringDataStore);
			_stateResolver = new StateResolver(_databaseConnectionFactory, _connectionStringDataStore);

			if (_messageSender == null) return;

			try
			{
				_messageSender.InstantiateBrokerService();
			}
			catch (BrokerNotInstantiatedException ex)
			{
				_loggingSvc.Error(
					"The message broker will be unavailable until this service is restarted and initialized with correct parameters",
					ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"
			)]
		public void ProcessScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			try
			{
				if (string.IsNullOrEmpty(_connectionStringDataStore))
				{
					_loggingSvc.Error("No connection information avaiable in configuration file.");
					return;
				}
				_agentHandler.InvalidateReadModelCache(personId);
				var agentState = _agentHandler.CheckSchedule(personId, businessUnitId, timestamp);
				if (agentState == null)
				{
					_loggingSvc.InfoFormat("Schedule for {0} has not changed", personId);
					return;
				}

				_loggingSvc.InfoFormat("Trying to send object {0} through Message Broker", agentState);
				_messageSender.SendRtaData(personId, businessUnitId, agentState);
			}
			catch (SocketException exception)
			{
				_loggingSvc.Error("The message broker seems to be down.", exception);
			}
			catch (BrokerNotInstantiatedException exception)
			{
				_loggingSvc.Error("The message broker seems to be down.", exception);
			}
			catch (Exception exception)
			{
				_loggingSvc.Error("An error occured while handling RTA-Event", exception);
			}
		}

		public bool IsAlive
		{
			get { return _messageSender.IsAlive; }
		}

		// Probably a WaitHandle object isnt a best choice, but same applies to QueueUserWorkItem method.
		// An alternative using Tasks should be looked at instead.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"
			)]
		public void ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp,
		                           Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
		{
			int dataSourceId;

			if (string.IsNullOrEmpty(_connectionStringDataStore))
			{
				_loggingSvc.Error("No connection information available in configuration file.");
				return;
			}

			if (!_dataSourceResolver.TryResolveId(sourceId, out dataSourceId))
			{
				_loggingSvc.WarnFormat(
					"No data source available for source id = {0}. Event will not be handled before data source is set up.", sourceId);
				return;
			}

			IEnumerable<PersonWithBusinessUnit> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, logOn, out personWithBusinessUnits))
			{
				_loggingSvc.InfoFormat(
					"No person available for datasource id = {0} and log on {1}. Event will not be handled before person is set up.",
					dataSourceId, logOn);
				return;
			}
			
			try
			{
				foreach (var personWithBusinessUnit in personWithBusinessUnits)
				{
					_loggingSvc.InfoFormat("ACD-Logon: {0} is connected to PersonId: {1}", logOn, personWithBusinessUnit.PersonId);
					if (!_stateResolver.HaveStateCodeChanged(personWithBusinessUnit.PersonId, stateCode, timestamp))
					{
						_loggingSvc.InfoFormat("Person {0} is already in state {1}", personWithBusinessUnit.PersonId, stateCode);
						continue;
					}


					var agentState = _agentHandler.GetAndSaveState(personWithBusinessUnit.PersonId,
					                                               personWithBusinessUnit.BusinessUnitId,
					                                               platformTypeId, stateCode,
					                                               timestamp, timeInState);
					if (agentState == null)
					{
						_loggingSvc.WarnFormat("Could not get state for Person {0}", personWithBusinessUnit.PersonId);
						continue;
					}
					_loggingSvc.InfoFormat("Sending AgentState: {0} through Message Broker", agentState);
					if (_messageSender.IsAlive)
						_messageSender.SendRtaData(personWithBusinessUnit.PersonId, personWithBusinessUnit.BusinessUnitId, agentState);
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
			catch (Exception exception)
			{
				_loggingSvc.Error("An error occured while handling RTA-Event", exception);
			}
		}
	}
}
