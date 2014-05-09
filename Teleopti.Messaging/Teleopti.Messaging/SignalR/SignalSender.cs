﻿using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;
using log4net;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSender : IMessageSender
	{
		private readonly string _serverUrl;
		private IHandleHubConnection _connection;
		private ISignalBrokerCommands _signalBrokerCommands;
		protected ILog Logger;

		public SignalSender(string serverUrl)
		{
			_serverUrl = serverUrl;

			ServicePointManager.ServerCertificateValidationCallback = IgnoreInvalidCertificate;
			ServicePointManager.DefaultConnectionLimit = 50;

			TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
			Logger = MakeLogger();
		}

		protected static bool IgnoreInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		protected void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (e.Observed) return;
			Logger.Debug("An unobserved task failed.", e.Exception);
			e.SetObserved();
		}

		public void StartBrokerService()
		{
			StartBrokerService(TimeSpan.FromSeconds(240));
		}

		public void StartBrokerService(int reconnectAttempts)
		{
			StartBrokerService(TimeSpan.FromSeconds(20), 3);
		}

		public void StartBrokerService(TimeSpan reconnectDelay, int reconnectAttempts = 0)
		{
			if (string.IsNullOrEmpty(_serverUrl))
				return;
			try
			{
				if (_connection == null)
				{
					var connection = new SignalConnection(MakeHubConnection, Logger, reconnectDelay, reconnectAttempts);
					_signalBrokerCommands = new SignalBrokerCommands(Logger, connection);
					_connection = connection;
				}

				_connection.StartConnection();
			}
			catch (SocketException exception)
			{
				Logger.Error("The message broker seems to be down.", exception);
			}
			catch (BrokerNotInstantiatedException exception)
			{
				Logger.Error("The message broker seems to be down.", exception);
			}
		}

		public bool IsAlive
		{
			get { return _connection != null && _connection.IsConnected(); }
		}

		public void SendNotification(Notification notification)
		{
			_signalBrokerCommands.NotifyClients(notification);
		}

		public virtual void Dispose()
		{
			_connection.CloseConnection();
			_connection = null;
		}

		protected virtual ILog MakeLogger()
		{
			return LogManager.GetLogger(typeof(SignalSender));
		}

		[CLSCompliant(false)]
		protected virtual IHubConnectionWrapper MakeHubConnection()
		{
			return new HubConnectionWrapper(new HubConnection(_serverUrl));
		}

	}
}