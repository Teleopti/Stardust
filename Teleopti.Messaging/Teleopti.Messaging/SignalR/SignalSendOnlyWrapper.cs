using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Exceptions;
using log4net;
using Subscription = Teleopti.Interfaces.MessageBroker.Subscription;

namespace Teleopti.Messaging.SignalR
{
	public class SignalSendOnlyWrapper : ISignalWrapper
	{
		private readonly IHubProxy _hubProxy;
		private readonly HubConnection _hubConnection;
		private int _retryCount;

		public static readonly ILog Logger = LogManager.GetLogger(typeof(SignalSendOnlyWrapper));
		private static readonly object LockObject = new object();

		public SignalSendOnlyWrapper(IHubProxy hubProxy, HubConnection hubConnection)
		{
			_hubProxy = hubProxy;
			_hubConnection = hubConnection;
		}

		public Task NotifyClients(Notification notification)
		{
			if (verifyStillConnected())
			{
				try
				{
					var startTask = _hubProxy.Invoke("NotifyClients", notification);
					startTask.ContinueWith(t =>
						{
							if (t.IsFaulted && t.Exception != null)
								Logger.Error("An error happened when notifying", t.Exception.GetBaseException());
						}, TaskContinuationOptions.OnlyOnFaulted);
					return startTask;
				}
				catch (InvalidOperationException exception)
				{
					Logger.Error("An error happened when notifycing.", exception);
				}
			}
			return emptyTask();
		}


		private static Task emptyTask()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetResult(null);
			return tcs.Task;
		}

		private bool verifyStillConnected()
		{
			if (_retryCount > 3)
				return false;
			lock (LockObject)
			{
				if (_hubConnection.State != ConnectionState.Connected)
				{
					try
					{
						startHubConnection();
						_retryCount = 0;
						return true;
					}
					catch (Exception exception)
					{
						Logger.Error("An error happened when verifyting that we still are connected.", exception);
						_retryCount++;
						return false;
					}
				}
			}
			return true;
		}

		private void startHubConnection()
		{
			try
			{
				Exception exception = null;
				_hubConnection.Credentials = CredentialCache.DefaultNetworkCredentials;
				var startTask = _hubConnection.Start();
				startTask.ContinueWith(t =>
				{
					if (!t.IsFaulted || t.Exception == null) return;

					exception = t.Exception.GetBaseException();
					Logger.Error("An error happened when starting hub connection.", exception);
				}, TaskContinuationOptions.OnlyOnFaulted);

				if (!startTask.Wait(TimeSpan.FromSeconds(30)))
				{
					exception = new InvalidOperationException("Could not start message broker client within 30 seconds.");
				}
				if (exception != null)
				{
					throw exception;
				}
			}
			catch (AggregateException aggregateException)
			{
				Logger.Error("An error happened when starting hub connection.", aggregateException);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", aggregateException);
			}
			catch (SocketException socketException)
			{
				Logger.Error("An error happened when starting hub connection.", socketException);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", socketException);
			}
			catch (InvalidOperationException exception)
			{
				Logger.Error("An error happened when starting hub connection.", exception);
				throw new BrokerNotInstantiatedException("Could not start the SignalR message broker.", exception);
			}
		}

		public void StopListening()
		{
			if (_hubConnection != null && _hubConnection.State == ConnectionState.Connected)
			{
				try
				{
					_hubConnection.Stop();
				}
				catch (Exception exception)
				{
					Logger.Error("An error happened when stopping connection.", exception);
				}
			}

		}

		public bool IsInitialized()
		{
			// Ska man ha en _isRunning? Den verkar ju mest subscriba till grejer
			return _hubProxy != null;
		}

		// Byta namn på denna? I SingnalSender lyssnar den ju, men här startar den bara
		public void StartListening()
		{
			startHubConnection();
		}

		public Task NotifyClients(IEnumerable<Notification> notification)
		{
			throw new NotImplementedException();
		}

		public Task AddSubscription(Subscription subscription)
		{

			throw new NotImplementedException();
		}

		public Task RemoveSubscription(string route)
		{

			throw new NotImplementedException();
		}

		public event Action<Notification> OnNotification;
	}
}
