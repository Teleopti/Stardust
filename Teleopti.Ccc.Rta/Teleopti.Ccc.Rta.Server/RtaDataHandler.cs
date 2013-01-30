using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.Threading;
using log4net;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Rta.Server
{
	public class RtaDataHandler : IRtaDataHandler
	{
		private readonly IRtaConsumer _rtaConsumer;
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
		                      IPersonResolver personResolver, IStateResolver stateResolver, IRtaConsumer rtaConsumer)
		{
			_loggingSvc = loggingSvc;
			_messageSender = messageSender;
			_connectionStringDataStore = connectionStringDataStore;
			_databaseConnectionFactory = databaseConnectionFactory;
			_dataSourceResolver = dataSourceResolver;
			_personResolver = personResolver;
			_stateResolver = stateResolver;
			_rtaConsumer = rtaConsumer;

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

		public RtaDataHandler(IRtaConsumer rtaConsumer)
			: this(
				LogManager.GetLogger(typeof (RtaDataHandler)),
				MessageSenderFactory.CreateMessageSender(ConfigurationManager.AppSettings["MessageBroker"]),
				ConfigurationManager.AppSettings["DataStore"], new DatabaseConnectionFactory(), null, null, null)
		{
			_rtaConsumer = rtaConsumer;

			_dataSourceResolver = new DataSourceResolver(_databaseConnectionFactory, _connectionStringDataStore);
			_personResolver = new PersonResolver(_databaseConnectionFactory, _connectionStringDataStore);
			_stateResolver = new StateResolver(_databaseConnectionFactory, _connectionStringDataStore);
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

		public bool IsAlive
		{
			get { return _messageSender.IsAlive; }
		}

		#region IRTADataHandler Members

		// Probably a WaitHandle object isnt a best choice, but same applies to QueueUserWorkItem method.
		// An alternative using Tasks should be looked at instead.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"
			)]
		public WaitHandle ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp,
		                                 Guid platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
		{
			int dataSourceId;
			var waitHandle = new AutoResetEvent(false);

			if (string.IsNullOrEmpty(_connectionStringDataStore))
			{
				_loggingSvc.Warn("No connection information available in configuration file.");
				waitHandle.Set();
				return waitHandle;
			}

			if (!_dataSourceResolver.TryResolveId(sourceId, out dataSourceId))
			{
				_loggingSvc.ErrorFormat(
					"No data source available for source id = {0}. Event will not be handled before data source is set up.", sourceId);
				waitHandle.Set();
				return waitHandle;
			}

			IEnumerable<PersonWithBusinessUnit> personWithBusinessUnits;
			if (!_personResolver.TryResolveId(dataSourceId, logOn, out personWithBusinessUnits))
			{
				_loggingSvc.WarnFormat(
					"No person available for datasource id = {0} and log on {1}. Event will not be sent through message broker before person is set up.",
					dataSourceId, logOn);
				waitHandle.Set();
				return waitHandle;
			}

			if (_messageSender.IsAlive)
			{
				try
				{
					foreach (var personWithBusinessUnit in personWithBusinessUnits)
					{
						if (!_stateResolver.HaveStateCodeChanged(personWithBusinessUnit.PersonId, stateCode))
						{
							_loggingSvc.InfoFormat("Person {0} is already in state {1}", personWithBusinessUnit.PersonId, stateCode);
							continue;
						}

						var agentState = _rtaConsumer.Consume(personWithBusinessUnit.PersonId, personWithBusinessUnit.BusinessUnitId,
						                                      platformTypeId, stateCode,
						                                      timestamp, timeInState, waitHandle);
						if (agentState == null) continue;
						_loggingSvc.InfoFormat("Trying to send object {0} through Message Broker", agentState);
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
			}
			return waitHandle;
		}
		#endregion
	}
}
